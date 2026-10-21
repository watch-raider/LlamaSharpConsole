using CommunityToolkit.VectorData.SqliteVec;
using LLama;
using LLama.Abstractions;
using LLama.Common;
using Microsoft.Agents.AI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Spectre.Console;

namespace LlamaSharpConsole.Model;

public class DocumentAgent 
{
    private string ChatModelPath { get; set; }
    private string EmbedModelPath { get; set; }
    private LLamaEmbedder Embedder { get; set; }
    private IChatClient ChatClient { get; set; }

    public DocumentAgent(string chatModelPath, string embedModelPath)
    {
        ChatModelPath = chatModelPath;
        EmbedModelPath = embedModelPath;

        var chatParams = new ModelParams(ChatModelPath) { ContextSize = 4096, GpuLayerCount = -1 };
        var chatWeights = LLamaWeights.LoadFromFile(chatParams);
        var chatExecutor = new StatelessExecutor(chatWeights, chatParams);

        var embedParams = new ModelParams(EmbedModelPath) { Embeddings = true, GpuLayerCount = -1 };
        var embedWeights = LLamaWeights.LoadFromFile(embedParams);
        Embedder = new LLamaEmbedder(embedWeights, embedParams);

        // 4. Build the Microsoft Agent ChatClient
        ChatClient = new ChatClientBuilder(chatExecutor.AsChatClient()).Build();
    }

    public async Task Run()
    {
        Console.WriteLine("=== Microsoft Agent Framework & Deterministic RAG ===");

        Console.WriteLine("\nLoading Models into Memory...");

        // 2. Setup SQLite Vector Database
        Console.WriteLine("Initializing In-Memory SQLite Vector Database...");
        string connectionString = "Data Source=SharedVectorDb;Mode=Memory;Cache=Shared";

        // 2a. OPEN A MASTER CONNECTION TO KEEP THE DB ALIVE
        // This prevents SQLite from destroying the DB between vector store operations
        using var keepAliveConnection = new SqliteConnection(connectionString);
        await keepAliveConnection.OpenAsync();

        // 2b. Initialize the vector store using the same connection string
        var vectorStore = new SqliteVectorStore(connectionString);

        VectorStoreCollection<long, DocumentRecord> documentCollection =
            vectorStore.GetCollection<long, DocumentRecord>("company_docs");

        await documentCollection.EnsureCollectionExistsAsync();

        // 3. Seed the Database
        string[] rawDocuments = [
            "Project Zephyr launches in Q4. The marketing budget is $50,000.",
            "The CEO announced that Fridays are now remote-work days.",
            "Project Zephyr's lead engineer is Sarah Connor."
        ];

        long idCounter = 1;
        foreach (var doc in rawDocuments)
        {
            var embeddingBatch = await Embedder.GetEmbeddings(doc);
            float[] vectorArray = embeddingBatch.First();

            await documentCollection.UpsertAsync(new DocumentRecord
            {
                Id = idCounter++,
                Text = doc,
                Vector = new ReadOnlyMemory<float>(vectorArray)
            });
        }

        // 5. Run the Interactive Loop
        Console.WriteLine("\nSetup Complete. Type 'exit' to quit.");
        while (true)
        {
            string task = AnsiConsole.Ask("Enter your research task (or ENTER for default): ", "Draft an email about Project Zephyr");
            if (task.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            Console.WriteLine("\n[System] Searching database...");
            var embeddingBatch = await Embedder.GetEmbeddings(task);
            float[] queryVector = embeddingBatch.First();

            var searchResults = documentCollection.SearchAsync(
                new ReadOnlyMemory<float>(queryVector),
                top: 2
            );

            var retrievedFacts = new List<string>();
            await foreach (var result in searchResults)
            {
                retrievedFacts.Add($"- {result.Record.Text}");
            }
            string contextString = string.Join("\n", retrievedFacts);

            Console.WriteLine($"[System] Facts Retrieved:\n{contextString}\n");
            Console.WriteLine("[System] Agent is drafting response...");

            var writer = ChatClient.AsAIAgent(
                name: "Writer",
                instructions: "You are a strict corporate communications AI. Your ONLY job is to write a short email using EXACTLY the facts provided by the user. You must NOT add, infer, or invent any outside information."
            );

            var runOptions = new ChatClientAgentRunOptions
            {
                ChatOptions = new ChatOptions { MaxOutputTokens = 1000, Temperature = 0.0f }
            };

            string strictPrompt = $"Draft an email to the team using ONLY these exact facts:\n{contextString}\nDo not add any other information.";

            var writerResponse = await writer.RunAsync(strictPrompt, options: runOptions);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{writerResponse.Text}\n");
            Console.ResetColor();
        }
    }
}