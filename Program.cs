using System.CommandLine;
using System.CommandLine.Parsing;

namespace DebianPackager;

class Program
{
    static void Main(string[] args)
    {
        RootCommand root = new RootCommand("Debian Package Automating CLI Tool");
        Command initCommand = new Command(
            "init",
            "Initializes the Debian Builder folder with package folder inside and default scripts."
        );
        Option<string> packageOption = new("--package", new[] { "-p" })
        {
            Description = "Name of the package",
            Required = true,
        };
        Option<string> versionOption = new("--version", new[] { "-v" })
        {
            Description = "Version of the package",
            Required = true,
        };
        Option<string> archOption = new("--arch", new[] { "-a" })
        {
            Description = "Architecture of the package",
            Required = true,
        };
        Option<string> maintainerOption = new("--maintainer", new[] { "-m" })
        {
            Description = "Maintainer of the package",
            Required = true,
        };
        Option<string> descriptionOption = new("--description", new[] { "-d" })
        {
            Description = "Description of the package",
            Required = true,
        };
        initCommand.Options.Add(packageOption);
        initCommand.Options.Add(versionOption);
        initCommand.Options.Add(archOption);
        initCommand.Options.Add(maintainerOption);
        initCommand.Options.Add(descriptionOption);
        initCommand.SetAction(initResult =>
        {
            //run the init logic
            if (initResult.Errors.Count == 0)
            {
                string? packageName = initResult.GetValue(packageOption);
                string? version = initResult.GetValue(versionOption);
                string? architecture = initResult.GetValue(archOption);
                string? maintainer = initResult.GetValue(maintainerOption);
                string? description = initResult.GetValue(descriptionOption);
                DebBuilder.Initialize(
                    packageName!,
                    version!,
                    architecture!,
                    maintainer!,
                    description!
                );
            }
            foreach (ParseError error in initResult.Errors)
            {
                Console.WriteLine(error.Message);
            }
        });
        Command filesCommand = new Command(
            "files",
            "Takes source and destination of the files and appends to the debbuiler.json Files Array."
        );
        Option<string> srcOption = new("--src", new[] { "-s" })
        {
            Description =
                "Source path of the file (no need to exist, it should only exist during build command)",
            Required = true,
        };
        Option<string> destOption = new("--dest", new[] { "-d" })
        {
            Description = "Destination path of the file",
            Required = true,
        };
        filesCommand.Options.Add(srcOption);
        filesCommand.Options.Add(destOption);
        filesCommand.SetAction(filesResult =>
        {
            //run the files logic
            if (filesResult.Errors.Count == 0)
            {
                string? sourceFilePath = filesResult.GetValue(srcOption);
                string? destinationFilePath = filesResult.GetValue(destOption);
                DebBuilder.AddFileMappings(sourceFilePath!, destinationFilePath!);
            }
            foreach (ParseError error in filesResult.Errors)
            {
                Console.WriteLine(error.Message);
            }
        });
        Command prebuildCommand = new Command(
            "prebuild",
            "Takes command string and appends to the debbuiler.json PrebuildCommands Array."
        );
        Option<string> commandOption = new("--command", new[] { "-c" })
        {
            Description = "Prebuild command",
            Required = true,
        };
        prebuildCommand.Options.Add(commandOption);
        prebuildCommand.SetAction(prebuildResult =>
        {
            //run prebuild logic
            if (prebuildResult.Errors.Count == 0)
            {
                string? command = prebuildResult.GetValue(commandOption);
                DebBuilder.AddPreBuildCommand(command!);
            }
            foreach (ParseError error in prebuildResult.Errors)
            {
                Console.WriteLine(error.Message);
            }
        });
        Command buildCommand = new Command(
            "build",
            "Loads the debbuiler.json and performs file copying and package building"
        );
        buildCommand.SetAction(buildResult =>
        {
            //run build logic
            if (buildResult.Errors.Count == 0)
            {
                DebBuilder.Build();
            }
            foreach (ParseError error in buildResult.Errors)
            {
                Console.WriteLine(error.Message);
            }
        });
        root.Subcommands.Add(initCommand);
        root.Subcommands.Add(filesCommand);
        root.Subcommands.Add(prebuildCommand);
        root.Subcommands.Add(buildCommand);
        ParseResult result = root.Parse(args);
        result.Invoke();
    }
}
