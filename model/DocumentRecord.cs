using Microsoft.Extensions.VectorData;

namespace LlamaSharpConsole.Model;

public class DocumentRecord
{
    [VectorStoreKey]
    public long Id { get; set; }

    [VectorStoreData]
    public string Text { get; set; } = string.Empty;

    // Note: Adjust dimensions to match your embedding model (e.g., 1024 for Qwen3-0.6B)
    [VectorStoreVector(dimensions: 1024)]
    public ReadOnlyMemory<float> Vector { get; set; }
}