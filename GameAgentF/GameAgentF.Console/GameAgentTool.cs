using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
public static class GameAgentTool
{
    public static string WorkingDirectory { get; set; }
    // Define a native high-speed project scanner tool
    [Description("Quickly scans the structure of the project directories to give the agent an immediate map of the files, bypassing node_modules.")]
    public static string ScanProjectStructure([Description("The project root directory path.")] string workingPath)
    {
        var sb = new StringBuilder();
        if (workingPath == "." || workingPath == "/")
        {
            workingPath = WorkingDirectory;
        }
        else if (!workingPath.StartsWith("D:"))
        {
            workingPath = Path.Combine(WorkingDirectory, workingPath);
        }
        var root = new DirectoryInfo(workingPath);

        // Quick structural mapping function
        void MapDirectory(DirectoryInfo dir, string indent)
        {
            // Hard filter heavy directories right at the OS level
            if (dir.Name == "node_modules" || dir.Name == ".git" || dir.Name == "dist") return;

            sb.AppendLine($"{indent}📁 {dir.Name}/");
            foreach (var file in dir.GetFiles())
            {
                // Optional: Include tiny summary details like file size to help agent decisions
                sb.AppendLine($"{indent}  📄 {file.Name} ({file.Length} bytes)");
            }
            foreach (var subDir in dir.GetDirectories())
            {
                MapDirectory(subDir, indent + "  ");
            }
        }

        MapDirectory(root, "");
        return sb.ToString();
    }
}