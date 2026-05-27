using System.Text.Json;

public class CrashRecoverySnapshot
{
    public int CurrentLoopCycle { get; set; }

    public string CurrentWorkflowStep { get; set; }

    public string LastFeedbackMessage { get; set; }

public JsonElement ProductOwnerSessionJson { get; set; }
    public JsonElement DeveloperSessionJson { get; set; }
    public JsonElement TechLeadSessionJson { get; set; }
}


public class TeamSessionState
{
    public int CurrentCycle { get; set; }
    public string CurrentStep { get; set; } = "Breakdown";
    public string LastFeedback { get; set; } = string.Empty;

    // We serialize each agent's individual session state into this central container
    public JsonElement ProductOwnerSessionJson { get; set; }
    public JsonElement DeveloperSessionJson { get; set; }
    public JsonElement TechLeadSessionJson { get; set; }
}