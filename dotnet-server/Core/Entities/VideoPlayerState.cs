public class VideoPlayerState : IVideoPlayerState {
    public IPlaylistVideo PlaylistVideo { get; set; }
    public bool IsPlaying { get; set; }
    public double CurrentTime { get; set; }
    public bool SetPlayedSecondsCalled { get; set; }

    public VideoPlayerState()
    {
        PlaylistVideo = new PlaylistVideo(null, null, 3, null, null);
        IsPlaying = false;
        CurrentTime = 0;
        SetPlayedSecondsCalled = false;
    }
}