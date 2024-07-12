using System.Xml;
using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.Genetics;

public class scrML_NA_Kmer_LettersEncoding : _IEncodingGenerator<string>
{
    private Dictionary<string, string> _kMerMapping;
    private readonly int kmerSize = 2;
    public override bool IsNonAlignedEncoding => true;

    public override string DefaultValueForGene()
    {
        return "";
    }

    public override void Initialize()
    {
        _kMerMapping = new Dictionary<string, string>();
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
        
        //Number of possible kmers
        int possibleKmers = (int)Math.Pow(singleValues.Count, kmerSize);
        int alphabetSize = (int)Math.Ceiling(Math.Log(possibleKmers, 26));
        int[] initialCharacters;
        if (alphabetSize == 1)
        {
            initialCharacters = new int[] {65};
        }
        else
        {
            initialCharacters = new int[alphabetSize];
            for (int i = 0; i < alphabetSize; i++)
            {
                initialCharacters[i] = 65;
            }
        }
        int currentModifyingIndex = 0;
        // Generate all possible kmers (2)^kmerSize
        foreach (var permutation in Utils.PermutationGenerator.PermutationsWithRepetition(singleValues.ToArray(), kmerSize))
        {
            string kmer = string.Join("", permutation);
            // The new kmer will be encoded as a string of letters of the alphabet the first being A times the needed size
            // So, for unitunit itll be AA, unitcomment will be AB, unitynamespace will be AC, etc
            if (initialCharacters[currentModifyingIndex] == 91)
            {
                currentModifyingIndex++;
                while (initialCharacters[currentModifyingIndex] == 90)
                {
                    initialCharacters[currentModifyingIndex] = 65;
                    currentModifyingIndex++;
                }
                initialCharacters[currentModifyingIndex]++;
                currentModifyingIndex = 0;
                initialCharacters[0] = 65;
            }
            
            string encodedKmer = string.Join("", initialCharacters.Select(x => (char)x));
            _kMerMapping.Add(kmer, encodedKmer); 
            initialCharacters[currentModifyingIndex]++;
        }
    }

    protected override bool GenerateGene(in string filePath, out Gene<string> gene)
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
                geneList.Add(_kMerMapping[kmerString]);
            }
        }
        
        gene = new Gene<string>
        {
            Name = Path.GetFileName(filePath),
            Value = geneList.ToArray(),
            // Number from 1 to n, where n is the lenght of the gene
            Headers = Enumerable.Range(1, geneList.Count).Select(x => x.ToString()).ToArray()
        };
        
        return false;
    }
}