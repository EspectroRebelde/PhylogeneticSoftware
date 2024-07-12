using System.Runtime.CompilerServices;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.CorrelationMatrixDistance.NotAligned;

/// <summary>
/// This Levenshtein will handle genes that are not the same lenght
/// When comparing outside the lenghts, it will consider the difference as the lenght of the longest comparing gene
/// </summary>
public class LevenshteinDistance : _IDistance<string>
{
    public bool IsNonAlignedEncoding => true;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public float CalculateDistance(in DataStructures.GeneStripped<string> gene1, in DataStructures.GeneStripped<string> gene2)
    {
        float totalDistance = 0;
        // Get the smallest gene
        if (gene1.Values.Length < gene2.Values.Length)
        {
            for (int i = 0; i < gene1.Values.Length; i++)
            {
                totalDistance += LevenshteinDistanceExecution(gene1.Values[i], gene2.Values[i]);
            }
            for (int i = gene1.Values.Length; i < gene2.Values.Length; i++)
            {
                totalDistance += LevenshteinDistanceExecution("", gene2.Values[i]);
            }
        }
        else
        {
            for (int i = 0; i < gene2.Values.Length; i++)
            {
                totalDistance += LevenshteinDistanceExecution(gene1.Values[i], gene2.Values[i]);
            }
            for (int i = gene2.Values.Length; i < gene1.Values.Length; i++)
            {
                totalDistance += LevenshteinDistanceExecution(gene1.Values[i], "");
            }
        }
        
        return totalDistance / Math.Max(gene1.Values.Length, gene2.Values.Length);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private float LevenshteinDistanceExecution(in string str1, in string str2)
    {
        int[,] distance = new int[str1.Length + 1, str2.Length + 1];

        // Initialize the first row and column
        for (int i = 0; i <= str1.Length; i++) {
            distance[i, 0] = i;
        }

        for (int j = 0; j <= str2.Length; j++) {
            distance[0, j] = j;
        }

        // Calculate Levenshtein distance
        for (int i = 1; i <= str1.Length; i++) {
            for (int j = 1; j <= str2.Length; j++) {
                int cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[str1.Length, str2.Length] / (float)Math.Max(str1.Length, str2.Length);
    }
}