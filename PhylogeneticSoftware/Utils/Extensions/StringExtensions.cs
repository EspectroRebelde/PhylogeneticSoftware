using Pastel;

namespace StringExtensions;

public static class StringExtensions
{
    #region Format
    public static string ToBoldANSI(this string str)
    {
        return $"\u001b[1m{str}\u001b[0m";
    }
    
    public static string ToItalicANSI(this string str)
    {
        return $"\u001b[3m{str}\u001b[0m";
    }
    
    public static string ToUnderlineANSI(this string str)
    {
        return $"\u001b[4m{str}\u001b[0m";
    }
    
    public static string ToInverseANSI(this string str)
    {
        return $"\u001b[7m{str}\u001b[0m";
    }
    #endregion

    #region Colors
    
    public static string ToBlackANSI(this string str)
    {
        return str.Pastel("#000000");
    }
    
    public static string ToRedANSI(this string str)
    {
        return str.Pastel("#FF0000");
    }
    
    public static string ToGreenANSI(this string str)
    {
        return str.Pastel("#00FF00");
    }
    
    public static string ToYellowANSI(this string str)
    {
        return str.Pastel("#FFFF00");
    }
    
    public static string ToBlueANSI(this string str)
    {
        return str.Pastel("#0000FF");
    }

    public static string ToMagentaANSI(this string str)
    {
        return str.Pastel("#FF00FF");
    }
    
    #endregion
}