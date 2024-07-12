using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.Ownership;

public class BasicOwnershipFloat : _IOwnershipGenerator<float>
{
    public void GenerateOwnership(in string pathToOutputFolder, in DataStructures.Gene<float>[] genes, string fileName = "Ownership.csv")
    {
        DataStructures.GeneticEncoding<float> geneticEncoding = new DataStructures.GeneticEncoding<float>();
        
        geneticEncoding.TransferGenes(genes, 0);
        
        // Foreach Genes in the GeneticEncoding, if the value is 1 or higher, make it 1
        // If the value is 0 or lower, make it 0
        foreach (var geneStripped in geneticEncoding.Genes)
        {
            for (int j = 0; j < geneStripped.Values.Length; j++)
            {
                if (geneStripped.Values[j] >= 1)
                {
                    geneStripped.Values[j] = 1;
                }
                else
                {
                    geneStripped.Values[j] = 0;
                }
            }
        }
        
        geneticEncoding.WriteGenes(pathToOutputFolder, fileName);
    }
}