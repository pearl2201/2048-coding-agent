public class CrashRecoverySnapshot
{
    public int CurrentLoopCircle { get; set; }

    public string CurrentWorkflowStep { get; set; }

    public string LastFeedbackMessage { get; set; }

    public List<SerializableChatMessage> AgentThreadHistory { get; set; }
}

public class SerializableChatMessage
{
    public string Role { get; set; }

    public string Content { get; set; }

    public string AuthorName { get; set; }
}
