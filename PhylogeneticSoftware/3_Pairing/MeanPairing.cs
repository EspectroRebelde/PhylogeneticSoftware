using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.Pairing;

public class MeanPairing : _IPairing
{
    public void GeneratePairs(in string[] headers, in float[] correlationMatrix,
        ref DataStructures.GeneticStructures.OutputPair[] pairs)
    {
        pairs = Array.Empty<DataStructures.GeneticStructures.OutputPair>();
        // Create a value copy of the headers
        string[] elementNames = new string[headers.Length];
        headers.CopyTo(elementNames, 0);
        LoadDictionary(elementNames, in correlationMatrix, ref pairs);
    }

    private void LoadDictionary(string[] headers, in float[] correlationMatrix,
        ref DataStructures.GeneticStructures.OutputPair[] returnPairs)
    {
        int n = headers.Length;

        // Make the matrix's diagonal -1
        for (int i = 0; i < n; i++)
        {
            correlationMatrix[i * n + i] = -1;
        }

        using (Utils.ProgressBar progress = new Utils.ProgressBar())
        {
            int items = CountComparableElements(correlationMatrix);
            int startingItems = items;
            int iteration = 0;
            while (items > 1)
            {
                // Step 2: Identify the pair with the highest correlation
                Tuple<int, int> maxCorrelationIndices = FindMaxCorrelation(correlationMatrix, headers);

                // Step 4: Create an OutputPair and store the information
                GeneticStructures.OutputPair outputPair = new GeneticStructures.OutputPair
                {
                    First = headers[maxCorrelationIndices.Item1],
                    Second = headers[maxCorrelationIndices.Item2],
                    Value = correlationMatrix[maxCorrelationIndices.Item1 * n + maxCorrelationIndices.Item2]
                };

                // Output the result or store it as needed
                returnPairs = returnPairs.Append(outputPair).ToArray();

                // Replace the existing row and column with the combined values
                ReplaceRowAndColumn(correlationMatrix, maxCorrelationIndices.Item1, maxCorrelationIndices.Item2,
                    ref headers, iteration + n);

                // Set the second element to zero (or any sentinel value)
                correlationMatrix[maxCorrelationIndices.Item2 * n + maxCorrelationIndices.Item2] = -1;
                items = CountComparableElements(correlationMatrix);
                iteration++;

                // Update the progress bar (original items - current items) / original items
                progress.Report((float)(startingItems - items) / startingItems);
            }

            if (items > 0)
            {
                // Search for the last element in the matrix
                Tuple<int, int> lastCorrelationIndices = FindMaxCorrelation(correlationMatrix, headers);

                // Output the last element
                GeneticStructures.OutputPair lastOutputPair = new GeneticStructures.OutputPair
                {
                    First = headers[lastCorrelationIndices.Item1],
                    Second = headers[lastCorrelationIndices.Item2],
                    Value = correlationMatrix[lastCorrelationIndices.Item1 * n + lastCorrelationIndices.Item2]
                };
                returnPairs = returnPairs.Append(lastOutputPair).ToArray();
                return;
            }
        }

        // Set diagonal to 0
        for (int i = 0; i < n; i++)
        {
            correlationMatrix[i * n + i] = 0;
        }
    }

    static void ReplaceRowAndColumn(float[] matrix, int rowIndex, int columnIndex, ref string[] elementNames,
        int iteration)
    {
        int n = elementNames.Length;

        // Add a new header for the combined element substituting the first element
        /*/
        string combinedElementName = elementNames[rowIndex] + "^" + elementNames[columnIndex];
        elementNames[rowIndex] = combinedElementName;
        elementNames[columnIndex] = "*"+elementNames[columnIndex];
        /*/
        elementNames[rowIndex] = iteration.ToString();
        elementNames[columnIndex] = "*" + elementNames[columnIndex];
        //*/

        // Replace the new row with the combined values
        // Since its a symmetrical matrix we replace the column as well
        for (int i = 0; i < n; i++)
        {
            int position = rowIndex * n + i;
            int oppositePosition = i * n + rowIndex;
            matrix[position] = (matrix[position] + matrix[columnIndex * n + i]) / 2;
            matrix[oppositePosition] = matrix[position];
            matrix[columnIndex * n + i] = -1;
        }

        // Set the diagonal to 0
        matrix[rowIndex * n + rowIndex] = -1;

        // Set the second elements row and column to -1 (or any sentinel value)
        for (int i = 0; i < n; i++)
        {
            matrix[columnIndex * n + i] = -1;
            matrix[i * n + columnIndex] = -1;
        }
    }

    // Helper method to find the indices of the maximum correlation in the matrix
    static Tuple<int, int> FindMaxCorrelation(float[] matrix, string[] headerNames)
    {
        int n = headerNames.Length;
        float maxCorrelation = 2;
        Tuple<int, int> maxIndices = null;
        for (int i = 0; i < n; i++)
        {
            // If the header has been merged (contains a *) we skip it
            if (headerNames[i].Contains('*'))
            {
                continue;
            }

            for (int j = i + 1; j < n; j++)
            {
                int position = i * n + j;
                if (matrix[position] < maxCorrelation && matrix[position] >= 0 && i != j)
                {
                    maxCorrelation = matrix[position];
                    maxIndices = Tuple.Create(i, j);
                }
            }
        }

        // Log the result
        // Console.WriteLine("Max correlation: " + maxCorrelation);
        return maxIndices;
    }

    /// <summary>
    /// Returns the number of elements that are not lower than 0 in the matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="lenght"></param>
    /// <returns></returns>
    /// <remarks>Divided by 2 (as matrix should be symmetrical)</remarks>
    static int CountComparableElements(float[] matrix)
    {
        return matrix.Count(x => x >= 0) / 2;
    }
}