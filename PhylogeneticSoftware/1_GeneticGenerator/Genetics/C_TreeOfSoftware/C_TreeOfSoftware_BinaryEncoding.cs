using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.Genetics;

public class C_TreeOfSoftware_BinaryEncoding : _IEncodingGenerator<int>
{
    private readonly _IEncodingGenerator<int> singleProjectFilesIEncoding;
    public C_TreeOfSoftware_BinaryEncoding(_IEncodingGenerator<int> singleProjectFilesIEncoding) : base()
    {
        this.singleProjectFilesIEncoding = singleProjectFilesIEncoding;
    }
    
    public override int DefaultValueForGene()
    {
        throw new NotImplementedException();
    }

    protected override bool GenerateGene(in string filePath, out Gene<int> gene)
    {
        throw new NotImplementedException();
    }
}