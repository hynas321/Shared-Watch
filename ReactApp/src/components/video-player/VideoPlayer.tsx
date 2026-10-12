import ReactPlayer from "react-player";
import { ChangeEvent, useCallback, useContext, useEffect, useRef, useState } from "react";
import {
  BsArrowsFullscreen,
  BsPauseFill,
  BsPlayFill,
  BsVolumeDownFill,
  BsVolumeMuteFill,
  BsVolumeUpFill,
} from "react-icons/bs";
import { AppHubContext, AppStateContext } from "../../context/AppContext";
import * as signalR from "@microsoft/signalr";
import { HubMessages } from "../../classes/constants/HubMessages";
import { VideoPlayer as PlaybackSnapshot } from "../../types/VideoPlayer";
import { UserPermissions } from "../../types/UserPermissions";
import NoVideoPlaceholder from "./NoVideoPlaceholder";
import { getPlayableVideoUrl } from "../../classes/helpers/VideoUrl";
import { isYouTubeVideoLink } from "../../classes/helpers/RoomUrl";
import api from "../../classes/Http/Api";

const SEEK_CORRECTION_THRESHOLD_SECONDS = 3;
const YOUTUBE_CORRECTION_THRESHOLD_SECONDS = 4;
const SEEK_CORRECTION_COOLDOWN_MS = 1_500;
const SEEK_CONFIRMATION_WINDOW_MS = 2_000;
const SEEK_DEBOUNCE_MS = 120;
const SEEK_INTERACTION_WINDOW_MS = 2_000;
const PROGRAMMATIC_PLAYBACK_EVENT_WINDOW_MS = 500;
const PLAYBACK_COMMAND_CONFIRMATION_WINDOW_MS = 2_000;
const SNAPSHOT_REFRESH_INTERVAL_MS = 15_000;

const formatPlaybackTime = (seconds: number): string => {
  if (!Number.isFinite(seconds) || seconds < 0) {
    return "0:00";
  }

  const totalSeconds = Math.floor(seconds);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const remainingSeconds = totalSeconds % 60;

  return hours > 0
    ? `${hours}:${String(minutes).padStart(2, "0")}:${String(remainingSeconds).padStart(2, "0")}`
    : `${minutes}:${String(remainingSeconds).padStart(2, "0")}`;
};

