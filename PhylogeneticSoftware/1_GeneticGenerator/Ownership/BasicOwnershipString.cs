using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.Ownership;

public class BasicOwnershipString : _IOwnershipGenerator<string>
{
    public void GenerateOwnership(in string pathToOutputFolder, in DataStructures.Gene<string>[] genes, string fileName = "Ownership.csv")
    {
        DataStructures.GeneticEncoding<string> geneticEncoding = new DataStructures.GeneticEncoding<string>();
        
        geneticEncoding.TransferGenes(genes, "");
        
        // Foreach Genes in the GeneticEncoding, if the value is 1 or higher, make it 1
        // If the value is 0 or lower, make it 0
        foreach (var geneStripped in geneticEncoding.Genes)
        {
            for (int j = 0; j < geneStripped.Values.Length; j++)
            {
                if (geneStripped.Values[j].Length > 1)
                {
                    geneStripped.Values[j] = geneStripped.Values[j].Substring(0, 1);
                }
            }
        }
        
        geneticEncoding.WriteGenes(pathToOutputFolder, fileName);
    }
}