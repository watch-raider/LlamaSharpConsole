using LLama;
using LLama.Common;
using LLama.Transformers;
using LLama.Sampling;
using Microsoft.Extensions.AI;

namespace LlamaSharpConsole.Interfaces;

interface IChatSession
{
    string ModelPath { get; }
    string ChatHistoryPath { get; }
    string StatePath { get; }
    string ExecutorPath { get; }
    LLamaWeights Model { get; set; }
    InteractiveExecutor Executor { get; set; }
    LLamaContext Context { get; set; }
    ChatSession Session { get; set; }
    InferenceParams InferenceParams { get;}
    ModelParams Parameters { get;}
    IChatClient? ChatClient { get; }
    ChatOptions? ChatOptions { get; set; }

    Task Run();
    void AddTools();
}