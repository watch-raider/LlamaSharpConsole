using LLama;
using LLama.Common;
using LLama.Transformers;
using LLama.Sampling;
using Microsoft.Extensions.AI;
using LLama.Abstractions;
using Microsoft.Agents.AI;
using System.ComponentModel;

namespace LlamaSharpConsole.Model;

public class MafAgent : Llama3ChatSession
{
    string systemPrompt = """
        You have access to the following tool:
        - GetCurrentWeather: Returns current weather conditions. Takes no parameters.

        If you need to call this tool, respond ONLY with JSON in this format:
        {"tool": "GetCurrentWeather"}
    """;


    [Description("Gets the current weather conditions.")]
    string GetCurrentWeather() =>
        Random.Shared.NextDouble() > 0.5
            ? "It's sunny"
            : "It's raining";

    public MafAgent(string modelPath, string statePath, string chatHistoryPath, string executorPath, string[] antiPrompts, int gpuLayerCount = 0, float temperature = 0.6F, int maxTokens = -1)
        : base(modelPath, statePath, chatHistoryPath, executorPath, antiPrompts, gpuLayerCount, temperature, maxTokens)
    {
        // Build the Microsoft Agent ChatClient
        ChatClient = new ChatClientBuilder(Executor.AsChatClient()).Build();
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
        
        history.Add(new(ChatRole.System, systemPrompt));
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
            await foreach (ChatResponseUpdate update in ChatClient.GetStreamingResponseAsync(history, ChatOptions))
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

    public override void AddTools()
    {
        ChatClient = ChatClientBuilderChatClientExtensions
            .AsBuilder(ChatClient)
            .UseFunctionInvocation()
            .Build();

        ChatOptions = new() { 
            Tools = [AIFunctionFactory.Create(GetCurrentWeather)],
            ToolMode = ChatToolMode.RequireAny
            };
    }
}