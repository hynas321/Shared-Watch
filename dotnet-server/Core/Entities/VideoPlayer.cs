using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DotnetServer.Core.Entities;

public class VideoPlayer
{
    [Key, ForeignKey("Room")]
    public string RoomHash { get; set; }

    public PlaylistVideo PlaylistVideo { get; set; }
    public bool IsPlaying { get; set; }
    public double CurrentTime { get; set; }
    public bool SetPlayedSecondsCalled { get; set; }

    public VideoPlayer()
    {
        PlaylistVideo = new PlaylistVideo();
        IsPlaying = false;
        CurrentTime = 0;
        SetPlayedSecondsCalled = false;
    }

}