export default function VideoPlayer() {
  const appState = useContext(AppStateContext);
  const appHub = useContext(AppHubContext);
  const [hubState, setHubState] = useState(appHub.getState());
  const videoPlayerRef = useRef<ReactPlayer>(null);
  const playerSurfaceRef = useRef<HTMLDivElement>(null);
  const serverClockOffsetMs = useRef<number | null>(null);
  const latestSnapshotTimestamp = useRef(0);
  const latestSnapshotRevision = useRef(-1);
  const lastAuthoritativeTime = useRef(0);
  const programmaticSeekTarget = useRef<number | null>(null);
  const pendingSeek = useRef<{ seconds: number; requestedAt: number } | null>(null);
  const queuedSeekRequestedAt = useRef<number | null>(null);
  const pendingPlaybackCommand = useRef<{ playing: boolean; requestedAt: number } | null>(null);
  const playbackCommandChain = useRef<Promise<void>>(Promise.resolve());
  const seekTimer = useRef<number | undefined>(undefined);
  const seekingUntil = useRef(0);
  const lastCorrectionAt = useRef(0);
  const authoritativePlaying = useRef(appState.videoPlayer.value?.isPlaying ?? false);
  const currentVideoHash = useRef(appState.videoPlayer.value?.playlistVideo?.hash ?? null);
  const endedVideoHash = useRef<string | null>(null);
  const suppressPlaybackEventsUntil = useRef(0);
  const [videoUrl, setVideoUrl] = useState<string | null>(
    appState.videoPlayer.value?.playlistVideo?.url ?? null
  );
  const [playableVideoUrl, setPlayableVideoUrl] = useState<string | null>(null);
  const [isMobileView, setIsMobileView] = useState(false);
  const [isVideoPlaying, setIsVideoPlaying] = useState(
    appState.videoPlayer.value?.isPlaying ?? false
  );
  const [videoDuration, setVideoDuration] = useState(0);
  const [playedSeconds, setPlayedSeconds] = useState(0);
  const displayedTime = useRef(0);
  const displayClock = useRef({ time: 0, updatedAt: Date.now(), playing: false });
  const [volume, setVolume] = useState(1);
  const [muted, setMuted] = useState(false);
  const [permissionSnapshot, setPermissionSnapshot] = useState<UserPermissions | null>(
    appState.userPermissions.value
  );
  const [isAdmin, setIsAdmin] = useState(appState.isAdmin.value);
  const effectivePermissions = permissionSnapshot ?? appState.userPermissions.value;
  const canStartOrPauseVideo = isAdmin || effectivePermissions?.canStartOrPauseVideo === true;
  const canChangeVideoTime = isAdmin || effectivePermissions?.canSkipVideo === true;

  const enqueuePlaybackCommand = (command: () => Promise<unknown>) => {
    const next = playbackCommandChain.current.then(() => command()).then(() => undefined);
    playbackCommandChain.current = next.catch(() => undefined);
    return next;
  };

  useEffect(() => {
    const handlePermissionsChanged = (permissions: UserPermissions) => {
      setPermissionSnapshot(permissions);
    };
    const handleAdminStatusChanged = (user: { username: string; isAdmin?: boolean }) => {
      if (user.username === appState.username.value) {
        setIsAdmin(user.isAdmin === true);
      }
    };

    appHub.on(HubMessages.OnSetUserPermissions, handlePermissionsChanged);
    appHub.on(HubMessages.OnSetAdminStatus, handleAdminStatusChanged);
    return () => {
      appHub.off(HubMessages.OnSetUserPermissions, handlePermissionsChanged);
      appHub.off(HubMessages.OnSetAdminStatus, handleAdminStatusChanged);
    };
  }, [appHub, appState.username]);

  const enforceAuthoritativePlayback = useCallback(() => {
    const internalPlayer = videoPlayerRef.current?.getInternalPlayer() as
      | (HTMLMediaElement & {
          playVideo?: () => void;
          pauseVideo?: () => void;
          getPlayerState?: () => number;
        })
      | undefined;
    if (!internalPlayer) {
      return;
    }

    suppressPlaybackEventsUntil.current = Date.now() + PROGRAMMATIC_PLAYBACK_EVENT_WINDOW_MS;

    if (authoritativePlaying.current) {
      if (typeof internalPlayer.playVideo === "function") {
        const playerState = internalPlayer.getPlayerState?.();
        if (playerState !== 1 && playerState !== 3) {
          internalPlayer.playVideo();
        }
      } else if (internalPlayer.paused) {
        void internalPlayer.play().catch(() => undefined);
      }
    } else if (typeof internalPlayer.pauseVideo === "function") {
      const playerState = internalPlayer.getPlayerState?.();
      if (playerState !== 2 && playerState !== 0) {
        internalPlayer.pauseVideo();
      }
    } else if (!internalPlayer.paused) {
      internalPlayer.pause();
    }
  }, []);

  useEffect(() => {
    if (!videoUrl) {
      setPlayableVideoUrl(null);
      return;
    }

    const uploadedVideoMatch = videoUrl.match(/^\/api\/VideoFiles\/([^/]+)\/([^/?#]+)$/);
    if (!uploadedVideoMatch) {
      setPlayableVideoUrl(videoUrl);
      return;
    }

    let isCurrent = true;
    setPlayableVideoUrl(null);
    void api.getVideoStreamUrl(
      decodeURIComponent(appState.roomHash.value),
      decodeURIComponent(uploadedVideoMatch[2])
    )
      .then((url) => {
        if (isCurrent) {
          setPlayableVideoUrl(url);
        }
      })
      .catch((error: unknown) => {
        if (isCurrent) {
          console.warn("Unable to authorize uploaded video playback.", error);
        }
      });

    return () => {
      isCurrent = false;
    };
  }, [appState.roomHash.value, videoUrl]);

  const applySnapshot = useCallback((snapshot: PlaybackSnapshot | null) => {
    if (!snapshot) {
      return;
    }
    if (snapshot.revision !== undefined && snapshot.revision < latestSnapshotRevision.current) {
      return;
    }
    if (snapshot.serverTimestampUnixMs && snapshot.serverTimestampUnixMs < latestSnapshotTimestamp.current) {
      return;
    }

    const pending = pendingSeek.current;
    let seekConfirmed = false;
    if (pending) {
      const age = Date.now() - pending.requestedAt;
      const snapshotTime = snapshot.currentTime ?? 0;
      if (age <= SEEK_CONFIRMATION_WINDOW_MS &&
          Math.abs(snapshotTime - pending.seconds) > SEEK_CORRECTION_THRESHOLD_SECONDS) {
        return;
      }

      seekConfirmed = age <= SEEK_CONFIRMATION_WINDOW_MS &&
        Math.abs(snapshotTime - pending.seconds) <= SEEK_CORRECTION_THRESHOLD_SECONDS;
      pendingSeek.current = null;
    }

    const pendingPlayback = pendingPlaybackCommand.current;
    if (pendingPlayback) {
      const age = Date.now() - pendingPlayback.requestedAt;
      if (age <= PLAYBACK_COMMAND_CONFIRMATION_WINDOW_MS &&
          snapshot.isPlaying !== pendingPlayback.playing) {
        return;
      }

      pendingPlaybackCommand.current = null;
    }
    latestSnapshotRevision.current = snapshot.revision ?? latestSnapshotRevision.current;
    latestSnapshotTimestamp.current = snapshot.serverTimestampUnixMs ?? latestSnapshotTimestamp.current;

    const nextUrl = snapshot.playlistVideo?.url ?? null;
    const nextHash = snapshot.playlistVideo?.hash ?? null;
    if (nextHash !== currentVideoHash.current) {
      currentVideoHash.current = nextHash;
      endedVideoHash.current = null;
    }
    const isInitialPlaybackStart = snapshot.isPlaying &&
      snapshot.currentTime <= 0.1 &&
      (nextUrl !== videoUrl || lastAuthoritativeTime.current <= 0.1);
    const serverNow = Date.now() + (serverClockOffsetMs.current ?? 0);
    const transitSeconds = !seekConfirmed && !isInitialPlaybackStart &&
      snapshot.isPlaying && snapshot.serverTimestampUnixMs
      ? Math.max(0, serverNow - snapshot.serverTimestampUnixMs) / 1000
      : 0;
    const targetTime = Math.max(0, snapshot.currentTime + transitSeconds);
    lastAuthoritativeTime.current = targetTime;

    appState.videoPlayer.value = {
      ...snapshot,
      currentTime: targetTime,
    };
    authoritativePlaying.current = snapshot.isPlaying;
    setIsVideoPlaying(snapshot.isPlaying);
    const displayTolerance = isYouTubeVideoLink(nextUrl ?? "")
      ? YOUTUBE_CORRECTION_THRESHOLD_SECONDS
      : SEEK_CORRECTION_THRESHOLD_SECONDS;
    const shouldEaseCorrection = snapshot.isPlaying && !seekConfirmed &&
      nextUrl === videoUrl && Math.abs(targetTime - displayedTime.current) <= displayTolerance;
    const displayTime = shouldEaseCorrection ? displayedTime.current : targetTime;
    displayedTime.current = displayTime;
    displayClock.current = {
      time: displayTime,
      updatedAt: Date.now(),
      playing: snapshot.isPlaying,
    };
    setPlayedSeconds(displayTime);

    if (nextUrl !== videoUrl) {
      setVideoUrl(nextUrl);
      return;
    }

    const player = videoPlayerRef.current;
    if (player && nextUrl) {
      const localTime = player.getCurrentTime();
      const correctionThreshold = isYouTubeVideoLink(nextUrl)
        ? YOUTUBE_CORRECTION_THRESHOLD_SECONDS
        : SEEK_CORRECTION_THRESHOLD_SECONDS;
      const now = Date.now();
      const canCorrect = now >= seekingUntil.current &&
        now - lastCorrectionAt.current >= SEEK_CORRECTION_COOLDOWN_MS;
      const internalPlayer = player.getInternalPlayer() as
        | { getPlayerState?: () => number }
        | undefined;
      const isYouTubeBuffering = isYouTubeVideoLink(nextUrl) &&
        internalPlayer?.getPlayerState?.() === 3;

      if (canCorrect && !isYouTubeBuffering && Math.abs(localTime - targetTime) > correctionThreshold) {
        lastCorrectionAt.current = now;
        seekingUntil.current = Date.now() + SEEK_INTERACTION_WINDOW_MS;
        programmaticSeekTarget.current = targetTime;
        player.seekTo(targetTime, "seconds");
        window.setTimeout(() => {
          if (programmaticSeekTarget.current === targetTime) {
            programmaticSeekTarget.current = null;
          }
        }, 1_000);
      }

      window.setTimeout(enforceAuthoritativePlayback, 200);
    }
  }, [appState, enforceAuthoritativePlayback, videoUrl]);

  const refreshSnapshot = useCallback(async () => {
    if (appHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    const sentAt = Date.now();
    try {
      const snapshot = await appHub.invoke(
        HubMessages.GetPlaybackSnapshot,
        appState.roomHash.value
      ) as PlaybackSnapshot | null;
      const receivedAt = Date.now();

      if (snapshot?.serverTimestampUnixMs) {
        serverClockOffsetMs.current = snapshot.serverTimestampUnixMs - (sentAt + receivedAt) / 2;
      }
      applySnapshot(snapshot);
    } catch (error) {
      console.warn("Unable to refresh the shared playback state.", error);
    }
  }, [appHub, appState, applySnapshot]);

  useEffect(() => {
    if (hubState !== signalR.HubConnectionState.Connected) {
      return;
    }

    appHub.on(HubMessages.OnPlaybackSync, applySnapshot);
    void refreshSnapshot();
    const interval = window.setInterval(() => void refreshSnapshot(), SNAPSHOT_REFRESH_INTERVAL_MS);

    return () => {
      appHub.off(HubMessages.OnPlaybackSync, applySnapshot);
      window.clearInterval(interval);
    };
  }, [appHub, applySnapshot, hubState, refreshSnapshot]);

  useEffect(() => {
    const interval = window.setInterval(() => {
      const clock = displayClock.current;
      if (!clock.playing || pendingSeek.current) {
        return;
      }

      const nextTime = clock.time + (Date.now() - clock.updatedAt) / 1000;
      displayedTime.current = nextTime;
      setPlayedSeconds(nextTime);
    }, 100);

    return () => window.clearInterval(interval);
  }, []);

  useEffect(() => {
    const refreshHubState = () => setHubState(appHub.getState());
    const unsubscribe = appHub.onStateChanged(refreshHubState);
    refreshHubState();
    return unsubscribe;
  }, [appHub]);

  const handleSeek = (seconds: number) => {
    if (!canChangeVideoTime) {
      seekingUntil.current = Date.now() + SEEK_INTERACTION_WINDOW_MS;
      programmaticSeekTarget.current = lastAuthoritativeTime.current;
      videoPlayerRef.current?.seekTo(lastAuthoritativeTime.current, "seconds");
      return;
    }

    if (programmaticSeekTarget.current !== null &&
        Math.abs(programmaticSeekTarget.current - seconds) < SEEK_CORRECTION_THRESHOLD_SECONDS) {
      programmaticSeekTarget.current = null;
      return;
    }

    const requestedAt = Date.now();
    pendingSeek.current = { seconds, requestedAt };
    seekingUntil.current = requestedAt + SEEK_CONFIRMATION_WINDOW_MS;
    suppressPlaybackEventsUntil.current = requestedAt + SEEK_INTERACTION_WINDOW_MS;

    if (seekTimer.current !== undefined) {
      window.clearTimeout(seekTimer.current);
    }

    seekTimer.current = window.setTimeout(() => {
      seekTimer.current = undefined;
      const pending = pendingSeek.current;
      if (!pending || pending.requestedAt !== requestedAt) {
        return;
      }

      queuedSeekRequestedAt.current = requestedAt;

      void enqueuePlaybackCommand(() =>
        appHub.invoke(HubMessages.SetPlayedSeconds, appState.roomHash.value, pending.seconds)
      ).catch((error: unknown) => {
        pendingSeek.current = null;
        console.warn("Unable to seek shared playback.", error);
      });
    }, SEEK_DEBOUNCE_MS);
  };

  const setUserVideoState = (playing: boolean, force = false) => {
    if (!canStartOrPauseVideo) {
      return;
    }

    if (!force && (Date.now() < seekingUntil.current || Date.now() < suppressPlaybackEventsUntil.current)) {
      return;
    }

    const pending = pendingPlaybackCommand.current;
    if (!force && pending && Date.now() - pending.requestedAt <= PLAYBACK_COMMAND_CONFIRMATION_WINDOW_MS &&
        pending.playing !== playing) {
      return;
    }

    authoritativePlaying.current = playing;
    setIsVideoPlaying(playing);
    pendingPlaybackCommand.current = { playing, requestedAt: Date.now() };

    suppressPlaybackEventsUntil.current = Date.now() + PROGRAMMATIC_PLAYBACK_EVENT_WINDOW_MS;
    const internalPlayer = videoPlayerRef.current?.getInternalPlayer() as
      | (HTMLMediaElement & { playVideo?: () => void; pauseVideo?: () => void })
      | undefined;
    if (internalPlayer) {
      if (playing) {
        if (typeof internalPlayer.playVideo === "function") {
          internalPlayer.playVideo();
        } else {
          void internalPlayer.play().catch(() => undefined);
        }
      } else if (typeof internalPlayer.pauseVideo === "function") {
        internalPlayer.pauseVideo();
      } else {
        internalPlayer.pause();
      }
    }

    if (seekTimer.current !== undefined) {
      window.clearTimeout(seekTimer.current);
      seekTimer.current = undefined;
    }
    const pendingSeekCommand = pendingSeek.current &&
      queuedSeekRequestedAt.current !== pendingSeek.current.requestedAt
      ? pendingSeek.current
      : null;
    void enqueuePlaybackCommand(async () => {
      if (pendingSeekCommand) {
        await appHub.invoke(HubMessages.SetPlayedSeconds, appState.roomHash.value, pendingSeekCommand.seconds);
      }
      await appHub.invoke(HubMessages.SetIsVideoPlaying, appState.roomHash.value, playing);
    }).catch((error: unknown) => console.warn("Unable to update shared playback.", error));
  };

  const reportVideoEnded = useCallback((videoHash: string | null | undefined) => {
    if (!videoHash) {
      return;
    }
    if (endedVideoHash.current === videoHash) {
      return;
    }

    endedVideoHash.current = videoHash;
    void appHub.invoke(HubMessages.NotifyVideoEnded, appState.roomHash.value, videoHash)
      .catch((error: unknown) => {
        if (endedVideoHash.current === videoHash) {
          endedVideoHash.current = null;
        }
        console.warn("Unable to advance the shared playlist.", error);
      });
  }, [appHub, appState.roomHash]);

  const notifyVideoEnded = () => reportVideoEnded(currentVideoHash.current);

  const setLocalVolume = (value: number) => {
    const nextVolume = Math.max(0, Math.min(1, value));
    const internalPlayer = videoPlayerRef.current?.getInternalPlayer() as
      | (HTMLMediaElement & { setVolume?: (value: number) => void; mute?: () => void; unMute?: () => void })
      | undefined;
    setVolume(nextVolume);
    setMuted(nextVolume === 0);
    if (!internalPlayer) {
      return;
    }
    if (typeof internalPlayer.setVolume === "function") {
      internalPlayer.setVolume(nextVolume * 100);
      if (nextVolume === 0) internalPlayer.mute?.();
      else internalPlayer.unMute?.();
    } else {
      internalPlayer.volume = nextVolume;
      internalPlayer.muted = nextVolume === 0;
    }
  };

  const toggleMute = () => {
    const nextMuted = !muted;
    const internalPlayer = videoPlayerRef.current?.getInternalPlayer() as
      | (HTMLMediaElement & { mute?: () => void; unMute?: () => void })
      | undefined;
    setMuted(nextMuted);
    if (typeof internalPlayer?.mute === "function") {
      if (nextMuted) internalPlayer.mute();
      else internalPlayer.unMute?.();
    } else if (internalPlayer) {
      internalPlayer.muted = nextMuted;
    }
  };

  const handleTimeControlChange = (event: ChangeEvent<HTMLInputElement>) => {
    const seconds = Number(event.target.value);
    displayedTime.current = seconds;
    displayClock.current = {
      time: seconds,
      updatedAt: Date.now(),
      playing: authoritativePlaying.current,
    };
    setPlayedSeconds(seconds);

    if (!canChangeVideoTime) {
      handleSeek(seconds);
      return;
    }

    handleSeek(seconds);
    programmaticSeekTarget.current = seconds;
    suppressPlaybackEventsUntil.current = Date.now() + SEEK_INTERACTION_WINDOW_MS;
    videoPlayerRef.current?.seekTo(seconds, "seconds");
  };

  const toggleFullscreen = () => {
    if (document.fullscreenElement) {
      void document.exitFullscreen();
    } else {
      void playerSurfaceRef.current?.requestFullscreen();
    }
  };

  useEffect(() => {
    const handleWindowResize = () => setIsMobileView(window.innerWidth <= 576);
    handleWindowResize();
    window.addEventListener("resize", handleWindowResize);
    return () => window.removeEventListener("resize", handleWindowResize);
  }, []);

  useEffect(() => () => {
    if (seekTimer.current !== undefined) {
      window.clearTimeout(seekTimer.current);
    }
  }, []);

  return (
    <>
      <div className="room-video-top-bar rounded-top-5 bg-dark bg-opacity-50 text-center" />
      <div ref={playerSurfaceRef} className="room-video-player-surface d-flex justify-content-center bg-dark bg-opacity-50">
        {videoUrl && playableVideoUrl ? (
          <>
            <ReactPlayer
              key={videoUrl}
              ref={videoPlayerRef}
              url={getPlayableVideoUrl(playableVideoUrl)}
              controls={false}
              width="100%"
              height="100%"
              style={{ objectFit: "cover", aspectRatio: "16/9" }}
              onReady={() => void refreshSnapshot()}
              onDuration={setVideoDuration}
              onProgress={(state: { playedSeconds: number }) => {
                if (videoDuration > 0 && state.playedSeconds >= videoDuration - 0.25) {
                  reportVideoEnded(currentVideoHash.current);
                }
              }}
              onEnded={notifyVideoEnded}
            />
            <button
              type="button"
              className={`room-video-click-surface${canStartOrPauseVideo ? "" : " room-video-click-surface-disabled"}`}
              aria-label={isVideoPlaying ? "Pause video" : "Play video"}
              aria-disabled={!canStartOrPauseVideo}
              onClick={() => {
                if (canStartOrPauseVideo) {
                  setUserVideoState(!isVideoPlaying, true);
                }
              }}
            />
          </>
        ) : (
            <NoVideoPlaceholder isMobileView={isMobileView} />
          )}
      </div>
      {videoUrl && playableVideoUrl && (
        <div className="room-video-controls rounded-bottom-5 bg-dark bg-opacity-50 px-3 text-white">
          <button
            type="button"
            className="room-video-control-button"
            aria-label={isVideoPlaying ? "Pause video" : "Play video"}
            title={isVideoPlaying ? "Pause video" : "Play video"}
            disabled={!canStartOrPauseVideo}
            onClick={() => setUserVideoState(!isVideoPlaying, true)}
          >
            {isVideoPlaying ? <BsPauseFill aria-hidden="true" /> : <BsPlayFill aria-hidden="true" />}
          </button>
          <span className="room-video-time-label">{formatPlaybackTime(playedSeconds)}</span>
          <input
            className="room-video-time-range"
            type="range"
            min={0}
            max={Math.max(videoDuration, 0)}
            step={0.1}
            value={Math.min(playedSeconds, Math.max(videoDuration, 0))}
            disabled={!canChangeVideoTime || videoDuration <= 0}
            aria-label="Video time"
            onChange={handleTimeControlChange}
          />
          <span className="room-video-time-label">{formatPlaybackTime(videoDuration)}</span>
          <button type="button" className="room-video-control-button" aria-label={muted ? "Unmute video" : "Mute video"} onClick={toggleMute}>
            {muted || volume === 0 ? <BsVolumeMuteFill aria-hidden="true" /> : volume < 0.5 ? <BsVolumeDownFill aria-hidden="true" /> : <BsVolumeUpFill aria-hidden="true" />}
          </button>
          <input
            className="room-video-volume-range"
            type="range"
            min={0}
            max={1}
            step={0.01}
            value={muted ? 0 : volume}
            aria-label="Volume"
            onChange={(event) => setLocalVolume(Number(event.target.value))}
          />
          <button type="button" className="room-video-control-button" aria-label="Fullscreen" title="Fullscreen" onClick={toggleFullscreen}>
            <BsArrowsFullscreen aria-hidden="true" />
          </button>
        </div>
      )}
    </>
  );
}
