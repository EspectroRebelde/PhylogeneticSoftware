using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Templates;
using System.Xml;

namespace PhylogeneticApp.Implementations.Genetics;

public class srcML_C_PerTagEncoding : _IEncodingGenerator<float>
{
    // Tags to catch the information from the scrML
    private readonly string[] tags = new[]
    {
        "function",
    };
    
    public override float DefaultValueForGene()
    {
        return 0;
    }

    /// <summary>
    /// An xml file with the scrML is passed and we will grab the information from it
    /// We will define the tags to catch the information from
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="gene"></param>
    /// <returns></returns>
    protected override bool GenerateGene(in string filePath, out Gene<float> gene)
    {
        gene = new Gene<float>(tags.Length);
        Dictionary<string, float> geneValues = new Dictionary<string, float>();
        // We will initialize the gene values with 0
        foreach (var tag in tags)
        {
            geneValues[tag] = 0;
        }
        // We will read the file and get the information from it
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        // Get all the nodes with the tags we want to catch
        foreach (var tag in tags)
        {
            //If the XML is the tag <TAG> then we will get the information from it
            XmlNodeList nodes = doc.GetElementsByTagName(tag);
            geneValues[tag] = nodes.Count;
        }

        gene.Name = Path.GetFileName(filePath);
        gene.Headers = tags;
        gene.Value = geneValues.Values.ToArray();
        
        return false;
    }
    
    public bool GenerateSingleGene(in string filePath, out Gene<float> gene)
    {
        return GenerateGene(filePath, out gene);
    }
}