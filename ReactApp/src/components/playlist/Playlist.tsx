import { BsCollectionPlayFill, BsFileEarmarkPlayFill, BsPlusCircleFill, BsYoutube } from "react-icons/bs";
import { ChangeEvent, useContext, useEffect, useRef, useState } from "react";
import { AppHubContext, AppStateContext } from "../../context/AppContext";
import { HubMessages } from "../../classes/constants/HubMessages";
import { InputField } from "../shared/InputField";
import Button from "../shared/Button";
import PlaylistVideoItem from "./PlaylistVideoItem";
import { PlaylistVideo } from "../../types/PlaylistVideo";
import { isYouTubeVideoLink } from "../../classes/helpers/RoomUrl";
import api from "../../classes/Http/Api";
import { HttpStatusCode } from "axios";
import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";

type VideoSource = "youtube" | "file";
const MAX_FILE_BYTES = 500 * 1024 * 1024;

const isSamePlaylistVideo = (left: PlaylistVideo, right: PlaylistVideo): boolean =>
  left.hash != null && right.hash != null
    ? left.hash === right.hash
    : left.url === right.url;

const readVideoDuration = (file: File): Promise<number> => new Promise((resolve, reject) => {
  const video = document.createElement("video");
  const objectUrl = URL.createObjectURL(file);
  const cleanup = () => {
    URL.revokeObjectURL(objectUrl);
    video.removeAttribute("src");
    video.load();
  };
  video.preload = "metadata";
  video.onloadedmetadata = () => {
    const duration = video.duration;
    cleanup();
    if (Number.isFinite(duration) && duration > 0) {
      resolve(duration);
    }
    else reject(new Error("The video's duration could not be read."));
  };
  video.onerror = () => {
    cleanup();
    reject(new Error("This video could not be opened by the browser."));
  };
  video.src = objectUrl;
});

