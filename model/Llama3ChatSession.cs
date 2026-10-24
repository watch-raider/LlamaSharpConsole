using LLama;
using LLama.Common;
using LLama.Transformers;
using LLama.Sampling;
using LlamaSharpConsole.Interfaces;

namespace LlamaSharpConsole.Model;

public class Llama3ChatSession : IChatSession
{
    public string ModelPath { get; set; }
    public string ChatHistoryPath { get; set; }
    public string StatePath { get; set; }
    public string ExecutorPath { get; set; }
    public LLamaWeights Model { get; set; }
    public InteractiveExecutor Executor { get; set; }
    public LLamaContext Context { get; set; }
    public ChatSession Session { get; set; }
    public InferenceParams InferenceParams { get; set; }
    public ModelParams Parameters { get; set; }

    public Llama3ChatSession(
        string modelPath, 
        string statePath,
        string chatHistoryPath, 
        string executorPath, 
        string[] antiPrompts,
        int gpuLayerCount = 0, 
        float temperature = 0.6f, 
        int maxTokens = -1
    )
    {
        ModelPath = modelPath;
        StatePath = statePath;
        ExecutorPath = executorPath;
        ChatHistoryPath = chatHistoryPath;

        Parameters = new ModelParams(modelPath)
        {
            GpuLayerCount = gpuLayerCount // CPU only
        };

        InferenceParams = InferenceParams = new InferenceParams
        {
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = temperature
            },
            MaxTokens = maxTokens, // keep generating tokens until the anti prompt is encountered
            AntiPrompts = antiPrompts // model specific end of turn string (or default)
        };

        Model = LLamaWeights.LoadFromFile(Parameters);
        Context = Model.CreateContext(Parameters);
        Executor = new InteractiveExecutor(Context);

        Session = new(Executor, new ChatHistory());
    }

    public virtual async Task Run()
    {
        // Load the chat history from the specified file, if it exists
        
        var chatHistoryJson = File.ReadAllText(ChatHistoryPath);
        var chatHistory = ChatHistory.FromJson(chatHistoryJson) ?? new ChatHistory();

        Session = new(Executor, chatHistory);
    }
}