namespace PhylogeneticApp.DataStructures.GraphViz;

public class Visualization
{
    public bool legend = true;
    public bool useColorLabels = true;

    public NodeConfig baseNodeConfig = new NodeConfig("plaintext", "filled", "#87CEEB", "#000000", .3f, .3f);
    public NodeConfig linkNodeConfig = new NodeConfig("diamond", "filled", "#006400", "#006400", .3f, .3f);
    public NodeConfig finalNodeConfig = new NodeConfig("diamond", "filled", "#8B0000", "#8B0000", .3f, .3f);

    public Visualization(bool legend, bool useColorLabels)
    {
        this.legend = legend;
        this.useColorLabels = useColorLabels;
    }

    // Change the baseNodeConfig
    public void setBaseNodeConfig(NodeConfig config)
    {
        baseNodeConfig = config;
    }

    // Change the linkNodeConfig
    public void setLinkNodeConfig(NodeConfig config)
    {
        linkNodeConfig = config;
    }

    // Change the finalNodeConfig
    public void setFinalNodeConfig(NodeConfig config)
    {
        finalNodeConfig = config;
    }

    public string LegendBuilder(TreeTypes type, Structures gf, params Tuple<string, string>[] elements)
    {
        if (legend)
        {
            switch (type)
            {
                case TreeTypes.Projects:
                    return gf.Legend(
                        margin1: "2.5",
                        margin2: "1.25",
                        fontSize: 14,
                        border: 0,
                        cellBorder: 1,
                        cellSpacing: 0,
                        cellPadding: 4,
                        label: "Legend",
                        elements: elements
                    );
                case TreeTypes.ProjectsWithQ40:
                    return gf.Legend(
                        margin1: "3",
                        margin2: "0.5",
                        fontSize: 14,
                        border: 0,
                        cellBorder: 1,
                        cellSpacing: 0,
                        cellPadding: 4,
                        label: "Legend",
                        elements: elements
                    );
                case TreeTypes.AllProjects:
                    return gf.Legend(
                        margin1: "0",
                        margin2: "0",
                        fontSize: 14,
                        border: 0,
                        cellBorder: 1,
                        cellSpacing: 0,
                        cellPadding: 4,
                        label: "Legend",
                        elements: elements
                    );
                case TreeTypes.defaultTree:
                default:
                    return gf.Legend(
                        margin1: "0",
                        margin2: "0",
                        fontSize: 12,
                        border: 0,
                        cellBorder: 1,
                        cellSpacing: 0,
                        cellPadding: 4,
                        label: "Legend",
                        elements: elements
                    );

            }
        }

        return "";
    }
}



