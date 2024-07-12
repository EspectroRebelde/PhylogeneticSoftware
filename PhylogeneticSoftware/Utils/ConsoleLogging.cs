using System.Runtime.CompilerServices;
using System.Text;
using Pastel;
using StringExtensions;

namespace PhylogeneticApp.Utils;

public static class ConsoleLogging
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void PrintToConsole(string message, Color color)
    {
        Console.WriteLine(message.Pastel(color));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void PrintErrorToConsole(string message)
    {
        PrintToConsole(message, Color.Red);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void PrintWarningToConsole(string message)
    {
        PrintToConsole(message, Color.Yellow);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void PrintSuccessToConsole(string message)
    {
        PrintToConsole(message, Color.Blue);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void PrintInfoToConsole(string message)
    {
        PrintToConsole(message, Color.Green);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void PrintDebugToConsole(string message)
    {
        PrintToConsole(message, Color.Gray);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void PrintGeneToConsole<T>(DataStructures.Gene<T> gene, TypeDescriptor? headerType = null, TypeDescriptor? valueType = null)
    {
        StringBuilder geneString = new();
        geneString.Append(($"--- Gene: {gene.Name} ---" + Environment.NewLine).ToYellowANSI());
        for (int i = 0; i < gene.Headers.Length; i++)
        {
            var header =
                ValueToTypeString(gene.Headers[i], headerType ?? TypeDescriptor.String);
            var value =
                ValueToTypeString(gene.Value[i], valueType ?? TypeDescriptor.String);
            
            geneString.Append($"{header}: {value}" + Environment.NewLine);
        }
        PrintToConsole(geneString.ToString(), Color.Beige);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static string ValueToTypeString<T>(T value, TypeDescriptor type)
    {
        switch (type)
        {
            case TypeDescriptor.Int:
                return value.ToString();
            case TypeDescriptor.Byte:
                string val = Convert.ToString(Convert.ToInt32(value), 2);
                // Add padding to the binary string to make it 32 bits long with a _ every 8 bits
                return val.PadLeft(32, '0').Insert(24, "_").Insert(16, "_").Insert(8, "_");
            case TypeDescriptor.Float:
                return Convert.ToSingle(value).ToString();
            case TypeDescriptor.Double:
                return Convert.ToDouble(value).ToString();
            case TypeDescriptor.String:
                return value.ToString();
            default:
                return value.ToString();
        }
    }
    
    public enum TypeDescriptor
    {
        Int,
        Byte,
        OpcodedByte,
        Float,
        Double,
        String
    }
}