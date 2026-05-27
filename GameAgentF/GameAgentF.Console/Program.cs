

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Requires Microsoft.Extensions.Configuration.Json
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

// Resolve working path from config; fall back to current directory
var workingPath = config["WorkingPath"];
if (string.IsNullOrWhiteSpace(workingPath))
{
    workingPath = Directory.GetCurrentDirectory();
}
workingPath = Path.GetFullPath(workingPath);

var togetherOptions = new OpenAIClientOptions { Endpoint = new Uri("https://api.together.xyz/v1") };
var togetherClient = new OpenAIClient(new ApiKeyCredential(config["TOGETHER_AI_KEY"]), togetherOptions);

await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
{
    Name = "MCPServer",
    Command = "npx",
    Arguments = ["-y", "--verbose", "@modelcontextprotocol/server-filesystem", config["WorkingPath"]],
}));
var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
Console.WriteLine("Hello! I'm a simple AI assistant powered by Gemini 3.0 and the Microsoft Agent Framework.");
Console.WriteLine("Chat with me! Type 'exit' or 'quit' to end the conversation.\n");
var mcpToolsResult = await mcpClient.ListToolsAsync().ConfigureAwait(false);
var secureAgentTools = new List<AITool>();

foreach (var mcpTool in mcpToolsResult)
{
    // Bypassing search_files completely prevents the 'query' argument crash we saw earlier
    if (mcpTool.Name == "list_directory") continue;

    // Convert raw tool to Microsoft.Extensions.AI base tool
    var baseTool = (AITool)mcpTool;
}

// // Wrap the tool in a strict defensive runtime validator
// secureAgentTools.Add(AIFunctionFactory.Create(async (arguments, cancellationToken) =>
//     {
//         // Scan all incoming tool string arguments for forbidden keywords
//         foreach (var kvp in arguments)
//         {
//             string? argumentValue = kvp.Value?.ToString();
//             if (!string.IsNullOrEmpty(argumentValue) &&
//                 (argumentValue.Contains("node_modules", StringComparison.OrdinalIgnoreCase) ||
//                  argumentValue.Contains("node_modules/")))
//             {
//                 // INTERCEPTED: Block execution and return a descriptive correction to the model
//                 Console.ForegroundColor = ConsoleColor.Red;
//                 Console.WriteLine($"\n🛡️ [Interceptor Blocked Tool Call]: '{baseTool.Metadata.Name}' attempted to access 'node_modules'.");
//                 Console.ResetColor();

//                 return new AIToolResult(
//                     "Error: Access denied. The 'node_modules' folder is completely locked down. " +
//                     "Please alter your path parameter targets to search inside 'src/' or 'game-2048/' source files only."
//                 );
//             }
//         }

//         // Path is clear -> execute the original file system tool operation safely
//         return await mcpTool.ExecuteAsync(arguments, cancellationToken);
//     },
//      name: mcpTool.Name,
//     description: mcpTool.Description,

// ));


var chatClient = togetherClient.GetChatClient("MiniMaxAI/MiniMax-M2.7");
var file_agent = new ChatClientAgent(chatClient.AsIChatClient(),
    instructions: """
        You are a specialized file utility agent with direct access to the local filesystem.
        You can list directory contents, read files, write new files, and make selective edits.
        When asked to create or write a file, you MUST execute your actual 'write_file' or 'edit_file' tools.
        Do not just reply with text; call the tool.
        
        SAFETY GUARDRAIL:
        - You are strictly FORBIDDEN from interacting with any folder named 'node_modules'. 
        - If a child agent or master agent asks you to list or search a path inside 'node_modules', intercept the call and return a text message stating that 'node_modules' is omitted for optimization.
        """,
     tools: [.. mcpTools.Cast<AITool>()]);
// 2. CORRECT FIX: Force a beautiful tool name and schema description for the master model to read!
var toolOptions = new AIFunctionFactoryOptions
{
    Name = "file_agent", // This replaces AsAIFunction_InvokeAgent_0!
    Description = "Delegates an execution task to the file manager agent. Input should be a clear, written instruction of what you want to do with files (e.g., 'Save this text to a file called document/GameDesign.md' or 'List files')."
};

AITool fileAgentTool = file_agent.AsAIFunction(toolOptions);
var agent = new ChatClientAgent(chatClient.AsIChatClient(),
                            instructions: """
                                    You are an AI Product Owner. Your job is to manage features and author design documents.
                                    CRITICAL: You do not have direct file access. You MUST call your 'file_agent' tool to save, write, list, or read any file.
                                    If the user asks to save a file, you have NOT completed the task until you successfully trigger the file_agent tool function call. 
                                    Your working directory root folder is 'document'.
                                    """,
                              tools: [fileAgentTool]);


// Prepare folders under configured working path
Directory.CreateDirectory(Path.Combine(workingPath, "document"));
Directory.CreateDirectory(Path.Combine(workingPath, "game-2048"));

