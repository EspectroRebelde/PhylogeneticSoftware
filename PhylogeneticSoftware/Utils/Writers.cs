using System.Globalization;

namespace PhylogeneticApp.Utils;

public class Writers
{
    public static void WriteGenesToFile<T>(in DataStructures.GeneticEncoding<T> geneticEncoding, in string pathToOutputFolder,
        string filename = "Genetics.csv")
    {
        geneticEncoding.WriteGenes(pathToOutputFolder, filename);
    }
    
    public static void WriteNAGenesToFile<T>(in DataStructures.GeneticEncoding<T> geneticEncoding, in string pathToOutputFolder,
        string filename = "Genetics.csv")
    {
        geneticEncoding.WriteNAGenes(pathToOutputFolder, filename);
    }
    
    public static void WriteMatrixToFile(in string[] headers, in float[] matrix, in string pathToOutputFolder,
        in string filename = "CorrelationMatrix.csv")
    {
        using (StreamWriter writer = new StreamWriter(pathToOutputFolder + "\\" + filename))
        {
            const string separator = ";";
            // Structure is:
            // spacer, header1, header2, header3, ...
            // header1, value, value, value, ...
            // header2, value, value, value, ...
            
            // Write the headers
            for (int i = 0; i < headers.Length; i++)
            {
                writer.Write("{0}{1}", separator, headers[i]);
            }
            
            // Write the matrix
            for (int i = 0; i < headers.Length; i++)
            {
                writer.Write("\n{0}", headers[i]);
                for (int j = 0; j < headers.Length; j++)
                {
                    writer.Write("{0}{1}", separator, matrix[i * headers.Length + j].ToString(CultureInfo.InvariantCulture));
                }
            }
            
            Console.WriteLine("Matrix written to file.");
        }
    }
    
    public static void WritePairsToFile(in DataStructures.GeneticStructures.OutputPair[] pairs, in string[] headers, in string pathToOutputFolder,
        in string filename = "OutputPairs.csv")
    {
        const char separator = ';';    
        const string newline = "\n";
        
        using (StreamWriter writer = new StreamWriter(pathToOutputFolder + "\\" + filename))
        {
            // Write the headers
            foreach (var header in headers)
            {
                writer.Write(separator + header);
            }
            
            // Write the pairs
            foreach (var pair in pairs)
            {
                writer.Write(newline + pair.First + separator + pair.Second + separator + pair.Value.ToString(CultureInfo.InvariantCulture));
            }
            
            Console.WriteLine("Pairs written to file.");
        }
    }
}