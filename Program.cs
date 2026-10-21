using LlamaSharpConsole.Model;

namespace LlamaSharpConsole;

class Program
{
    public static async Task Main(string[] args)
    {
        Llama3ChatSession chatSession = new(
            modelPath: "./models/Ministral-3-3B-Instruct-2512-Q4_K_M.gguf",
            statePath: "./session_state",
            chatHistoryPath: "./session_state/ChatHistory.json",
            executorPath: "./session_executor/executor_state.bin"
        );
        
        // Entry point for the application
        await chatSession.Run();

        var documentAgent = new DocumentAgent(
            chatModelPath: "./models/Ministral-3-3B-Instruct-2512-Q4_K_M.gguf",
            embedModelPath: "./models/Qwen3-Embedding-0.6B-Q8_0.gguf"
        );

        // Entry point for running the DocumentAgent
        await documentAgent.Run();
    }
}