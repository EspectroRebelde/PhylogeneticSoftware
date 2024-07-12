using System.Drawing.Imaging;

namespace PhylogeneticApp.Utils.ColorAssignments;

public abstract class _IColorAssignment
{
    public string defaultColor = "#87CEEB";
    
    public abstract void AssignColors(
        in string[] headers,
        in string[] filesPathProcessed,
        out Dictionary<string, Color> headerColors);
    
    public virtual int MaximunPaletteSize => new Bitmap(1, 1, PixelFormat.Format8bppIndexed).Palette.Entries.Length;
    
    /// <summary>
    /// Get a ColorPalette object with 0 - 30 colors
    /// </summary>
    /// <returns type="Color[]"> A Color[] array with 0 - 30 colors</returns>
    /// <remarks> The maximun numbers of colors is 0 - 255 as it's the limit of the 8bppIndexed format</remarks>
    public virtual Color[] GetMainColorPalette(int lengthToReturn = 31)
    {
        if (lengthToReturn > MaximunPaletteSize)
        {
            ConsoleLogging.PrintErrorToConsole("The maximum number of colors for a palette is 256");
        }
        
        // Only used to get a ColorPalette object (handles more entries if needed)
        var imagePalette = new Bitmap(1, 1, PixelFormat.Format8bppIndexed).Palette;

        #region ColorAdditions

        #region Blue

        imagePalette.Entries[0] = Color.Teal;
        imagePalette.Entries[1] = Color.DodgerBlue;
        imagePalette.Entries[2] = Color.CornflowerBlue;
        
        #region Cyan

        imagePalette.Entries[3] = Color.Aquamarine;
        imagePalette.Entries[4] = Color.Aqua;
        imagePalette.Entries[5] = Color.CadetBlue;

        #endregion
        
        #endregion
        
        #region Green

        imagePalette.Entries[6] = Color.SpringGreen;
        imagePalette.Entries[7] = Color.SeaGreen;
        imagePalette.Entries[8] = Color.Olive;
        imagePalette.Entries[9] = Color.ForestGreen;
        imagePalette.Entries[10] = Color.Lime;
        
        #endregion
        
        #region Purple

        imagePalette.Entries[11] = Color.Lavender;
        imagePalette.Entries[12] = Color.Violet;
        imagePalette.Entries[13] = Color.DarkOrchid;
        imagePalette.Entries[14] = Color.Indigo;
        
        #region Pink and Magenta 

        imagePalette.Entries[15] = Color.HotPink;
        imagePalette.Entries[16] = Color.MediumVioletRed;
        imagePalette.Entries[17] = Color.Orchid;
        
        #endregion

        #endregion
        
        #region Red

        imagePalette.Entries[18] = Color.Coral;
        imagePalette.Entries[29] = Color.Tomato;
        imagePalette.Entries[20] = Color.Red;
        imagePalette.Entries[21] = Color.DarkRed;

        #endregion
        
        #region Yellow and Orange

        imagePalette.Entries[22] = Color.Yellow;
        imagePalette.Entries[23] = Color.GreenYellow;
        imagePalette.Entries[24] = Color.Gold;
        imagePalette.Entries[25] = Color.Orange;
        
        #region Brown

        imagePalette.Entries[26] = Color.Bisque;
        imagePalette.Entries[27] = Color.Peru;
        imagePalette.Entries[28] = Color.Chocolate;
        
        #endregion

        #endregion
        
        #region Gray

        imagePalette.Entries[29] = Color.Gray;
        imagePalette.Entries[30] = Color.DarkSlateGray;

#endregion

        #endregion

        if (lengthToReturn == -1 || lengthToReturn > MaximunPaletteSize)
        {
            return imagePalette.Entries;
        }
        else
        {
            // Trim the palette to the length requested
            var trimmedPalette = new Color[lengthToReturn];
            Array.Copy(imagePalette.Entries, trimmedPalette, lengthToReturn);
        
            return trimmedPalette;
        }
    }
    
    public virtual int MainColorCount => 31;

    /// <summary>
    /// Get a ColorPalette object with 0 - 8 colors that highly contrast within each other
    /// </summary>
    /// <returns type="Color[]"> A Color[] array with 0 - 7 colors</returns>
    /// <remarks> The maximun numbers of colors is 0 - 255 as it's the limit of the 8bppIndexed format</remarks>
    public virtual Color[] GetContrastingColorPalette(int lengthToReturn = 31)
    {
        if (lengthToReturn > MaximunPaletteSize)
        {
            ConsoleLogging.PrintErrorToConsole("The maximum number of colors for a palette is 256");
        }
        
        
        // Only used to get a ColorPalette object (handles more entries if needed)
        var imagePalette = new Bitmap(1, 1, PixelFormat.Format8bppIndexed).Palette;

        // We get the most contrasting colors from the main palette
        imagePalette.Entries[0] = Color.Cyan;
        imagePalette.Entries[1] = Color.Red;
        imagePalette.Entries[2] = Color.Lime;
        imagePalette.Entries[3] = Color.Blue;
        imagePalette.Entries[4] = Color.Yellow;
        imagePalette.Entries[5] = Color.Magenta;
        imagePalette.Entries[6] = Color.Teal;
        imagePalette.Entries[7] = Color.Orange;
        
        if (lengthToReturn == -1 || lengthToReturn > MaximunPaletteSize)
        {
            return imagePalette.Entries;
        }
        else
        {
            // Trim the palette to the length requested
            var trimmedPalette = new Color[lengthToReturn];
            Array.Copy(imagePalette.Entries, trimmedPalette, lengthToReturn);
        
            return trimmedPalette;
        }
    }
    
    public virtual int ContrastingColorCount => 8;
}