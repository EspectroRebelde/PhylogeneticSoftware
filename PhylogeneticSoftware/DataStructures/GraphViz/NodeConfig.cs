namespace PhylogeneticApp.DataStructures.GraphViz;

/**
 * Class that contains the configuration for a GraphViz node
 * - shape: The shape of the node
 * - style: The style of the node
 * - color: The color of the node
 * - fontColor: The color of the font
 * - width: The width of the node
 * - height: The height of the node
 */
public struct NodeConfig
{
    // Shape of the node
    public string Shape = "plaintext";
    public string Style = "rounded";
    public string Color = "#87CEEB";
    public string FontColor = "#000000";
    public Dictionary<string, object> Options = null;
    float width = .3f;
    float height = .3f;

    public NodeConfig(string shape, string style, string color, string fontColor, float width, float height, Dictionary<string, object> options = null)
    {
        this.Shape = shape;
        this.Style = style;
        this.Color = color;
        this.FontColor = fontColor;
        this.width = width;
        this.height = height;
        this.Options = options ?? new Dictionary<string, object>();
    }
}
