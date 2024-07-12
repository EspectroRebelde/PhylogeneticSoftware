using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using PhylogeneticApp.Templates;


namespace PhylogeneticApp.DataStructures;

/// <summary>
/// This struct represents the encoding of ALL the genetic information of a Gen2 implementation
/// <list type="Properties">
/// <item>
/// <description> <b>IGene[] Structure</b>: The structure of the genetic information</description>
/// </item>
/// <item>
/// <description> <b>string[] Headers</b>: The headers of the genetic information</description>
/// </item>
/// <item>
/// <description> <b>CONST Separator</b>: The separator used in the file</description>
/// </item>
/// <item>
/// <description> <b>CONST NewLine</b>: The new line character used in the file</description>
/// </item>
/// </list>
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="GeneticEncoding{T}" />
public struct GeneticEncoding<T>
{
    public string[] Headers;
    public GeneStripped<T>[] Genes;
    
    private const char Separator = ';';
    private const char NewLine = '\n';
    
    public void Deconstruct(out string[] types, out GeneStripped<T>[] genes)
    {
        types = Headers;
        genes = Genes;
    }
    public GeneticEncoding()
    {
        this.Headers = Array.Empty<string>();
        this.Genes = Array.Empty<GeneStripped<T>>();
    }
    
    public GeneticEncoding(GeneticEncoding<T> geneticEncoding)
    {
        this.Headers = geneticEncoding.Headers;
        this.Genes = geneticEncoding.Genes;
    }
    
    public void TransferGenes(in Gene<T>[] genesInput, in T defaultValue)
    {
        List<string> headers = new();
        foreach (var gene in genesInput)
        {
            // If the gene is empty, we skip
            if (gene.Headers.Length == 0)
            {
                continue;
            }
            foreach (var header in gene.Headers)
            {
                if (!headers.Contains(header))
                {
                    headers.Add(header);
                }
            }
        }

        Gene<T>[] genesInputFiltered = genesInput;
        if (Options.Instance.Value.GeneticOptions.DataAnalysis.IsDataAnalysis)
        {
            genesInputFiltered = DataAnalysisForGenes(in genesInput, ref headers,
                Options.Instance.Value.GeneticOptions.DataAnalysis.PercentileToKeepFromPopulation,
                Options.Instance.Value.GeneticOptions.DataAnalysis.OutlinerThresholdFromMedianMultiplier,
                Options.Instance.Value.GeneticOptions.DataAnalysis.OutlinerThresholdFromMedianMultiplier > 0);
            
        }

        Headers = headers.ToArray();
        Genes = new GeneStripped<T>[genesInputFiltered.Length];
        
        for (var i = 0; i < genesInputFiltered.Length; i++)
        {
            Genes[i] = new GeneStripped<T>(genesInputFiltered[i].Name, new T[Headers.Length]);
            // Fill with default value
            for (int j = 0; j < Genes[i].Values.Length; j++)
            {
                Genes[i].Values[j] = defaultValue;
            }

            for (int j = 0; j < genesInputFiltered[i].Headers.Length; j++)
            {
                Genes[i].Values[Array.IndexOf(Headers, genesInputFiltered[i].Headers[j])] = genesInputFiltered[i].Value[j];
            }
        }
    }
    
    public void TransferNAGenes(in Gene<T>[] genesInput)
    {
        int maxHeaders = 0;
        int jMinus = 0;
        // Direct translation since it's not aligned 
        Genes = new GeneStripped<T>[genesInput.Length];
        
        for (var index = 0; index < genesInput.Length; index++)
        {
            //If empty, we skip
            if (genesInput[index].Value.Length == 0)
            {
                jMinus++;
                continue;
            }
            if (genesInput[index].Value.Length > maxHeaders)
            {
                maxHeaders = genesInput[index].Value.Length;
            }
            Genes[index-jMinus] = new GeneStripped<T>(genesInput[index].Name, genesInput[index].Value);
        }
        // Remove the last jMinus elements
        Array.Resize(ref Genes, Genes.Length - jMinus);
        
        Headers = Enumerable.Range(1, maxHeaders).Select(x => x.ToString()).ToArray();
    }
    