// Create Tech Lead agent
var techLead = new ChatClientAgent(chatClient.AsIChatClient(),
    instructions: """
        You are a senior Tech Lead who reviews game design documents and code deliverables.
        Your working folders: 'document' for design docs and 'game-2048' for code.
        You must use the 'file_agent' tool to read or write any files for review results.
        
        CRITICAL FILE FILTER RULES:
        1. Completely IGNORE the 'node_modules' folder. Do not try to read, open, list, or scan any path containing 'node_modules'.
        2. Focus exclusively on original project source files (e.g., HTML, CSS, JS, TS, or JSON configuration structures).
        
        When you accept a design document, write the single word 'accepted' to 'document/2048.review'.
        When you request changes, write 'changes_needed' to 'document/2048.review' and save feedback to 'document/2048.comments'.
        When you accept code, write 'accepted' to 'game-2048/acceptance.txt'. Otherwise write 'changes_needed' and put comments in 'game-2048/comments.txt'
        """,
    tools: [fileAgentTool]);

// Create Phaser Developer agent
var phaserDev = new ChatClientAgent(chatClient.AsIChatClient(),
    instructions: """
        You are a Phaser developer working in the 'game-2048' folder using Phaser 4 + Vite.
        
        CRITICAL FILE FILTER RULES:
        1. Never read, open, list, or interact with files inside the 'node_modules' folder. It is external dependency code.
        2. Only scan local src files, configurations, or asset lists within the workspace.
        
        Before making any changes, list and scan the project files in 'game-2048' (use the file_agent tool).
        After scanning, implement the requested changes and write progress into 'game-2048/code_status.txt'.
        When ready for review, write 'ready_for_review' to 'game-2048/code_status.txt' and include a short summary file 'game-2048/summary.txt'.
        Use the 'file_agent' tool for all file reads/writes.
        """,
    tools: [fileAgentTool]);

// Helper to synchronously read small review files produced by agents
string ReadFileIfExists(string path)
{
    if (File.Exists(path)) return File.ReadAllText(path).Trim();
    return string.Empty;
}

AgentSession agentSession = await agent.CreateSessionAsync();
if (!File.Exists(Path.Combine(workingPath, "document", "2048.md")))
{
    await foreach (var update in agent.RunStreamingAsync("""
        Use your file_agent tool to write a complete game design document for 2048 to the file 'document/2048.md'. Do not reply to me until the tool execution returns success
        """))
    {
        Console.Write(update);
    }
}
if (!File.Exists(Path.Combine(workingPath, "document", "2048.review")))
{
    // 1) Loop: PO <-> Tech Lead until design doc accepted
    Console.WriteLine("--- Starting PO <-> Tech Lead design review loop ---");
    while (true)
    {
        Console.WriteLine("Tech Lead: reviewing document/2048.md...");
        await foreach (var update in techLead.RunStreamingAsync("Read 'document/2048.md' and review it. If acceptable, write 'accepted' to 'document/2048.review'. Otherwise write 'changes_needed' to 'document/2048.review' and put comments in 'document/2048.comments'. Do not reply until file operations complete."))
        {
            Console.Write(update);
        }

        var reviewPath = Path.Combine(workingPath, "document", "2048.review");
        var review = ReadFileIfExists(reviewPath);
        Console.WriteLine($"Tech Lead review result: {review}");
        if (review.Equals("accepted", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Design document accepted by Tech Lead.");
            break;
        }

        // Tech Lead requested changes: let the PO agent update the document using comments
        var commentsPath = Path.Combine(workingPath, "document", "2048.comments");
        var comments = ReadFileIfExists(commentsPath);
        Console.WriteLine("PO: updating document based on comments from Tech Lead...");
        var poInstruction = $"Read 'document/2048.comments' and the current 'document/2048.md', apply the requested changes, and overwrite 'document/2048.md' with the improved version. Save a brief changelog to 'document/2048.changelog'. Do not reply until saved.";
        await foreach (var update in agent.RunStreamingAsync(poInstruction))
        {
            Console.Write(update);
        }
        Console.WriteLine("PO: update complete — looping back to Tech Lead for another review.");
    }
}
// 2) Loop: Tech Lead <-> Phaser Developer until code accepted
Console.WriteLine("--- Starting Tech Lead <-> Phaser Developer code review loop ---");
while (true)
{
    // Tech Lead asks for code or reviews latest code status
    Console.WriteLine("Tech Lead: perform a code review on 'game-2048' and write acceptance status to 'game-2048/acceptance.txt' (accepted/changes_needed). If changes needed, write details to 'game-2048/comments.txt'.");
    await foreach (var update in techLead.RunStreamingAsync("Review the current code in 'game-2048'. If acceptable write 'accepted' to 'game-2048/acceptance.txt'. Otherwise write 'changes_needed' and put review comments in 'game-2048/comments.txt'. Use the file_agent tool only for file reads/writes."))
    {
        Console.Write(update);
    }

    var acceptancePath = Path.Combine(workingPath, "game-2048", "acceptance.txt");
    var acceptance = ReadFileIfExists(acceptancePath);
    Console.WriteLine($"Tech Lead code acceptance result: {acceptance}");
    if (acceptance.Equals("accepted", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Phaser code accepted by Tech Lead. Loop complete.");
        break;
    }

    // Phaser developer scans project and implements requested changes
    Console.WriteLine("Phaser Dev: scanning 'game-2048' and applying requested changes...");
    await foreach (var update in phaserDev.RunStreamingAsync("List files in 'game-2048', then read 'game-2048/comments.txt' and implement the requested changes. After changes, write 'game-2048/code_status.txt' with 'ready_for_review' and a short 'game-2048/summary.txt' describing what you changed."))
    {
        Console.Write(update);
    }

    Console.WriteLine("Phaser Dev: changes applied, asking Tech Lead to re-review.");
}

Console.WriteLine("All loops complete. Exiting.");
