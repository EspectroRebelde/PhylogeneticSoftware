using System.Xml;
using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.Genetics;

public class scrML_NA_Kmer_IntegerEncoding : _IEncodingGenerator<float>
{
    private Dictionary<string, float> _kMerMapping;
    private readonly int kmerSize = 2;
    public override bool IsNonAlignedEncoding => true;

    public override float DefaultValueForGene()
    {
        return 0;
    }

    public override void Initialize()
    {
        _kMerMapping = new Dictionary<string, float>();
        List<string> singleValues = new List<string>();
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
                        if (singleValues.Contains(tag))
                        {
                            continue;
                        }
                        singleValues.Add(tag);
                    }
                }
            }
        }
        
        // Generate all possible kmers (2)^kmerSize
        foreach (var permutation in Utils.PermutationGenerator.PermutationsWithRepetition(singleValues.ToArray(), kmerSize))
        {
            string kmer = string.Join("", permutation);
            _kMerMapping[kmer] = _kMerMapping.Count;
        }
    }

    protected override bool GenerateGene(in string filePath, out Gene<float> gene)
    {
        // We will encode the gene as a chain of integers, where each integer represents a kmer-natural encoding (grouping in kmers)
        List<string> geneList = new List<string>();
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        XmlNodeList nodes = doc.SelectNodes("//*");
        foreach (XmlNode node in nodes)
        {
            // The tag to search will be the concatenation of kmerSize tags
            int index = 0;
            string[] kmer = new string[kmerSize];
            if (node.NodeType != XmlNodeType.Element)
            {
                continue;
            }
            kmer[index] = node.Name;
            index++;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (index == kmerSize)
                {
                    break;
                }
                if (child.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                
                kmer[index] = child.Name;
                index++;
            }

            if (index == kmerSize)
            {
                string kmerString = string.Join("", kmer);
                geneList.Add(kmerString);
            }
        }
        
        // Count the occurrences of each element
        var elementCounts = geneList.GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());
        
        // Create the list of frequencies
        List<float> frequencyList = geneList.Select(x => (float)elementCounts[x] / geneList.Count).ToList();
        
        gene = new Gene<float>
        {
            Name = Path.GetFileName(filePath),
            Value = frequencyList.ToArray(),
            // Number from 1 to n, where n is the lenght of the gene
            Headers = Enumerable.Range(1, geneList.Count).Select(x => x.ToString()).ToArray()
        };
        
        return false;
    }
}