    private Gene<T>[] DataAnalysisForGenes(in Gene<T>[] genes, ref List<string> headers, Tuple<float, float> percentileToKeepFromPopulation, float thresholdMultiplier, bool cutOutliers = true)
    {
        int totalLength = headers.Count;
        float minPercentile = percentileToKeepFromPopulation.Item1;
        float maxPercentile = percentileToKeepFromPopulation.Item2;
        bool usePercentileFromLenghtOfPopulation = Options.Instance.Value.GeneticOptions.DataAnalysis.UsePercentileFromLengthOfPopulation;

        List<Gene<T>> genesToKeep = new();
        if (cutOutliers)
        {
            double[] percentages = new double[genes.Length];
            for (var i = 0; i < genes.Length; i++)
            {
                var gene = genes[i];
                int geneLength = gene.Headers.Length;
                float percentage = (float)geneLength / totalLength;
                percentages[i] = percentage;
            }
        
            double median = CalculateMedian(percentages);
            double[] absoluteDeviations = percentages.Select(p => Math.Abs(p - median)).ToArray();
            double threshold = thresholdMultiplier * CalculateMedian(absoluteDeviations);;

            for (var i = 0; i < genes.Length; i++)
            {
                if (Math.Abs(percentages[i] - median) <= threshold)
                {
                    genesToKeep.Add(genes[i]);
                }
            }
        }
        
        //Percentile filter
        // This will go for the lenght of the population min * lenght to max * lenght
        List<Gene<T>> genesToKeepFromPercentile = genesToKeep;
        if (Options.Instance.Value.GeneticOptions.DataAnalysis.UsePercentiles)
        {
            if (usePercentileFromLenghtOfPopulation)
            {
                genesToKeep = genesToKeep.OrderBy(g => g.Headers.Length).ToList();
                int minIndex = (int)(genesToKeep.Count * minPercentile);
                int maxIndex = (int)(genesToKeep.Count * maxPercentile);
                for (int i = minIndex; i < maxIndex; i++)
                {
                    genesToKeepFromPercentile.Remove(genesToKeep[i]);
                }
            }
            // This will go for the percentage headers min * header.length to max * header.length
            else
            {
                int minCount = (int)(totalLength * minPercentile);
                int maxCount = (int)(totalLength * maxPercentile);
                foreach (var gene in genesToKeep)
                {
                    if (gene.Headers.Length < minCount && gene.Headers.Length > maxCount)
                    {
                        genesToKeepFromPercentile.Remove(gene);
                    }
                }
            }
        }
        
        // Get the headers from the genes to keep
        List<string> headersToKeep = new();
        foreach (var gene in genesToKeepFromPercentile)
        {
            foreach (var header in gene.Headers)
            {
                if (!headersToKeep.Contains(header))
                {
                    headersToKeep.Add(header);
                }
            }
        }
        
        headers = headersToKeep;
        return genesToKeepFromPercentile.ToArray();
    }
    
    static double CalculateMedian(double[] values)
    {
        Array.Sort(values);
        int n = values.Length;
        if (n % 2 == 0)
            return (values[n / 2 - 1] + values[n / 2]) / 2.0;
        else
            return values[n / 2];
    }
    
    /// <summary>
    /// Checks if the data is correctly formatted.
    /// </summary>
    /// <returns>A boolean indicating whether the components length is equal to the structure value length.</returns>
    private readonly bool IsDataCorrectlyFormatted()
    {
        return Headers.Length == Genes[0].Values.Length;
    }
    
    public readonly Tuple<bool, string> WriteGenes(string filePath, string filename = "Genetics.csv",  bool outputToConsole = true)
    {
        return WriteGeneticFile(filePath, filename , outputToConsole);
    }
    
    public readonly Tuple<bool, string> WriteNAGenes(string filePath, string filename = "Genetics.csv",  bool outputToConsole = true)
    {
        return WriteNAGeneticFile(filePath, filename , outputToConsole);
    }

    /// <summary>
    /// Writes the data of the GeneticStructure object to a file.
    /// </summary>
    /// <param name="filePath">The path of the file where the data will be written.</param>
    /// <param name="filename">Optional. The name of the file where the data will be written. Default is "Genetics.csv".</param>
    /// <param name="outputToConsole">Optional. A flag indicating whether the output should be written to the console. Default is true.</param>
    /// <returns> <b>Tuple (result,output)</b> <br></br> A Tuple containing a boolean and a string. The boolean indicates whether the file was written successfully. The string contains a log of the operations performed, including any error or warning messages.</returns>
    /// <exception cref="System.Exception">Thrown when an error occurs while writing to the file.</exception>
    private readonly Tuple<bool,string> WriteGeneticFile(string filePath, string filename = "Genetics.csv", bool outputToConsole = true)
    {
        StringBuilder consoleLog = new StringBuilder();
        outputToConsole = outputToConsole && Console.OpenStandardOutput() != Stream.Null;
        
#region Data Checkers
        
        if (!IsDataCorrectlyFormatted())
        {
            consoleLog.Append("The data length is not correct, lenghts should be equal" + NewLine);
            consoleLog.Append($"Headers length: {Headers.Length}{NewLine}");
            consoleLog.Append($"Genes length: {Genes[0].Values.Length}{NewLine}");
            if (outputToConsole)
            {
                Console.WriteLine(consoleLog);
            }

            return new Tuple<bool, string>(false, consoleLog.ToString());
        }

        // Check that the extension is .csv, if not, log a warning
        if (Path.GetExtension(filename) != ".csv")
        {
            string warning = "The file extension is not .csv" + NewLine;
            if (outputToConsole)
            {
                Console.WriteLine(warning);
            }
            consoleLog.Append(warning);
        }
#endregion

        using (StreamWriter sw = new StreamWriter(filePath + "\\" + filename))
        {
            
#region Write the components
            //First we clean the headers for any NewLine chars, Separator chars and " characters
            for (int i = 0; i < Headers.Length; i++)
            {
                sw.Write(Separator);
                Headers[i] = Headers[i]
                    .Replace(NewLine.ToString(), "")
                    .Replace(Separator.ToString(), "")
                    .Replace("\"", "");
                
                sw.Write(Headers[i]);

            }
#endregion

#region Write the structure
            for (int i = 0; i < Genes.Length; i++)
            {
                sw.Write(NewLine);
                sw.Write(Genes[i].Name);
                for (int j = 0; j < Genes[i].Values.Length; j++)
                {
                    sw.Write(Separator);
                    sw.Write(Genes[i].ValueToString(j));
                }
            }
#endregion

            if (outputToConsole)
            {
                Console.WriteLine(@"The" + filename + " file was written successfully");
            }
            consoleLog.Append("The " + filename + " file was written successfully" + NewLine);
            return new Tuple<bool, string>(true, consoleLog.ToString());
        }
    }
    
