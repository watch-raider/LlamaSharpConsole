using LLama;
using LLama.Common;
using LLama.Abstractions;
using Microsoft.Extensions.AI;

namespace LlamaSharpConsole.Model;

public class FunctionAgent
{
    private IChatClient Client { get; set; }

    public FunctionAgent(string chatModelPath)
    {
        var chatParams = new ModelParams(chatModelPath) { ContextSize = 4096, GpuLayerCount = -1 };
        var chatWeights = LLamaWeights.LoadFromFile(chatParams);
        var chatExecutor = new StatelessExecutor(chatWeights, chatParams);

        // 4. Build the Microsoft Agent ChatClient
        Client = new ChatClientBuilder(chatExecutor.AsChatClient()).Build();
    }

    // Add a new plugin with a local .NET function
    // that should be available to the AI model.
    private ChatOptions chatOptions = new ChatOptions
    {
        Tools = [AIFunctionFactory.Create((string location, string unit) =>
        {
            // Here you would call a weather API
            // to get the weather for the location.
            return "Periods of rain or drizzle, 15 C";
        },
        "get_current_weather",
        "Gets the current weather in a given location")]
    };

    public async Task Run()
    {
        // System prompt to provide context.
            List<ChatMessage> chatHistory = [new(ChatRole.System, """
                You are a hiking enthusiast who helps people discover fun hikes in their area. You are upbeat and friendly.
                """)];

            // Weather conversation relevant to the registered function.
            chatHistory.Add(new ChatMessage(ChatRole.User,
                "I live in Montreal and I'm looking for a moderate intensity hike. What's the current weather like?"));
            Console.WriteLine($"{chatHistory.Last().Role} >>> {chatHistory.Last()}");

            ChatResponse response = await Client.GetResponseAsync(chatHistory, chatOptions);
            Console.WriteLine($"Assistant >>> {response.Text}");
    }
}