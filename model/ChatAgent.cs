using LLama;
using LLama.Common;
using LLama.Transformers;
using LLama.Sampling;

namespace LlamaSharpConsole.Model;

public class ChatAgent : Llama3ChatSession
{
    public ChatAgent(string modelPath, string statePath, string chatHistoryPath, string executorPath, string[] antiPrompts, int gpuLayerCount = 0, float temperature = 0.6F, int maxTokens = -1)
        : base(modelPath, statePath, chatHistoryPath, executorPath, antiPrompts, gpuLayerCount, temperature, maxTokens)
    {
    }

    public override async Task Run()
    {
        await base.Run();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("The chat session has started.");
        // show the prompt
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("User> ");
        var userInput = Console.ReadLine() ?? "";

        while (userInput != "exit")
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Assistant> ");
            // as each token (partial or whole word is streamed back) print it to the console, stream to web client, etc
            await foreach (
                var text
                in Session.ChatAsync(
                    new ChatHistory.Message(AuthorRole.User, userInput),
                    InferenceParams))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(text);
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("User> ");
            userInput = Console.ReadLine() ?? "";
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Saving session...");
        Session.SaveSession(StatePath);
        Console.WriteLine("Saved session!");
        Console.ForegroundColor = ConsoleColor.White;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Saving executor state...");
        await Executor.SaveState(ExecutorPath);
        Console.WriteLine("Saved executor state!");
        Console.ForegroundColor = ConsoleColor.White;
    }
}