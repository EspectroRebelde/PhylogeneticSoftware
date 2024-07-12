using PhylogeneticApp.Utils.ColorAssignments;

namespace PhylogeneticApp.Templates;

public interface _IOutputGenerator
{
    /// <summary>
    /// Calls a specific <b>OutputDelegate</b> to generate the output file.
    /// The default implementation is to generate a DOT file.
    /// </summary>
    /// <param name="pairs"> The pairs to be written to the file. </param>
    /// <param name="folderPath"> The path to the output folder. </param>
    /// <param name="filename"> The name of the file to be created. </param>
    /// <returns> The <b>OutputDelegate</b> creates a file at the <b>OutputFolder</b> location with a given structure. </returns>
    public void GenerateOutput(
        in DataStructures.GeneticStructures.OutputPair[] pairs, 
        in string[] headers, 
        in string folderPath, 
        string[] filesProcessed,
        out string dotPath, 
        in string filename = "Output.dot");
}