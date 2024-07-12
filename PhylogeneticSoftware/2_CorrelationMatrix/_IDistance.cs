using System.Runtime.CompilerServices;

namespace PhylogeneticApp.Templates;

public interface _IDistance<T>
{
    /// <summary>
    /// If the encoding is non-aligned, override this property to return true
    /// </summary>
    public virtual bool IsNonAlignedEncoding => false;
    
    public virtual void ConvertToCorrelationMatrix(in DataStructures.GeneticEncoding<T> geneticEncoding, out float[] correlationMatrix)
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

            for (int i = 0; i < geneticEncoding.Genes.Length; i++)
            {
                for (int j = i + 1; j < geneticEncoding.Genes.Length; j++)
                {
                    progress.Report((float) (i * geneticEncoding.Genes.Length + j) / total);
                    float distance =
                        CalculateDistance(in geneticEncoding.Genes[i], in geneticEncoding.Genes[j]);
                    correlationMatrix[i * geneticEncoding.Genes.Length + j] = distance;
                    correlationMatrix[j * geneticEncoding.Genes.Length + i] = distance;
                }
            }
        }
    }
    
    public virtual void ParallelConvertToCorrelationMatrix(in DataStructures.GeneticEncoding<T> geneticEncoding,
        out float[] correlationMatrix)
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
            Parallel.For(0, numberOfElements, i =>
            {
                if (Utils.General.IsPositionInUpperTriangleNotDiagonal(i, encoding.Genes.Length))
                {
                    int row = i / encoding.Genes.Length;
                    int column = i % encoding.Genes.Length;
                    float distance = CalculateDistance(in encoding.Genes[row], in encoding.Genes[column]);
                    matrix[row * encoding.Genes.Length + column] = distance;
                    matrix[column * encoding.Genes.Length + row] = distance;
                    progress.Report((float) progressCount++ / totalExecutions);
                }
            });
        }
    }
    
    /*
     var parallelOptions = new ParallelOptions {MaxDegreeOfParallelism = Environment.ProcessorCount};
            var job = Parallel.For(0, numberOfElements, i =>
            {
                int row = i / encoding.Genes.Length;
                int column = i % encoding.Genes.Length;
                if (row < column)
                {
                    float distance = IDistanceAlgorithm.CalculateDistance(in encoding.Genes[row], in encoding.Genes[column]);
                    matrix[row * encoding.Genes.Length + column] = distance;
                    matrix[column * encoding.Genes.Length + row] = distance;
                }
            });

     */
    
    /// <summary>
    /// Calculate the distance between two genes.
    /// </summary>
    /// <param name="gene1"> First gene to compare. </param>
    /// <param name="gene2"> Second gene to compare. </param>
    /// <returns></returns>
    /// <remarks> When implementing this method, ensure that the distance is normalized between 0 and 1. <br/>
    /// Also, add: <br/><b>[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]</b><br/> attribute to the method. </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    float CalculateDistance(in DataStructures.GeneStripped<T> gene1, in DataStructures.GeneStripped<T> gene2);
}