using System.Text.Json;

namespace DebianPackager;

public class DebBuilderConfig
{
    public Control Control { get; set; }
    public Dictionary<string, string> Files { get; set; } = new();
    public Dictionary<string, string> Scripts { get; set; } = new();
    public List<string> PreBuildCommands { get; set; } = new();

    public DebBuilderConfig() { }

    public DebBuilderConfig(
        string packageName,
        string version,
        string arch,
        string maintainer,
        string description,
        Dictionary<string, string> scripts
    )
    {
        Control = new Control
        {
            Package = packageName,
            Version = version,
            Architecture = arch,
            Maintainer = maintainer,
            Description = description,
        };
        Scripts = scripts;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize<DebBuilderConfig>(this);
    }

    public static DebBuilderConfig? Deserialize(string configPath)
    {
        if (!File.Exists(configPath))
        {
            Console.WriteLine($"debbuilder.json file does not exist at : {configPath}");
            return null;
        }
        string fileContent = File.ReadAllText(configPath);
        return JsonSerializer.Deserialize<DebBuilderConfig>(fileContent);
    }
}

public class Control
{
    public required string Package { get; set; }
    public required string Version { get; set; }
    public required string Architecture { get; set; }
    public required string Maintainer { get; set; }
    public required string Description { get; set; }
}
