using System.Diagnostics;
namespace DebianPackager;

public class DebBuilder
{
    private const string DEB_BUILER_FOLDER_PATH = "DebBuilder";
    private static readonly string DebBuilderCompletePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        DEB_BUILER_FOLDER_PATH
    );
    private static readonly string DebBuilderJsonFilePath = Path.Combine(
        DebBuilderCompletePath,
        "debbuilder.json"
    );
    private static readonly string DebBuilderControlFilePath = Path.Combine(
        DebBuilderCompletePath,
        "control"
    );
    private static readonly string PreInstPath = Path.Combine(DebBuilderCompletePath, "preinst");
    private static readonly string PostInstPath = Path.Combine(DebBuilderCompletePath, "postinst");
    private static readonly string PreRmPath = Path.Combine(DebBuilderCompletePath, "prerm");
    private static readonly string PostRmPath = Path.Combine(DebBuilderCompletePath, "postrm");

    public static void Initialize(
        string packageName,
        string version,
        string architecture,
        string maintainer,
        string description
    )
    {
        try
        {
            //create DebBuilder folder
            if (!Directory.Exists(DebBuilderCompletePath))
            {
                Directory.CreateDirectory(DebBuilderCompletePath);
            }
            if (File.Exists(DebBuilderJsonFilePath))
            {
                Console.WriteLine("Config already exists.");
            }
            if (!File.Exists(PreInstPath))
            {
                File.Create(PreInstPath);
            }
            if (!File.Exists(PostInstPath))
            {
                File.Create(PostInstPath);
            }
            if (!File.Exists(PreRmPath))
            {
                File.Create(PreRmPath);
            }
            if (!File.Exists(PostRmPath))
            {
                File.Create(PostRmPath);
            }

            Dictionary<string, string> scripts = new()
            {
                {
                    $"{DebBuilderControlFilePath}",
                    $"{DebBuilderCompletePath}/{packageName}/DEBIAN/control"
                },
                { $"{PreInstPath}", $"{DebBuilderCompletePath}/{packageName}/DEBIAN/preinst" },
                { $"{PostInstPath}", $"{DebBuilderCompletePath}/{packageName}/DEBIAN/postinst" },
                { $"{PreRmPath}", $"{DebBuilderCompletePath}/{packageName}/DEBIAN/prerm" },
                { $"{PostRmPath}", $"{DebBuilderCompletePath}/{packageName}/DEBIAN/postrm" },
            };
            DebBuilderConfig config = new DebBuilderConfig(
                packageName,
                version,
                architecture,
                maintainer,
                description,
                scripts
            );
            if (!File.Exists(DebBuilderControlFilePath))
            {
                using StreamWriter sw = new(DebBuilderControlFilePath, false);
                sw.WriteLine($"Package: {config.Control.Package}");
                sw.WriteLine($"Version: {config.Control.Version}");
                sw.WriteLine($"Architecture: {config.Control.Architecture}");
                sw.WriteLine($"Maintainer: {config.Control.Maintainer}");
                sw.WriteLine($"Description: {config.Control.Description}");
            }
            string? json = config.Serialize();
            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine("Unable to create config");
                return;
            }
            File.WriteAllText(DebBuilderJsonFilePath, json);
            Console.WriteLine($"Config is created at : {DebBuilderJsonFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static void AddFileMappings(string srcFilePath, string destFilePath)
    {
        try
        {
            if (!File.Exists(DebBuilderJsonFilePath))
            {
                Console.WriteLine("Please initialize first, debbuilder init command");
                return;
            }
            DebBuilderConfig? config = DebBuilderConfig.Deserialize(DebBuilderJsonFilePath);
            if (config == null)
            {
                Console.WriteLine("Please initialize first, debbuilder init command");
                return;
            }
            if (config.Files.TryAdd(srcFilePath, destFilePath))
            {
                Console.WriteLine("Added file mappings");
                string configJson = config.Serialize();
                File.WriteAllText(DebBuilderJsonFilePath, configJson);
            }
            else
            {
                Console.WriteLine("Skipping the file mappings, srcfile already exists");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static void AddPreBuildCommand(string command)
    {
        try
        {
            if (!File.Exists(DebBuilderJsonFilePath))
            {
                Console.WriteLine("Please initialize first, debbuilder init command");
                return;
            }
            DebBuilderConfig? config = DebBuilderConfig.Deserialize(DebBuilderJsonFilePath);
            if (config == null)
            {
                Console.WriteLine("Please initialize first, debbuilder init command");
                return;
            }
            config.PreBuildCommands.Add(command);
            Console.WriteLine("Added prebuild command");
            string configJson = config.Serialize();
            File.WriteAllText(DebBuilderJsonFilePath, configJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static void Build()
    {
        try
        {
            if (!File.Exists(DebBuilderJsonFilePath))
            {
                Console.WriteLine("Please initialize first, debbuilder init command");
                return;
            }
            DebBuilderConfig? config = DebBuilderConfig.Deserialize(DebBuilderJsonFilePath);
            if (config == null)
            {
                Console.WriteLine("Please initialize first, debbuilder init command");
                return;
            }
            //do file copying
            Console.WriteLine("Performing files copying");
            foreach (KeyValuePair<string, string> fileMappings in config.Files)
            {
                string source = fileMappings.Key;
                if (!File.Exists(source))
                {
                    Console.WriteLine($"{source} does not exists, Skipping");
                    continue;
                }
                string destination = fileMappings.Value;

                Console.WriteLine($"Copying File {source} to {destination}");

                string? parent = Path.GetDirectoryName(destination);

                if (parent != null)
                Directory.CreateDirectory(parent);

                File.Copy(source, destination, overwrite: true);
            }

            //do script mappings
            Console.WriteLine("Performing scripts copying");
            foreach (KeyValuePair<string, string> fileMappings in config.Scripts)
            {
                string source = fileMappings.Key;
                if (!File.Exists(source))
                {
                    Console.WriteLine($"{source} does not exists, Skipping");
                    continue;
                }
                string destination = fileMappings.Value;

                Console.WriteLine($"Copying Script {source} to {destination}");

                string? parent = Path.GetDirectoryName(destination);

                if (parent != null)
                Directory.CreateDirectory(parent);

                File.Copy(source, destination, overwrite: true);
            }

            //run prebuild commands
            foreach (string command in config.PreBuildCommands)
            {
                Console.WriteLine($"Running: {command}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{command}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    }
                };

                process.Start();

                Console.WriteLine(process.StandardOutput.ReadToEnd());
                Console.Error.WriteLine(process.StandardError.ReadToEnd());

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Command failed: {command}");
                }
            }
            //run dpkg-deb --build packagepath by using control.package property
            Console.WriteLine("Packaging...");
            string build_command=$"dpkg-deb --build {DebBuilderCompletePath}/{config.Control.Package}";
            var build_process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{build_command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            build_process.Start();

            Console.WriteLine(build_process.StandardOutput.ReadToEnd());
            Console.Error.WriteLine(build_process.StandardError.ReadToEnd());

            build_process.WaitForExit();

            if (build_process.ExitCode != 0)
            {
                throw new Exception($"Command failed: {build_command}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
