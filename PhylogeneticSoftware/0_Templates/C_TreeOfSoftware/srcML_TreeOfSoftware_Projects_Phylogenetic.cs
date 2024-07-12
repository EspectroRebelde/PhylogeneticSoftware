using PhylogeneticApp.Implementations.Genetics;
using PhylogeneticApp.Implementations.OutputGenerator;
using PhylogeneticApp.Implementations.Ownership;
using PhylogeneticApp.Implementations.TreeCreator;
using PhylogeneticApp.Templates;
using PhylogeneticApp.Utils.ColorAssignments;

namespace PhylogeneticApp.Implementations.Phylogenetic;

public class srcML_TreeOfSoftware_Projects_Phylogenetic : _PhylogeneticTemplate<float>
{
    public srcML_TreeOfSoftware_Projects_Phylogenetic(string[] foldersToProcess, string outputFolder) : base(foldersToProcess, outputFolder)
    {
    }
    
    protected override bool SearchRecursively()
    {
        return false;
    }

    protected override string GetName()
    {
        return "srcML Tree Of Software Projects Phylogenetic";
    }

    protected override string[] GetFileTypes()
    {
        return new string[] { ".directory" };
    }

    protected override _IEncodingGenerator<float> SetEncoderGenerator()
    {
        var singleFileEncoding = new scrML_NA_Kmer_IntegerEncoding();
        return new srcML_TreeOfSoftware_ProjectsEncoding(singleFileEncoding);
    }

    protected override _IOwnershipGenerator<float> SetOwnershipGenerator()
    {
        return new BasicOwnershipFloat();
    }

    protected override _IDistance<float> SetDistanceAlgorithm()
    {
        return new Implementations.CorrelationMatrixDistance.NotAligned.EuclideanDistance();
    }

    protected override _IPairing SetPairingAlgorithm()
    {
        return new Implementations.Pairing.MeanPairing();
    }

    protected override _IOutputGenerator SetOutputAlgorithm()
    {
        /*/
        var output = new DOTOutput();
        output.SetColorAssignment(new ColorPerParentFolder(1));
        /*/
        var output = new NewickOutput(); 
        //*/
        return output;
    }
    
    protected override _ITreeCreator SetTreeGenerator()
    {
        /*/
        return new GraphvizTreeCreator();
        /*/
        return new NullCreator();
        //*/
    }

    protected override void FilterOutPaths(ref string[] paths)
    {
        // Filter out paths which filename that start with __
        paths = paths.Where(path => !Path.GetFileName(path).StartsWith("__")).ToArray();
    }
    
    protected override OptionsDefinitions GetOptions()
    {
        OptionsDefinitions options = new OptionsDefinitions();
        
        options.ProcessInParallel = true;
        options.GeneticOptions = new OptionsDefinitions.SGeneticalOptions();
        options.CreateOwnershipFile = false;
        options.GeneticOptions.GroupOperation = new OptionsDefinitions.SGeneticalOptions.SGroupOperations
        {
            IsGroupOperation = false,
            OutputGeneHeaderData = true,
            OutputUniqueGeneHeaderData = true,
            GroupOperationFunctions =
            [
                OptionsDefinitions.SGeneticalOptions.SGroupOperations.GroupOperationType.Segregation
            ],
            SegregationThreshold = 1
        };
        
        options.OutputOptions = new OptionsDefinitions.SOutputOptions();
        options.OutputOptions.OutputAlgorithmicEvaluation = true;
        
        return options;
    }
}