using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Templates;
using System.Xml;

namespace PhylogeneticApp.Implementations.Genetics;

public class srcML_C_FullEncoding : _IEncodingGenerator<float>
{
    private readonly bool normalizeValues = true;
    public override bool IsNonAlignedEncoding => true;

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
        Dictionary<string, float> geneValues = new Dictionary<string, float>();
        // We will read the file and get the information from it
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        // Get all the nodes, except the ones we want to skip
        XmlNodeList nodes = doc.SelectNodes("//*");
        int total = 0;
        foreach (XmlNode node in nodes)
        {
            total++;
            if (!geneValues.TryAdd(node.Name, 1))
            {
                geneValues[node.Name]++;
            }
        }
        
        // Normalize the values
        if (normalizeValues)
        {
            foreach (var key in geneValues.Keys.ToList())
            {
                geneValues[key] /= total;
            }
        }

        gene = new Gene<float>(geneValues.Count);
        gene.Name = Path.GetFileName(filePath);
        gene.Headers = geneValues.Keys.ToArray();
        gene.Value = geneValues.Values.ToArray();
        
        return false;
    }
    
    public bool GenerateSingleGene(in string filePath, out Gene<float> gene)
    {
        return GenerateGene(filePath, out gene);
    }
}