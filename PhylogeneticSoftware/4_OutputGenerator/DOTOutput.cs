using System.Globalization;
using PhylogeneticApp.Templates;
using PhylogeneticApp.Utils.ColorAssignments;

namespace PhylogeneticApp.Implementations.OutputGenerator;

public class DOTOutput : _IOutputGenerator
{
    private _IColorAssignment? colorAssignments;
    
    public void SetColorAssignment(_IColorAssignment? colorAssignment = null)
    {
        colorAssignments = colorAssignment;
    }
    
    public void GenerateOutput(in DataStructures.GeneticStructures.OutputPair[] pairs, in string[] headers, in string folderPath, string[] filesProcessed, out string dotPath, in string filename = "Output.dot")
    {
        DataStructures.GraphViz.Structures gf = new DataStructures.GraphViz.Structures();
        DataStructures.GraphViz.Visualization genericVisualization = new DataStructures.GraphViz.Visualization(false, true);

        Dictionary<string, Color> colors = new Dictionary<string, Color>();
        if (genericVisualization.useColorLabels && colorAssignments != null)
        {
            colorAssignments.AssignColors(headers, filesProcessed, out colors);
        }
        
        dotPath = $"{folderPath}\\{filename}";
        Dictionary<Tuple<string,string>, string> nodesEvaluation = new Dictionary<Tuple<string, string>, string>();
        using (StreamWriter outfile = new StreamWriter(dotPath))
        {
            outfile.Write(gf.Init(""));

            outfile.Write(gf.Parameters(
                "dot",
                "Phylogenetic Tree",
                "major",
                "shortpath",
                "",
                "fill",
                "portrait"
            ));

            DataStructures.GraphViz.NodeConfig currentConfig = genericVisualization.baseNodeConfig;
            // We also save the nodes id to later relate to them without having to search for them
            Dictionary<string, string> nodes = new Dictionary<string, string>();
            string[] headersToProcess = headers.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            for (int i = 0; i < headersToProcess.Length; i++)
            {
                outfile.WriteLine("#" + i);
                string header = headersToProcess[i];
                String color = currentConfig.Color;
                if (genericVisualization.useColorLabels && colors.ContainsKey(header))
                {
                    // We assign the color from the header without the alpha
                    var backgroundColor = colors[header];
                    color = $"#{backgroundColor.R:x2}{backgroundColor.G:x2}{backgroundColor.B:x2}";
                    // Get the opposite color for the font
                    Color fontColor = Utils.ColorHelper.BlackOrWhiteFromPerceivedBrightness(colors[header]);
                    currentConfig.FontColor = $"#{fontColor.R:x2}{fontColor.G:x2}{fontColor.B:x2}";
                }
                
                // We add the nodes to the nodes dictionary if they are not already there
                outfile.WriteLine("\t" + gf.Node(
                    name: header,
                    id: (i).ToString(),
                    options: currentConfig.Options,
                    label: "",
                    shape: currentConfig.Shape,
                    style: currentConfig.Style,
                    color: color,
                    fontcolor: currentConfig.FontColor)
                );
                nodes.Add(header, (i).ToString());
            }
            
            if (Options.Instance.Value.OutputOptions.OutputAlgorithmicEvaluation)
            {
                nodesEvaluation = nodes.ToDictionary(x => new Tuple<string, string>(x.Key, x.Key), x => GetHeaderCategory(x.Key, filesProcessed));
            }

            // We process the results
            currentConfig = genericVisualization.linkNodeConfig;
            for (int i = 0; i < pairs.Length - 1; i++)
            {
                outfile.WriteLine("#" + i);
                DataStructures.GeneticStructures.OutputPair currentResult = pairs[i];
                string firstHeader = currentResult.First;
                string secondHeader = currentResult.Second;

                // Inside each header, the combined headers are separated by a ^, we get the concatenatiopn of all of them in id
                string id = getIdFromHeader(firstHeader, ref nodes);
                string id2 = getIdFromHeader(secondHeader, ref nodes);

                // The new id is the concatenation of the ids of the headers
                string newId = (nodes.Count + i).ToString();

                if (Options.Instance.Value.OutputOptions.OutputAlgorithmicEvaluation)
                {
                    nodesEvaluation.Add(new Tuple<string, string>(firstHeader, secondHeader), newId);
                }
                
                outfile.WriteLine("\t" + gf.Node(
                    name: i.ToString(),
                    id: newId,
                    options: currentConfig.Options,
                    label: "",
                    tooltip: currentResult.Value.ToString(CultureInfo.InvariantCulture),
                    shape: currentConfig.Shape,
                    style: currentConfig.Style,
                    color: currentConfig.Color,
                    fontcolor: currentConfig.FontColor)
                );

                outfile.Write("\t");
                // We add the link between the first header and the new header
                outfile.WriteLine(gf.Edge(id, newId, "forward"));
                outfile.Write("\t");
                // We add the link between the second header and the new header
                outfile.WriteLine(gf.Edge(id2, newId, "forward"));
            }

            currentConfig = genericVisualization.finalNodeConfig;
            outfile.WriteLine($"#{pairs.Length - 1}");
            DataStructures.GeneticStructures.OutputPair lastResult = pairs[^1];
            string firstHeaderLast = lastResult.First;
            string secondHeaderLast = lastResult.Second;
            string idLast = getIdFromHeader(firstHeaderLast, ref nodes);
            string id2Last = getIdFromHeader(secondHeaderLast, ref nodes);
            string newIdLast = (nodes.Count + pairs.Length - 1).ToString();
            outfile.WriteLine("\t" + gf.Node(
                name: (pairs.Length - 1).ToString(),
                id: newIdLast,
                options: currentConfig.Options,
                label: "",
                tooltip: lastResult.Value.ToString(CultureInfo.InvariantCulture),
                shape: currentConfig.Shape,
                style: currentConfig.Style,
                color: currentConfig.Color,
                fontcolor: currentConfig.FontColor)
            );

            if (Options.Instance.Value.OutputOptions.OutputAlgorithmicEvaluation)
            {
                nodesEvaluation.Add(new Tuple<string, string>(firstHeaderLast, secondHeaderLast), newIdLast);
            }

            outfile.Write("\t");
            outfile.WriteLine(gf.Edge(idLast, newIdLast, "forward"));
            outfile.Write("\t");
            outfile.WriteLine(gf.Edge(id2Last, newIdLast, "forward"));

            outfile.WriteLine();

            outfile.Write(gf.End());
            
            outfile.Close();
            
            Console.WriteLine($"File created at {dotPath}");
        }
        
        if (Options.Instance.Value.OutputOptions.OutputAlgorithmicEvaluation)
        {
            OutputEvaluation(nodesEvaluation);
        }
    }
    
