using System;
using System.Collections.Generic;
using StringExtensions;

namespace PhylogeneticApp.DataStructures;

public class TreeNode
{
    public string Id { get; }
    public string[] Categories { get; private set; }
    
    public TreeNode(string id)
    {
        Id = id;
    }
    
    public TreeNode(string id, string[] categories)
    {
        Id = id;
        Categories = categories;
    }
    
    public void SetCategories(string[] categories)
    {
        Categories = categories;
    }
}

public class TreeCreatedNode : TreeNode
{
    public List<TreeNode> Children { get; }

    public TreeCreatedNode(string id) : base(id)
    {
        Children = new List<TreeNode>();
    }

    public void AddChild(TreeNode child)
    {
        Children.Add(child);
    }
}

public class TreeLeaf : TreeNode
{
    public TreeLeaf(string id, string[] categories) : base(id, categories)
    {
    }
}

public class Tree
{
    public TreeNode Root { get; }
    public string[] Categories { get; private set; }
    public int TreeNodesCount { get; private set; }

    public Tree(TreeNode root, string[] categories, int treeNodesCount = 0)
    {
        Root = root;
        Categories = categories;
        TreeNodesCount = treeNodesCount;
    }

    public static Tree ConstructTree(Dictionary<Tuple<string, string>, string> connections)
    {
        Dictionary<string, TreeNode> nodes = new Dictionary<string, TreeNode>();
        List<string> categoriesRecorded = new List<string>();
        // Create nodes
        for (int i = 0; i < connections.Count; i++)
        {
            string id1 = connections.Keys.ElementAt(i).Item1;
            string id2 = connections.Keys.ElementAt(i).Item2;
            string value = connections.Values.ElementAt(i);
            
            // If the two ids are the same, we create a single node
            if (id1 == id2)
            {
                nodes[id1] = new TreeLeaf(id1, new string[] { value });
                if (!categoriesRecorded.Contains(value))
                {
                    categoriesRecorded.Add(value);
                }
            }
            else
            {
                TreeCreatedNode node = new TreeCreatedNode(value);
                node.AddChild(nodes[id1]);
                node.AddChild(nodes[id2]);
                // Merge the categories of the two nodes (only uniques)
                string[] categories = nodes[id1].Categories.Union(nodes[id2].Categories).ToArray();
                node.SetCategories(categories.Length == 1 ? categories : []);
                nodes[value] = node;
            }
        }
        // Set the categories of the root node to empty
        nodes.Values.Last().SetCategories([]);
        
        return new Tree(nodes.Values.Last(), categoriesRecorded.ToArray(), nodes.Count);
    }
    
    public Dictionary<string,float> GetTreeAnalysis()
    {
        Dictionary<string, float> distribution = new Dictionary<string, float>();
        Dictionary<string, int> categoriesCount = new Dictionary<string, int>();
        foreach (var category in Categories)
        {
            distribution[category] = 0;
            categoriesCount[category] = 0;
        }
        
        GetCategoriesDistribution(Root, ref distribution, ref categoriesCount);
        Utils.ConsoleLogging.PrintWarningToConsole("------ Tree Analysis ------");
        foreach (var category in Categories)
        {
            Utils.ConsoleLogging.PrintInfoToConsole($"- {category.ToInverseANSI()}: {distribution[category]} from {categoriesCount[category]} expected appearances, " +
                                                    $"a value of {distribution[category] / categoriesCount[category]} " +
                                                    $"(on tree: {Math.Min(distribution[category] / (categoriesCount[category] - 1),1)})");
        }
        Utils.ConsoleLogging.PrintWarningToConsole("---------------------------");
        return distribution;
    }
    
    /// <summary>
    /// This will traverse the tree and get the distribution of the categories
    /// For this, if a node has more than one category, it will distribute the value among the categories
    /// If a node has only one category, it will add the value to the category
    /// The distribution is done by dividing the value of the node by the number of categories it has
    /// For the result, the total value per category will be the calculated value divided by the appearances of the category
    /// The value of the node is 1, if we have 2 categories, each category will have 0.5
    /// </summary>
    /// <param name="node"></param>
    /// <param name="distribution"></param>
    private void GetCategoriesDistribution(TreeNode node, ref Dictionary<string, float> distribution, ref Dictionary<string, int> categoriesCount)
    {
        if (node is TreeCreatedNode createdNode)
        {
            foreach (var child in createdNode.Children)
            {
                GetCategoriesDistribution(child, ref distribution, ref categoriesCount);
            }
            foreach (var category in createdNode.Categories)
            {
                distribution[category] += 1;
                categoriesCount[category] += 1;
            }
            foreach (var child in createdNode.Children)
            {
                // If it's a leaf, and it's category isn't in the parent, we add to the counter
                if (child is TreeLeaf leaf)
                {
                    foreach (var category in leaf.Categories)
                    {
                        if (!createdNode.Categories.Contains(category))
                        {
                            categoriesCount[category] += 1;
                        }
                    }
                }
            }
        }
    }
}

