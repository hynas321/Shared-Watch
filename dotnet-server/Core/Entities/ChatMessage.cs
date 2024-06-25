public class ChatMessage : IChatMessage
{
    public string Username { get; set; }
    public string Text { get; set; }
    public string Date { get; set; }

    public ChatMessage(string username, string text, string date)
    {
        Username = username;
        Text = text;
        Date = date;
    }
}