    private readonly Tuple<bool,string> WriteNAGeneticFile(string filePath, string filename = "Genetics.csv", bool outputToConsole = true)
    {
        StringBuilder consoleLog = new StringBuilder();
        outputToConsole = outputToConsole && Console.OpenStandardOutput() != Stream.Null;
        
        // Check that the extension is .csv, if not, log a warning
        if (Path.GetExtension(filename) != ".csv")
        {
            string warning = "The file extension is not .csv" + NewLine;
            if (outputToConsole)
            {
                Console.WriteLine(warning);
            }
            consoleLog.Append(warning);
        }

        using (StreamWriter sw = new StreamWriter(filePath + "\\" + filename))
        {
            //First we clean the headers for any NewLine chars, Separator chars and " characters
            for (int i = 0; i < Headers.Length; i++)
            {
                sw.Write(Separator);
                sw.Write(Headers[i]);
            }
            
            for (int i = 0; i < Genes.Length; i++)
            {
                sw.Write(NewLine);
                sw.Write(Genes[i].Name);
                for (int j = 0; j < Genes[i].Values.Length; j++)
                {
                    sw.Write(Separator);
                    sw.Write(Genes[i].ValueToString(j));
                }
            }
            
            if (outputToConsole)
            {
                Console.WriteLine(@"The" + filename + " file was written successfully");
            }
            consoleLog.Append("The " + filename + " file was written successfully" + NewLine);
            return new Tuple<bool, string>(true, consoleLog.ToString());
        }
    }

    public void ReadGenes(string filePath, bool outputToConsole = true)
    {
        ReadGeneticFile(filePath, outputToConsole);
    }

    /// <summary>
    /// Reads the data of the GeneticStructure object from a file.
    /// </summary>
    /// <param name="filePath">The path of the file where the data will be read.</param>
    /// <param name="outputToConsole">Optional. A flag indicating whether the output should be written to the console. Default is true.</param>
    /// <returns> <b>Tuple (result,output)</b> <br></br> A Tuple containing a boolean and a string. The boolean indicates whether the file was written successfully. The string contains a log of the operations performed, including any error or warning messages.</returns>
    /// <exception cref="System.Exception">Thrown when an error occurs while writing to the file.</exception>
    private void ReadGeneticFile(string filePath, bool outputToConsole = true)
    {
        StringBuilder consoleLog = new StringBuilder();
        outputToConsole = outputToConsole && Console.OpenStandardOutput() != Stream.Null;
        
        // Check that the extension is .csv, if not, log a warning
        if (Path.GetExtension(filePath) != ".csv")
        {
            string warning = "The file extension is not .csv" + NewLine;
            if (outputToConsole)
            {
                Console.WriteLine(warning);
            }
            consoleLog.Append(warning);
        }

        string[] fileLines = File.ReadAllLines(filePath);

        // Check if the file contains the necessary components
        if (fileLines.Length < 2)
        {
            string error = "The file is incomplete. Headers and genes are required." + NewLine;
            if (outputToConsole)
            {
                Console.WriteLine(error);
            }
            consoleLog.Append(error);
            return;
        }

        // Extract headers
        Headers = fileLines[^1].Split(Separator).Skip(1).ToArray();
        Genes = new GeneStripped<T>[fileLines.Length - 1];

        // Extract genes
        for (int i = 1; i < fileLines.Length; i++)
        {
            string[] geneData = fileLines[i].Split(Separator);
            string geneName = geneData[0];
            T[] geneValues = new T[Headers.Length];
            for (int j = 1; j < geneValues.Length; j++)
            {
                geneValues[j] = (T)Convert.ChangeType(geneData[j], typeof(T), CultureInfo.InvariantCulture);
            }
            Genes[i - 1] = new GeneStripped<T>(geneName, geneValues);
        }
        
        if (outputToConsole)
        {
            Console.WriteLine("Genetic data read successfully.");
        }
        consoleLog.Append("Genetic data read successfully." + NewLine);
    }
}