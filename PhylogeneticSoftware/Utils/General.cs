using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Pastel;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Utils;

public static class General
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileExtensions"></param>
    /// <param name="foldersToProcess"></param>
    /// <param name="recursiveLookup"></param>
    /// <param name="completeLookup"></param>
    /// <returns></returns>
    /// <remarks> For any folder search, ensure that the .folder is the first element in the fileExtensions array </remarks>
    public static string[] RetrieveFilesOfType(string[] fileExtensions, string[] foldersToProcess, bool recursiveLookup = false, bool completeLookup = false)
    {
        List<string> files = new List<string>();
        if (fileExtensions.Length == 0)
        {
            return files.ToArray();
        }
        
        if (fileExtensions[0] == ".directory" || fileExtensions[0] == ".folder")
        {
            foreach (string folder in foldersToProcess)
            {
                string[] subFolders = Directory.GetDirectories(folder, "*.*",
                    recursiveLookup ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                
                files.AddRange(subFolders);
            }
        }
        else
        {
            foreach (string folder in foldersToProcess)
            {
                string[] filesInPath = Directory.GetFiles(folder, "*.*",
                        recursiveLookup ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(file => fileExtensions.Contains(
                        completeLookup ? PathExtensions.PathExtensions.GetExtensionReversed(file).ToLower() : Path.GetExtension(file).ToLower()
                    )).ToArray();
                
                files.AddRange(filesInPath);
            }
        }
        
        return files.ToArray();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static bool IsPositionInUpperTriangleNotDiagonal([NotNull] int position, [NotNull] int length)
    {
        int row = position / length;
        int column = position % length;
        return row < column;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PrintStopwatchData(Stopwatch stopwatch, string naming, bool reset = true, bool stop = false)
    {
        if (stop)
        {
            stopwatch.Stop();
        }
        string text = "- " + naming + ": " + stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff");
        ConsoleLogging.PrintInfoToConsole(text);
        if (reset && !stop)
        {
            stopwatch.Reset();
        }
    }
    
    //Extension of OptionsDefinitions.GroupOperations.GroupOperationType to string
    public static string GroupOperationTypeToString(OptionsDefinitions.SGeneticalOptions.SGroupOperations.GroupOperationType groupOperationType)
    {
        return groupOperationType switch
        {
            OptionsDefinitions.SGeneticalOptions.SGroupOperations.GroupOperationType.None => "None",
            OptionsDefinitions.SGeneticalOptions.SGroupOperations.GroupOperationType.Union => "Union",
            OptionsDefinitions.SGeneticalOptions.SGroupOperations.GroupOperationType.Intersection => "Intersection",
            OptionsDefinitions.SGeneticalOptions.SGroupOperations.GroupOperationType.Difference => "Difference",
            OptionsDefinitions.SGeneticalOptions.SGroupOperations.GroupOperationType.Segregation => "Segregation" + Options.Instance.Value.GeneticOptions.GroupOperation.SegregationThreshold.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(groupOperationType), groupOperationType, null)
        };
    }
    
    public static void NewickTreeMerger(string folderToSearch, string fileName = "MergedTree.tree")
    {
        string[] files = Directory.GetFiles(folderToSearch, "*.tree", SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            ConsoleLogging.PrintErrorToConsole("No newick files found in the folder");
            return;
        }

        using StreamWriter writer = new StreamWriter(folderToSearch + "\\" + fileName);
        {
            // Write the header
            writer.Write("#nexus\nbegin trees;\n");
            foreach (var file in files)
            {
                string folderName = Path.GetDirectoryName(file).Split(Path.DirectorySeparatorChar).Last();
                string tree = File.ReadAllText(file);
                // Substitute any whitespace with an underscore
                tree = tree.Replace(" ", "_");
                writer.Write("tree_" + folderName + " = " + tree + "\n");
            }
            writer.Write("end;");
        };
    }
}