export default function Playlist() {
  const appState = useContext(AppStateContext);
  const appHub = useContext(AppHubContext);
  const playlistVideosRef = useRef<HTMLDivElement>(null);

  const [source, setSource] = useState<VideoSource>("youtube");
  const [videoUrl, setVideoUrl] = useState("");
  const [videoFile, setVideoFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [uploadMessage, setUploadMessage] = useState("");
  const [isEnabled, setIsEnabled] = useState(true);
  const [placeholder, setPlaceholder] = useState("Paste YouTube video URL");
  const [youtubeApiUnavailable, setYoutubeApiUnavailable] = useState(false);
  const [draggedVideoHash, setDraggedVideoHash] = useState<string | null>(null);
  const [dragOverVideoHash, setDragOverVideoHash] = useState<string | null>(null);
  const canReorderVideos = appState.isAdmin.value;

  const blockYouTubeApi = () => {
    setYoutubeApiUnavailable(true);
    setSource("file");
    toast.error("The YouTube API is unavailable. YouTube videos have been disabled.", {
      containerId: ToastNotificationEnum.Room,
      autoClose: 5000,
    });
  };

  useEffect(() => {
    if (playlistVideosRef.current) {
      playlistVideosRef.current.scrollTop = playlistVideosRef.current.scrollHeight;
    }
  }, [appState.playlistVideos.value]);

  const handleAddVideo = async () => {
    if (!videoUrl.trim()) {
      return;
    }

    if (!isYouTubeVideoLink(videoUrl)) {
      setIsEnabled(false);
      setPlaceholder("Incorrect Youtube Video URL");
      setTimeout(() => {
        setIsEnabled(true);
        setPlaceholder("Paste YouTube video URL");
      }, 1500);
      setVideoUrl("");
      return;
    }

    const newVideo: PlaylistVideo = { url: videoUrl, duration: 5 };
    try {
      const result = await appHub.invoke<string | PlaylistVideo>(
        HubMessages.AddPlaylistVideo,
        appState.roomHash.value,
        newVideo
      );
      if (typeof result === "object" && result !== null) {
        const addedVideo = result as PlaylistVideo;
        if (!appState.playlistVideos.value.some((video) => isSamePlaylistVideo(video, addedVideo))) {
          appState.playlistVideos.value = [...appState.playlistVideos.value, addedVideo];
        }
      } else if (result === "VideoUnavailable") {
        const healthStatus = await api.healthCheck();
        if (healthStatus === HttpStatusCode.Ok) {
          blockYouTubeApi();
        } else {
          setUploadMessage("The server is unavailable. Please rejoin the room and try again.");
        }
      } else if (result === "PlaylistFull") {
        setUploadMessage("The playlist already has 10 videos.");
      } else if (result === "AlreadyExists") {
        setUploadMessage("This video is already in the playlist.");
      } else if (result === "Forbidden") {
        setUploadMessage("You do not have permission to add videos.");
      } else {
        setUploadMessage("The video could not be added to the playlist.");
      }
    } catch {
      const healthStatus = await api.healthCheck();
      if (healthStatus === HttpStatusCode.Ok) {
        blockYouTubeApi();
      } else {
        setUploadMessage("The server is unavailable. Please rejoin the room and try again.");
      }
    }
    setVideoUrl("");
  };

  const handleFileSelection = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0] ?? null;
    setVideoFile(file);
    setUploadMessage("");
    if (file && file.size > MAX_FILE_BYTES) {
      setVideoFile(null);
      setUploadMessage("Choose a video smaller than 500 MB.");
      event.target.value = "";
    }
  };

  const handleUpload = async () => {
    if (!videoFile || isUploading) {
      return;
    }
    setIsUploading(true);
    setUploadProgress(0);
    setUploadMessage("Reading video…");
    try {
      const duration = await readVideoDuration(videoFile);
      setUploadMessage("Uploading…");
      const [status, uploadedVideo, serverMessage] = await api.uploadVideo(
        appState.roomHash.value,
        videoFile,
        duration,
        setUploadProgress
      );
      if (status === 200) {
        setVideoFile(null);
        if (uploadedVideo && !appState.playlistVideos.value.some((video) => isSamePlaylistVideo(video, uploadedVideo))) {
          appState.playlistVideos.value = [...appState.playlistVideos.value, uploadedVideo];
        }
        setUploadMessage("Added to the playlist.");
      } else if (status === 0) {
        setUploadMessage(serverMessage || "Upload failed because the server connection was lost. Check your connection and try again.");
      } else if (status === 401) {
        setUploadMessage("The server rejected upload authorization (401). Please retry; if it continues, rejoin the room.");
      } else if (status === 409) {
        setUploadMessage("The playlist already has 10 videos.");
      } else if (status === 403) {
        setUploadMessage("You don't have permission to add videos.");
      } else if (status === 413) {
        setUploadMessage("The video is too large. Choose a file smaller than 500 MB.");
      } else if (status === 400) {
        setUploadMessage(serverMessage || "The video is invalid. Use an MP4, WebM, or Ogg file with a valid duration.");
      } else {
        setUploadMessage(serverMessage || `Upload failed (${status}). Check the file format, size, and duration.`);
      }
    } catch (error: unknown) {
      setUploadMessage(error instanceof Error ? error.message : "Could not read the video. Use an MP4, WebM, or Ogg file with a valid duration.");
    } finally {
      setIsUploading(false);
    }
  };

  const handleKeyPress = (key: string) => {
    if (key === "Enter") {
      handleAddVideo();
    }
  };

  const handleVideoDrop = (targetHash: string) => {
    if (!canReorderVideos || !draggedVideoHash || draggedVideoHash === targetHash) {
      setDraggedVideoHash(null);
      setDragOverVideoHash(null);
      return;
    }

    const videos = [...appState.playlistVideos.value];
    const draggedIndex = videos.findIndex(video => video.hash === draggedVideoHash);
    const targetIndex = videos.findIndex(video => video.hash === targetHash);
    if (draggedIndex === -1 || targetIndex === -1) {
      return;
    }

    const [draggedVideo] = videos.splice(draggedIndex, 1);
    videos.splice(targetIndex, 0, draggedVideo);
    setDraggedVideoHash(null);
    setDragOverVideoHash(null);
    void appHub.invoke(
      HubMessages.ReorderPlaylistVideos,
      appState.roomHash.value,
      videos.map(video => video.hash)
    );
  };

  return (
    <>
      {(appState.userPermissions.value?.canAddVideo || appState.isAdmin.value) && (
        <div className="mb-3">
          <div className="btn-group w-100 mb-2" role="group" aria-label="Choose video source">
            <button
              type="button"
              className={`btn ${source === "youtube" ? "btn-primary" : "btn-outline-light"}`}
              aria-pressed={source === "youtube"}
              disabled={youtubeApiUnavailable}
              onClick={() => { setSource("youtube"); setUploadMessage(""); }}
              ><BsYoutube className="me-2" aria-hidden="true" />YouTube</button>
            <button
              type="button"
              className={`btn ${source === "file" ? "btn-primary" : "btn-outline-light"}`}
              aria-pressed={source === "file"}
              onClick={() => { setSource("file"); setUploadMessage(""); }}
              ><BsFileEarmarkPlayFill className="me-2" aria-hidden="true" />Video file</button>
          </div>

          {source === "youtube" ? (
            <div className="d-flex">
              <InputField
                classNames={`form-control rounded-0 ${!isEnabled && "border-5 border-danger"}`}
                value={videoUrl}
                trim
                placeholder={placeholder}
                isEnabled={isEnabled}
                maxCharacters={200}
                onChange={setVideoUrl}
                onKeyDown={handleKeyPress}
              />
              <Button
                text={<BsPlusCircleFill />}
                classNames="btn btn-primary rounded-0"
                onClick={handleAddVideo}
              />
            </div>
          ) : (
            <div className="d-flex align-items-center">
              <input
                className="form-control rounded-0"
                type="file"
                accept=".mp4,.webm,.ogg,video/mp4,video/webm,video/ogg"
                aria-label="Choose a video file"
                disabled={isUploading}
                onChange={handleFileSelection}
              />
              <Button
                text={isUploading ? `${uploadProgress}%` : <BsPlusCircleFill />}
                classNames="btn btn-primary rounded-0 text-nowrap"
                disabled={!videoFile || isUploading}
                onClick={() => void handleUpload()}
              />
            </div>
          )}
          {(source === "file" || uploadMessage) && (
            <div className="small text-white-50 mt-1" aria-live="polite">
              {uploadMessage || "MP4, WebM, or Ogg · up to 500 MB · shared with everyone in this room"}
            </div>
          )}
        </div>
      )}

      <div className="list-group rounded-3 control-panel-list" ref={playlistVideosRef}>
        {appState.playlistVideos.value.length ? (
          appState.playlistVideos.value.map((video, i) => (
            <PlaylistVideoItem
              key={video.hash ?? video.url}
              index={i}
              video={video}
              draggable={canReorderVideos}
              isDragOver={dragOverVideoHash === video.hash}
              onDragStart={() => setDraggedVideoHash(video.hash ?? null)}
              onDragOver={() => setDragOverVideoHash(video.hash ?? null)}
              onDrop={() => handleVideoDrop(video.hash ?? "")}
              onDragEnd={() => {
                setDraggedVideoHash(null);
                setDragOverVideoHash(null);
              }}
            />
          ))
        ) : (
          <div className="empty-state text-white text-center" role="status">
            <BsCollectionPlayFill className="empty-state-icon" aria-hidden="true" />
            <span>Your playlist is empty</span>
          </div>
        )}
      </div>
    </>
  );
}
