using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Agents.AI;

public static class SessionRecoveryManager
{
    private static readonly string RecoveryFilePath = Path.Combine(Directory.GetCurrentDirectory(), "agent_session_checkpoint.json");

    public static async Task SaveCheckpointAsync(CrashRecoverySnapshot snapshot)
    {
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string jsonPayload = JsonSerializer.Serialize(snapshot, jsonOptions);
        await File.WriteAllTextAsync(RecoveryFilePath, jsonPayload);
        Console.WriteLine("💾 Session state checkpoint securely backed up to disk.");
    }

    public static async Task<CrashRecoverySnapshot?> LoadCheckpointAsync()
    {
        if (!File.Exists(RecoveryFilePath)) return null;

        try
        {
            string jsonPayload = await File.ReadAllTextAsync(RecoveryFilePath);
            return JsonSerializer.Deserialize<CrashRecoverySnapshot>(jsonPayload);
        }
        catch
        {
            Console.WriteLine("⚠️ Warning: Recovery checkpoint file found but was corrupted.");
            return null;
        }
    }

    public static void ClearCheckpoint()
    {
        if (File.Exists(RecoveryFilePath)) File.Delete(RecoveryFilePath);
    }

    public static async Task SaveTeamStateAsync(
        string savePath,
        int cycle, string step, string feedback,
        ChatClientAgent po, AgentSession poSession,
        ChatClientAgent dev, AgentSession devSession,
        ChatClientAgent tl, AgentSession tlSession)
    {
        var teamState = new TeamSessionState
        {
            CurrentCycle = cycle,
            CurrentStep = step,
            LastFeedback = feedback,
            // Use the async framework serialization method on each agent
            ProductOwnerSessionJson = await po.SerializeSessionAsync(poSession),
            DeveloperSessionJson = await dev.SerializeSessionAsync(devSession),
            TechLeadSessionJson = await tl.SerializeSessionAsync(tlSession)
        };

        string jsonPayload = JsonSerializer.Serialize(teamState, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(savePath, jsonPayload);
    }

    // 2. Hydrate the entire team loop back from a saved file
    public static async Task<(AgentSession po, AgentSession dev, AgentSession tl, TeamSessionState meta)> LoadTeamStateAsync(
        string loadPath,
        ChatClientAgent po, ChatClientAgent dev, ChatClientAgent tl)
    {
        string jsonPayload = await File.ReadAllTextAsync(loadPath);
        var state = JsonSerializer.Deserialize<TeamSessionState>(jsonPayload)!;

        // Restore individual sessions using the matching agent instances
        AgentSession poSession = await po.DeserializeSessionAsync(state.ProductOwnerSessionJson);
        AgentSession devSession = await dev.DeserializeSessionAsync(state.DeveloperSessionJson);
        AgentSession tlSession = await tl.DeserializeSessionAsync(state.TechLeadSessionJson);

        return (poSession, devSession, tlSession, state);
    }
}