    private static string getIdFromHeader(string header, ref Dictionary<string, string> nodes)
    {
        string[] headers = header.Split('^');
        string id = "";

        //If the header is already a number we return it
        if (int.TryParse(header, out _))
        {
            return header;
        }

        if (headers.Length == 1)
        {
            id = nodes[header];
        }
        else
        {
            id = nodes[headers[0]];
            for (var index = 1; index < headers.Length; index++)
            {
                var h = headers[index];
                id += nodes[h];
            }
        }

        return id;
    }
    
    private void OutputEvaluation(Dictionary<Tuple<string, string>, string> nodesEvaluation)
    {
        // We will evaluate the changes in color
        DataStructures.Tree tree = DataStructures.Tree.ConstructTree(nodesEvaluation);
        tree.GetTreeAnalysis();
    }
    
    private string GetHeaderCategory(string header, in string[] filesProcessed, int parentAltitude = 2)
    {
        // Depending on the parent folder of the file, we will assign a category (name of the parent folder)
        string category = "";
        foreach (var file in filesProcessed)
        {
            if (file.Contains(header))
            {
                string[] folders = file.Split('\\');
                if (folders.Length > parentAltitude)
                {
                    category = folders[^parentAltitude];
                }
                break;
            }
        }
        
        return category;
    }
}