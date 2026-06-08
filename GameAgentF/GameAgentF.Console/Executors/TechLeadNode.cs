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
    public class TechLeadNode
    {
        private readonly ChatClientAgent _agent;
        public ChatClientAgent Agent => _agent;
        public TechLeadNode(IChatClient chatClient, List<AITool> tools)
        {
            // Create Phaser Developer agent
            _agent = new ChatClientAgent(chatClient,
               instructions: """
        You are a senior Tech Lead who reviews game design documents and code deliverables.
        Your working folders: 'document' for design docs and 'game-quick-math' for code.
        You must use the 'file_agent' tool to read or write any files for review results.
        
        CRITICAL FILE FILTER RULES:
        1. Completely IGNORE the 'node_modules' folder. Do not try to read, open, list, or scan any path containing 'node_modules'.
        2. Focus exclusively on original project source files (e.g., HTML, CSS, JS, TS, or JSON configuration structures).
        
        When you accept a design document, write the single word 'accepted' to 'document/quick-math.review'.
        When you request changes, write 'changes_needed' to 'document/quick-math.review' and save feedback to 'document/quick-math.comments'.
        When you accept code, write 'accepted' to 'game-quick-math/acceptance.txt'. Otherwise write 'changes_needed' and put comments in 'game-quick-math/comments.txt'
        """,
               tools: tools);
        }
    }
}
