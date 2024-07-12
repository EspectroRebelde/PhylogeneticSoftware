using System.Xml;
using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.Genetics;

public class scrML_NA_TagDistanceEncoding : _IEncodingGenerator<float>
{
    private Dictionary<string, float> _tagIntegerMapping;
    public override bool IsNonAlignedEncoding => true;

    public override float DefaultValueForGene()
    {
        return 0;
    }

    public override void Initialize()
    {
        
    }

    protected override bool GenerateGene(in string filePath, out Gene<float> gene)
    {
        // We will encode the gene as a chain of integers, where each integer represents a tag as we find them
        List<string> geneList = new List<string>();
        Dictionary<string, int> tagCounter = new Dictionary<string, int>();
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        XmlNodeList nodes = doc.SelectNodes("//*");
        foreach (XmlNode node in nodes)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                string tag = node.Name;
                geneList.Add(tag);
                tagCounter.TryAdd(tag, 0);
                tagCounter[tag]++;
            }
        }
        
        Dictionary<string, float> distanceMapping = new Dictionary<string, float>();
        // We will count the distance (number of tags) between each repeating tag (counter > 1)
        // First we will get the tags that repeat
        foreach (var tag in tagCounter)
        {
            distanceMapping.Add(tag.Key, 0);
        }
        
        // Now we will count the distance between each repeating tag
        foreach (var tag in distanceMapping)
        {
            float distance = 0;
            List<int> indexes = new List<int>();
            for (int i = 0; i < geneList.Count; i++)
            {
                if (geneList[i] == tag.Key)
                {
                    indexes.Add(i);
                }
            }
            
            if (indexes.Count > 1)
            {
                for (int i = 0; i < indexes.Count - 1; i++)
                {
                    distance += indexes[i + 1] - indexes[i];
                }
                distanceMapping[tag.Key] = distance / geneList.Count;
            }
        }
        
        List<float> geneListFloat = new List<float>();
        // We substitute the tags with the distance between them
        foreach (var tag in geneList)
        {
            geneListFloat.Add(distanceMapping[tag]);
        }
        
        gene = new Gene<float>
        {
            Name = Path.GetFileName(filePath),
            Value = geneListFloat.ToArray(),
            // Number from 1 to n, where n is the lenght of the gene
            Headers = Enumerable.Range(1, geneList.Count).Select(x => x.ToString()).ToArray()
        };
        
        return false;
    }
}