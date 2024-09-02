using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DotnetServer.Core.Entities;

public class VideoPlayer
{
    [Key, ForeignKey("Room")]
    public string RoomHash { get; set; }

    public PlaylistVideo PlaylistVideo { get; set; } = new PlaylistVideo();
    public bool IsPlaying { get; set; } = false;
    public double CurrentTime { get; set; } = 0;
    public bool SetPlayedSecondsCalled { get; set; } = false;
}