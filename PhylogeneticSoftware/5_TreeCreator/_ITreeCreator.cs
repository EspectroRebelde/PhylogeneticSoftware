namespace PhylogeneticApp.Templates;

public interface _ITreeCreator
{
    void GenerateTree(in string inputFile, in string outputFolder, out string outputPath, string filename = "Tree.svg");
}