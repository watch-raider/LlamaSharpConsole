using LLama;
using LLama.Common;
using LLama.Transformers;
using LLama.Sampling;
using Microsoft.Extensions.AI;
using LLama.Abstractions;
using Microsoft.Agents.AI;

namespace LlamaSharpConsole.Model;

public class MafAgent : Llama3ChatSession
{
    private IChatClient chatClient;

    public MafAgent(string modelPath, string statePath, string chatHistoryPath, string executorPath, string[] antiPrompts, int gpuLayerCount = 0, float temperature = 0.6F, int maxTokens = -1)
        : base(modelPath, statePath, chatHistoryPath, executorPath, antiPrompts, gpuLayerCount, temperature, maxTokens)
    {
        // Build the Microsoft Agent ChatClient
        chatClient = new ChatClientBuilder(Executor.AsChatClient()).Build();
    }

    public override async Task Run()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("The chat session has started.");
        // show the prompt
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("User> ");
        var userInput = Console.ReadLine() ?? "";

        List<ChatMessage> history = [];
        history.Add(new(ChatRole.System, "You are a helpful assistant."));
        while (userInput != "exit")
        {
            history.Add(new(ChatRole.User, userInput));

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Assistant> ");
            // as each token (partial or whole word is streamed back) print it to the console, stream to web client, etc
            // var response = await assistant.RunAsync(userInput, options: runOptions);
            // var response = await chatClient.GetResponseAsync(history);

            Console.ForegroundColor = ConsoleColor.White;
            List<ChatResponseUpdate> updates = [];
            // Console.Write(response.Text);
            await foreach (ChatResponseUpdate update in chatClient.GetStreamingResponseAsync(history))
            {
                Console.Write(update);
                updates.Add(update);
            }
            
            history.AddMessages(updates);
            
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("User> ");
            userInput = Console.ReadLine() ?? "";
        }
    }
}