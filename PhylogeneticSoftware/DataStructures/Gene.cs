//#define MANUAL_CONVERSION

using System.Text;

namespace PhylogeneticApp.DataStructures;

/// <summary>
/// This struct represents the encoding of a single gene (single row) of a Gen2 implementation
/// </summary>
/// <typeparam name="T"></typeparam>
public struct Gene<T>
{
    public string Name { get; set; }
    public string[] Headers;
    public T[] Value { get; set; }

    public void Deconstruct(out string[] headers, out string name, out T[] value)
    {
        headers = Headers;
        name = Name;
        value = Value;
    }
    public Gene(string name, string[] headers, T[] value)
    {
        Name = name;
        Value = value;
        Headers = headers;
    }
    
    public Gene()
    {
        Name = string.Empty;
        Value = Array.Empty<T>();
        Headers = Array.Empty<string>();
    }
    
    public Gene(in int HeaderLength)
    {
        Name = string.Empty;
        Value = new T[HeaderLength];
        Headers = new string[HeaderLength];
    }
    
    public Gene(Dictionary<string, T> gene)
    {
        Name = string.Empty;
        Headers = gene.Keys.ToArray();
        Value = gene.Values.ToArray();
    }

    public Gene(Gene<T> gene)
    {
        Name = gene.Name;
        Value = gene.Value;
        Headers = gene.Headers;
    }
    
    public void AddGene(string header, T value)
    {
        // Copy the current headers and values to a new array with one more element
        Headers = Headers.Concat(new string[] {header}).ToArray();
        Value = Value.Concat(new T[] {value}).ToArray();
    }

    public string ValueToString(int position)
    {
        return Value[position]!.ToString()!;
    }
    
    public int HeadersLength()
    {
        return Headers.Length;
    }
    
    public int ValueLenght()
    {
        return Value.Length;
    }
    
    public static implicit operator GeneStripped<T>(Gene<T> gene)
    {
        return new GeneStripped<T>(gene.Name, gene.Value);
    }
    
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("--- Gene: " + Name + " ---");
        sb.Append("\n");
        // Structure should be a table with the pair header-value in each row
        for (int i = 0; i < Headers.Length; i++)
        {
            sb.Append(Headers[i] + " : " + Value[i]);
            sb.Append("\n");
        }
        return sb.ToString();
    }
}