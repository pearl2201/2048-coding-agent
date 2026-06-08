using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GameAgentF.Console.Executors
{
    public class JuniorDevNode
    {
        private readonly ChatClientAgent _agent;
        public ChatClientAgent Agent => _agent;
        public JuniorDevNode(IChatClient chatClient, List<AITool> tools)
        {
            // Create Phaser Developer agent
            _agent = new ChatClientAgent(chatClient,
                instructions: """
                    You are a Phaser developer working in the 'game-quick-math' folder using Phaser 4 + Vite.
        
                    CRITICAL FILE FILTER RULES:
                    1. Never read, open, list, or interact with files inside the 'node_modules' folder. It is external dependency code.
                    2. Only scan local src files, configurations, or asset lists within the workspace.
        
                    Before making any changes, list and scan the project files in 'game-quick-math' (use the file_agent tool).
                    After scanning, implement the requested changes and write progress into 'game-quick-math/code_status.txt'.
                    When ready for review, write 'ready_for_review' to 'game-quick-math/code_status.txt' and include a short summary file 'game-quick-math/summary.txt'.
                    Use the 'file_agent' tool for all file reads/writes.
                    """,
                tools: tools);
        }


    }
}
