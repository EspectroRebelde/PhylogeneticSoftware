namespace PhylogeneticApp.Utils.ColorAssignments;

public class ColorPerPrefix : _IColorAssignment
{
    private readonly string[] _prefixes;
    
    public ColorPerPrefix(in string[] prefixes)
    {
        _prefixes = prefixes;
    }
    
    public override void AssignColors(in string[] headers, in string[] filesPathProcessed, out Dictionary<string, Color> headerColors)
    {
        headerColors = new Dictionary<string, Color>();
        Dictionary<string, Color> categories = new Dictionary<string, Color>();
        categories.Add("Default", ColorTranslator.FromHtml(defaultColor));
        Color[] palette = GetContrastingColorPalette(-1);
        int colorIndex = 0;
        
        foreach (var header in headers)
        {
            string category = GetHeaderCategory(header);
            if (!categories.TryGetValue(category, out Color value))
            {
                value = palette[colorIndex];
                categories.Add(category, value);
                colorIndex++;
            }
            
            headerColors.TryAdd(header, value);
        }
    }
    
    private string GetHeaderCategory(string header)
    {
        // Depending on the prefix of the header, we will assign a category
        string category = "";
        foreach (var prefix in _prefixes)
        {
            if (header.StartsWith(prefix))
            {
                category = prefix;
                break;
            }
        }
        
        // If the header doesn't start with any of the prefixes, we will assign a default color
        if (category == "")
        {
            category = "Default";
        }
        
        return category;
    }
}