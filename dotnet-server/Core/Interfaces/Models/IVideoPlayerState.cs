public interface IVideoPlayerState
{
    IPlaylistVideo PlaylistVideo { get; set; }
    bool IsPlaying { get; set; }
    double CurrentTime { get; set; }
    bool SetPlayedSecondsCalled { get; set; }
}