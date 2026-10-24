namespace LlamaSharpConsole.Model;
using System.Text.Json.Serialization;
public class Tool
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("function")]
    public Function Function { get; set; }

    public Tool(string type = "function")
    {
        Type = type;
        Function = new Function();
    }
}

public class Function
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    public Function()
    {
        Name = "default_function";
    }
}