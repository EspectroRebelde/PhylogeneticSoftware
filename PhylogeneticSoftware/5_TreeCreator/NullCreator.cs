using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.OutputGenerator;

/// <summary>
/// Used when the output is all we need
/// </summary>
public class NullCreator : _ITreeCreator
{
    public void GenerateTree(in string inputFile, in string outputFolder, out string outputPath, string filename = "Tree.svg")
    {
        outputPath = "";
        return;
    }
}