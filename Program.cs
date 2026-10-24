using LlamaSharpConsole.Model;
using LlamaSharpConsole.Interfaces;

namespace LlamaSharpConsole;

class Program
{
    public static async Task Main(string[] args)
    {
        IChatSession chatSession = new ChatAgent(
            modelPath: "./models/Ministral-3-3B-Instruct-2512-Q4_K_M.gguf",
            statePath: "./session_state",
            chatHistoryPath: "./session_state/ChatHistory.json",
            executorPath: "./session_executor/executor_state.bin",
            antiPrompts: [ "User:" ]
        );
        
        // Entry point for the application
        await chatSession.Run();
    }
}