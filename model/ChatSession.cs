using LLama;
using LLama.Common;
using LLama.Transformers;
using LLama.Sampling;

namespace LlamaSharpConsole.Model;

public class Llama3ChatSession
{
    private string StatePath { get; set; }
    private string ExecutorPath { get; set; }
    private LLamaWeights Model { get; set; }
    private InteractiveExecutor Executor { get; set; }
    private LLamaContext Context { get; set; }
    private ChatSession Session { get; set; }

    public Llama3ChatSession(string modelPath, string statePath, string chatHistoryPath, string executorPath)
    {
        StatePath = statePath;
        ExecutorPath = executorPath;

        var parameters = new ModelParams(modelPath)
        {
            GpuLayerCount = 0 // CPU only
        };

        Model = LLamaWeights.LoadFromFile(parameters);
        Context = Model.CreateContext(parameters);
        Executor = new InteractiveExecutor(Context);

        var chatHistoryJson = File.ReadAllText(chatHistoryPath);
        var chatHistory = ChatHistory.FromJson(chatHistoryJson) ?? new ChatHistory();

        Session = new(Executor, chatHistory);
    }

    public async Task Run()
    {
        // add the default templator. If llama.cpp doesn't support the template by default, 
        // you'll need to write your own transformer to format the prompt correctly
        Session.WithHistoryTransform(new PromptTemplateTransformer(Model, withAssistant: true)); 
        // Add a transformer to eliminate printing the end of turn tokens, llama 3 specifically has an odd LF that gets printed sometimes
        Session.WithOutputTransform(new LLamaTransforms.KeywordTextOutputStreamTransform(
            ["User:", "�"],
            redundancyLength: 5));

        var inferenceParams = new InferenceParams
        {
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = 0.6f
            },
            MaxTokens = -1, // keep generating tokens until the anti prompt is encountered
            AntiPrompts = ["User:"] // model specific end of turn string (or default)
        };

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
                    inferenceParams))
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