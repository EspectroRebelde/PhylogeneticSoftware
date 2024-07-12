namespace PhylogeneticApp.Templates;

public interface _IPairing
{
    void GeneratePairs(in string[] headers, in float[] correlationMatrix, ref DataStructures.GeneticStructures.OutputPair[] pairs);
}