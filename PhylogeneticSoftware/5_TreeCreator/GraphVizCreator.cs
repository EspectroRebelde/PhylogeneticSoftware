using System.Diagnostics;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.TreeCreator;

public class GraphvizTreeCreator : _ITreeCreator
{
    public void GenerateTree(in string inputFile, in string outputFolder, out string outputPath, string filename = "Tree.svg")
    {
        string dotPath = @"C:\Program Files\Graphviz\bin\dot.exe";

        // Get the extension of the file (graphOutputLocation)
        string extension = Path.GetExtension(filename);
        outputPath = $"{outputFolder}\\{filename}";
        // Create the command depending on the extension
        string command = extension switch
        {
            ".svg" => $"-Tsvg \"{inputFile}\" -o \"{outputPath}\"",
            ".png" => $"-Tpng \"{inputFile}\" -o \"{outputPath}\"",
            ".jpg" => $"-Tjpg \"{inputFile}\" -o \"{outputPath}\"",
            ".gif" => $"-Tgif \"{inputFile}\" -o \"{outputPath}\"",
            ".pdf" => $"-Tpdf \"{inputFile}\" -o \"{outputPath}\"",
            _ => throw new Exception("Invalid extension")
        };
        ProcessStartInfo startInfo = new ProcessStartInfo(dotPath, command)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = false
        };

        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();

        Console.WriteLine(process.ExitCode != 0
            ? $"Error generating tree: {process.ExitCode}"
            : "Succesfully generated tree");
    }
}