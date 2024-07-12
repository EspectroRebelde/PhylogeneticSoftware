using System.Drawing.Imaging;

namespace PhylogeneticApp.Utils.ColorAssignments;

public class ColorPerParentFolder : _IColorAssignment
{
    private int ParentAltitude { get; set; }
    
    public ColorPerParentFolder(int parentAltitude = 1)
    {
        ParentAltitude = parentAltitude+1;
    }
    
    public override void AssignColors(in string[] headers, in string[] filesPathProcessed, out Dictionary<string, Color> headerColors)
    {
        headerColors = new Dictionary<string, Color>();
        Dictionary<string, Color> categories = new Dictionary<string, Color>();
        Color[] palette = GetContrastingColorPalette(headers.Length);
        int colorIndex = 0;
        
        foreach (var header in headers)
        {
            string category = GetHeaderCategory(header, in filesPathProcessed);
            if (!categories.TryGetValue(category, out Color value))
            {
                value = palette[colorIndex];
                categories.Add(category, value);
                colorIndex++;
            }
            
            headerColors.TryAdd(header, value);
        }
        
        ConsoleLogging.PrintWarningToConsole("----- Categories and assigned colors -----");
        // Console log the categories and assigned colors
        foreach (var (key, value) in categories)
        {
            ConsoleLogging.PrintInfoToConsole($"Category: {key} - {value}");
        }
        ConsoleLogging.PrintWarningToConsole("-----------------------------------------");
    }
    
    private string GetHeaderCategory(string header, in string[] filesProcessed)
    {
        // Depending on the parent folder of the file, we will assign a category (name of the parent folder)
        string category = "";
        foreach (var file in filesProcessed)
        {
            if (file.Contains(header))
            {
                string[] folders = file.Split('\\');
                if (folders.Length > ParentAltitude)
                {
                    category = folders[^ParentAltitude];
                }
                break;
            }
        }
        
        return category;
    }
    
}