using System.Text;

namespace PhylogeneticApp.DataStructures;

public struct GeneStripped<T>
{
    public string Name { get; set; }
    public T[] Values { get; }
    
    public GeneStripped(string name, T[] values)
    {
        Name = name;
        Values = values;
    }
    
    public string ValueToString(int position)
    {
        return Values[position]!.ToString()!;
    }
    
    public readonly string ConcatenateValueAsString()
    {
        StringBuilder sb = new();
        for (int i = 0; i < Values.Length; i++)
        {
            T value = Values[i]!;
            sb.Append(value.ToString());
        }

        return sb.ToString();
    }
    
    public int ValueLenght()
    {
        return Values.Length;
    }
    
}