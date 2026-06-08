

using DeepSeek;
using DeepSeek.Agents.AI;
using GameAgentF.Console.Executors;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;



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
GameAgentTool.WorkingDirectory = workingPath;

var togetherClient = new DeepSeekClient(config["DEEPSEEK_AI_KEY"])
    .GetChatClient("deepseek-v4-flash");

var pmClient = new DeepSeekClient(config["DEEPSEEK_AI_KEY"])
    .GetChatClient("deepseek-v4-flash");

// 2. Configure options specific for strategic PM tasks (PRD Generation / Roadmap Planning)
var strategicPrdOptions = new ChatClientAgentRunOptions(new ChatOptions()
{
    //Temperature = 0.7f, // Keeps the creative copywriting professional yet exploratory
    
    //// Pass custom properties recognized by the DeepSeek V4 API handler
    //AdditionalProperties = new AdditionalPropertiesDictionary( new Dictionary<string, object>
    //{
    //    // Force the model into its deeply analytical "Think High" or "Think Max" mode 
    //    // to prevent it from skimming over complex product edge-cases.
    //    { "thinking_mode", "thinking" },
    //    { "max_thinking_tokens", 4096 }
    //})
});

await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
{
    Name = "MCPServer",
    Command = "npx",
    Arguments = ["-y", "--verbose", "github:a13-team/filesystem-mcp-ignore", "-i", ".gitignore,.cursorignore", config["WorkingPath"]],
}));
var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

var mcpToolsResult = await mcpClient.ListToolsAsync().ConfigureAwait(false);

AIFunction scanProjectFunction = AIFunctionFactory.Create(GameAgentTool.ScanProjectStructure);


var chatClient = pmClient.AsIChatClient();//togetherClient.GetChatClient("MiniMaxAI/MiniMax-M2.7");
var coderClient = togetherClient.AsIChatClient(); //togetherClient.GetChatClient("Qwen/Qwen3.5-397B-A17B");
var file_agent = new ChatClientAgent(coderClient,
    instructions: """
        You are a specialized file utility agent with direct access to the local filesystem.
        You can list directory contents, read files, write new files, and make selective edits.
        When asked to create or write a file, you MUST execute your actual 'write_file' or 'edit_file' tools.
        Do not just reply with text; call the tool.
        
        SAFETY GUARDRAIL:
        - You are strictly FORBIDDEN from interacting with any folder named 'node_modules'. 
        - If a child agent or master agent asks you to list or search a path inside 'node_modules', intercept the call and return a text message stating that 'node_modules' is omitted for optimization.
        """,
     tools: [scanProjectFunction, .. mcpTools.Cast<AITool>()]);
// 2. CORRECT FIX: Force a beautiful tool name and schema description for the master model to read!
var toolOptions = new AIFunctionFactoryOptions
{
    Name = "file_agent", // This replaces AsAIFunction_InvokeAgent_0!
    Description = "Delegates an execution task to the file manager agent. Input should be a clear, written instruction of what you want to do with files (e.g., 'Save this text to a file called document/GameDesign.md' or 'List files')."
};

AITool fileAgentTool = file_agent.AsAIFunction(toolOptions);
var poAgent = new ProductManagerNode(pmClient.AsIChatClient(),
                              tools: [scanProjectFunction, fileAgentTool]);
var techLeadAgent = new TechLeadNode(pmClient.AsIChatClient(),
                              tools: [scanProjectFunction, fileAgentTool]);
var juniorDevAgent = new JuniorDevNode(pmClient.AsIChatClient(),
                              tools: [scanProjectFunction, fileAgentTool]);

// Create a checkpoint manager to manage checkpoints
CheckpointManager checkpointManager = CheckpointManager.CreateInMemory();

WorkflowBuilder builder = new(poAgent.Agent); // Set starting executor
builder.AddEdge(poAgent.Agent, techLeadAgent.Agent);
builder.AddEdge(techLeadAgent.Agent, juniorDevAgent.Agent);

var workflow = builder.Build();

// Prepare folders under configured working path
Directory.CreateDirectory(Path.Combine(workingPath, "document"));
Directory.CreateDirectory(Path.Combine(workingPath, "game-quick-math"));
string inputMessage = File.ReadAllText(config["Task"]);
// Streaming execution — get events as they happen
StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, new ChatMessage(ChatRole.User, inputMessage),checkpointManager);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is ExecutorCompletedEvent executorComplete)
    {
        Console.WriteLine($"{executorComplete.ExecutorId}: {executorComplete.Data}");
    }

    if (evt is WorkflowOutputEvent outputEvt)
    {
        Console.WriteLine($"Workflow completed: {outputEvt.Data}");
    }

    if (evt is SuperStepCompletedEvent superStepCompletedEvt)
    {
        // Access the checkpoint
        CheckpointInfo? checkpoint = superStepCompletedEvt.CompletionInfo?.Checkpoint;
    }
}