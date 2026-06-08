using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace GameAgentF.Console.Executors
{


    internal sealed class ProductManagerNode 
    {
        private readonly ChatClientAgent _agent;

        public ChatClientAgent Agent => _agent;
        public ProductManagerNode(IChatClient chatClient, List<AITool> tools)
        {
            _agent = new ChatClientAgent(chatClient,
                            instructions: """
                                    You are an AI Product Owner. Your job is to manage features and author design documents.
                                    CRITICAL: You do not have direct file access. You MUST call your 'file_agent' tool to save, write, list, or read any file.
                                    If the user asks to save a file, you have NOT completed the task until you successfully trigger the file_agent tool function call. 
                                    Your working directory root folder is 'document'.
                                    """,
                              tools: tools);
        }



    }
}
