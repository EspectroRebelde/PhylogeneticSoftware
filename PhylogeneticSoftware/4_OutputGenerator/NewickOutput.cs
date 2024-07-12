using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.OutputGenerator;

public class NewickOutput: _IOutputGenerator
{
    public void GenerateOutput(in GeneticStructures.OutputPair[] pairs, in string[] headers, in string folderPath, string[] filesProcessed, out string dotPath, in string filename = "Output.dot")
    {        
        dotPath = $"{folderPath}\\{"Output.tree"}";
        using (StreamWriter outfile = new StreamWriter(dotPath))
        {
            // We also save the nodes id to later relate to them without having to search for them
            Dictionary<string, NewickPair> nodes = new Dictionary<string, NewickPair>();
            string[] headersToProcess = headers.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            // We process the results
            for (int i = 0; i < pairs.Length - 1; i++)
            {
                GeneticStructures.OutputPair currentResult = pairs[i];

                nodes.Add((i + headersToProcess.Length).ToString(), new NewickPair(
                    i + headersToProcess.Length,
                    currentResult.First,
                    currentResult.Second,
                    currentResult.Value
                    ));
            }
            
            GeneticStructures.OutputPair lastResult = pairs[^1];
            string firstHeaderLast = lastResult.First;
            string secondHeaderLast = lastResult.Second;

            string newick = getNewick(firstHeaderLast, secondHeaderLast, lastResult.Value, ref nodes) + ";";
            
            outfile.Write( newick );
            outfile.Close();
            
            Console.WriteLine($"File created at {dotPath}");
        }
    }
    
    public string getNewick(string left, string right, float diff, ref Dictionary<string, NewickPair> nodes)
    {
        string leftPart = left;
        NewickPair leftPair;
        if (nodes.TryGetValue(left, out leftPair))
        {
            leftPart = getNewick(leftPair.left, leftPair.right, leftPair.diff, ref nodes);
        }

        string rightPart = right;
        NewickPair rightPair;
        if (nodes.TryGetValue(right, out rightPair))
        {
            rightPart = getNewick(rightPair.right, rightPair.left, rightPair.diff, ref nodes);
        }
        return $"({leftPart},{rightPart}):{diff.ToString().Replace(',', '.')}";
    }
}