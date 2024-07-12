namespace PhylogeneticApp.Templates;

#nullable disable

public interface _IOwnershipGenerator<T>
{
    void GenerateOwnership(in string pathToOutputFolder, in DataStructures.Gene<T>[] genes, string fileName = "Ownership.csv");

    void WriteOwnership(string pathToOutputFolder, in DataStructures.GeneticEncoding<T> geneticEncoding,
        string fileName = "Ownership.csv")
    {
        geneticEncoding.WriteGenes(pathToOutputFolder, fileName);
    }
}

#nullable restore

