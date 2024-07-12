namespace PhylogeneticApp.Utils;

public class ColorHelper
{
    private static int PerceivedBrightness(Color c)
    {
        return (int)Math.Sqrt(
            c.R * c.R * .241 +
            c.G * c.G * .691 +
            c.B * c.B * .068);
    }
    
    public static Color BlackOrWhiteFromPerceivedBrightness(Color c)
    {
        return PerceivedBrightness(c) > 130 ? Color.Black : Color.White;
    }
}