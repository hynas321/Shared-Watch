public class RoomSettings {
    public int MaxUsers { get; set; }

    //User settings
    public bool IsSendingChatMessagesAllowed { get; set; }
    public bool IsAddingVideosAllowed { get; set; }
    public bool IsRemovingVideosAllowed { get; set; }
    public bool IsPlayingVideosOutsideOfPlaylistAllowed { get; set; }
    public bool IsStartingPausingVideosAllowed { get; set; }
    public bool IsSkippingVideosAllowed { get; set; }

    public RoomSettings()
    {
        MaxUsers = 6;
        IsSendingChatMessagesAllowed = true;
        IsAddingVideosAllowed = true;
        IsRemovingVideosAllowed = true;
        IsPlayingVideosOutsideOfPlaylistAllowed = false;
        IsStartingPausingVideosAllowed = false;
        IsSkippingVideosAllowed = false;
    }
}