using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.CorrelationMatrixDistance.NotAligned;

public class EuclideanDistance : _IDistance<float>
{
    public bool IsNonAlignedEncoding => true;

    public void ConvertToCorrelationMatrix(in DataStructures.GeneticEncoding<float> geneticEncoding, out float[] correlationMatrix)
    {
        int total = geneticEncoding.Genes.Length * geneticEncoding.Genes.Length;
        correlationMatrix = new float[total];
        
        using (Utils.ProgressBar progress = new Utils.ProgressBar())
        {
            // Set diagonal to 0
            for (int i = 0; i < geneticEncoding.Genes.Length; i++)
            {
                correlationMatrix[i * geneticEncoding.Genes.Length + i] = 0;
            }

            float max = 0;

            for (int i = 0; i < geneticEncoding.Genes.Length; i++)
            {
                for (int j = i + 1; j < geneticEncoding.Genes.Length; j++)
                {
                    progress.Report((float) (i * geneticEncoding.Genes.Length + j) / total);
                    float distance =
                        CalculateDistance(in geneticEncoding.Genes[i], in geneticEncoding.Genes[j]);
                    correlationMatrix[i * geneticEncoding.Genes.Length + j] = distance;
                    correlationMatrix[j * geneticEncoding.Genes.Length + i] = distance;
                    
                    if (distance > max)
                    {
                        max = distance;
                    }
                }
            }
            
            // Normalize the values
            for (int i = 0; i < correlationMatrix.Length; i++)
            {
                correlationMatrix[i] /= max;
            }
        }
    }

    public void ParallelConvertToCorrelationMatrix(in GeneticEncoding<float> geneticEncoding, out float[] correlationMatrix)
    {
        int numberOfElements = geneticEncoding.Genes.Length * geneticEncoding.Genes.Length;
        correlationMatrix = new float[numberOfElements];
        // Total number of executions (triangular matrix minus the diagonal)
        int totalExecutions = (geneticEncoding.Genes.Length * (geneticEncoding.Genes.Length - 1)) / 2;
        int progressCount = 0;

        using (Utils.ProgressBar progress = new Utils.ProgressBar())
        {
            // Set diagonal to 0
            for (int i = 0; i < geneticEncoding.Genes.Length; i++)
            {
                correlationMatrix[i * geneticEncoding.Genes.Length + i] = 0;
            }

            var matrix = correlationMatrix;
            var encoding = geneticEncoding;
            float max = 0;
            Parallel.For(0, numberOfElements, i =>
            {
                if (Utils.General.IsPositionInUpperTriangleNotDiagonal(i, encoding.Genes.Length))
                {
                    int row = i / encoding.Genes.Length;
                    int column = i % encoding.Genes.Length;
                    float distance = CalculateDistance(in encoding.Genes[row], in encoding.Genes[column]);
                    matrix[row * encoding.Genes.Length + column] = distance;
                    matrix[column * encoding.Genes.Length + row] = distance;
                    if (distance > max)
                    {
                        max = distance;
                    }
                    progress.Report((float) progressCount++ / totalExecutions);
                }
            });
            
            // Normalize the values
            for (int i = 0; i < correlationMatrix.Length; i++)
            {
                correlationMatrix[i] /= max;
            }
        }
    }

    public float CalculateDistance(in GeneStripped<float> gene1, in GeneStripped<float> gene2)
    {
        float sum = 0;
        int index = 0;
        int leftover = 0;
        // Iterate through the smallest
        if (gene1.Values.Length < gene2.Values.Length)
        {
            leftover = gene2.Values.Length - gene1.Values.Length;
            for (index = 0; index < gene1.Values.Length; index++)
            {
                sum += (gene1.Values[index] - gene2.Values[index]) * (gene1.Values[index] - gene2.Values[index]);
            }
            // The leftover values will be compared to 0
            for (int i = 0; i < leftover; i++)
            {
                sum += gene2.Values[index + i] * gene2.Values[index + i];
            }
        }
        else
        {
            leftover = gene1.Values.Length - gene2.Values.Length;
            for (index = 0; index < gene2.Values.Length; index++)
            {
                sum += (gene1.Values[index] - gene2.Values[index]) * (gene1.Values[index] - gene2.Values[index]);
            }
            // The leftover values will be compared to 0
            for (int i = 0; i < leftover; i++)
            {
                sum += gene1.Values[index + i] * gene1.Values[index + i];
            }
        }
        
        return (float) Math.Sqrt(sum);
    }
}