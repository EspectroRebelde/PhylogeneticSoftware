namespace PhylogeneticApp.DataStructures;

public struct NewickPair
{
    public int id;
    public string left;
    public string right;
    public float diff;

    public NewickPair(int id, string left, string right, float diff)
    {
        this.id = id;
        this.left = left;
        this.right = right;
        this.diff = diff;
    }
}