using System.Xml;
using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.Genetics;

public class scrML_NA_IntegerEncoding : _IEncodingGenerator<float>
{
    private Dictionary<string, float> _tagIntegerMapping;
    public override bool IsNonAlignedEncoding => true;

    public override float DefaultValueForGene()
    {
        return 0;
    }

    public override void Initialize()
    {
        _tagIntegerMapping = new Dictionary<string, float>();
        // Get all .xml files from the foldersInProcess, get all the possible tags and assign an integer to each tag
        // This will be used to encode the genes
        foreach (var folder in foldersInProcess)
        {
            string[] files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                XmlNodeList nodes = doc.SelectNodes("//*");
                foreach (XmlNode node in nodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        string tag = node.Name;
                        if (!_tagIntegerMapping.ContainsKey(tag))
                        {
                            _tagIntegerMapping[tag] = _tagIntegerMapping.Count;
                        }
                    }
                }
            }
        }
    }

    protected override bool GenerateGene(in string filePath, out Gene<float> gene)
    {
        // We will encode the gene as a chain of integers, where each integer represents a tag as we find them
        List<float> geneList = new List<float>();
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        XmlNodeList nodes = doc.SelectNodes("//*");
        foreach (XmlNode node in nodes)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                string tag = node.Name;
                geneList.Add(_tagIntegerMapping[tag]);
            }
        }
        
        gene = new Gene<float>
        {
            Name = Path.GetFileName(filePath),
            Value = geneList.ToArray(),
            // Number from 1 to n, where n is the lenght of the gene
            Headers = Enumerable.Range(1, geneList.Count).Select(x => x.ToString()).ToArray()
        };
        
        return false;
    }
}