using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Utils;

/*
 var test = new Utils.TemplateBridge<int>(
            foldersToProcess: new[] { "" },
            outputFolder: "",
            fileTypes: new[] { "" },
            encoderGenerator: new Implementations.Genetics.CAFSingleProjectFilesIEncoding(),
            ownershipGenerator: new Implementations.Ownership.BasicOwnership(),
            iDistanceAlgorithm: new Implementations.CorrelationMatrixDistance.DamerauLevenshteinDistance(),
            pairingAlgorithm: new Implementations.Pairing.MeanPairing(),
            outputDelegate: new Implementations.OutputGenerator.DOTOutput(),
            treeGenerator: new Implementations.TreeCreator.GraphvizITreeCreator()
        );
 */
public class TemplateBridge<T>
{
    private readonly _PhylogeneticTemplate<T> _phylogeneticTemplate;
    
    // Set the interfaces required from the parameters
    public TemplateBridge(
        string[] foldersToProcess,
        string outputFolder,
        string[] fileTypes,
        _IEncodingGenerator<T> encoderGenerator, 
        _IOwnershipGenerator<T> ownershipGenerator, 
        _IDistance<T> iDistanceAlgorithm,
        _IPairing? pairingAlgorithm = null,
        _IOutputGenerator? outputDelegate = null,
        _ITreeCreator? treeGenerator = null)
    {
        _phylogeneticTemplate = new TemplateBridgeClass(
            foldersToProcess, 
            outputFolder, 
            fileTypes, 
            encoderGenerator, 
            ownershipGenerator, 
            iDistanceAlgorithm, 
            pairingAlgorithm ?? new Implementations.Pairing.MeanPairing(), 
            outputDelegate ?? new Implementations.OutputGenerator.DOTOutput(),
            treeGenerator ?? new Implementations.TreeCreator.GraphvizTreeCreator());
    }

    public void Run(PhylogeneticSteps stepToStartFrom = PhylogeneticSteps.Genetics)
    {
        _phylogeneticTemplate.Run(stepToStartFrom);
    }

    public void RunWithTimer(PhylogeneticSteps stepToStartFrom = PhylogeneticSteps.Genetics)
    {
        _phylogeneticTemplate.RunWithTimer(stepToStartFrom);
    }

    public void RunWithNotifications(PhylogeneticSteps stepToStartFrom = PhylogeneticSteps.Genetics)
    {
        _phylogeneticTemplate.RunWithNotifications(stepToStartFrom);
    }

    public void RunUniqueProcess(PhylogeneticSteps stepToRun = PhylogeneticSteps.Genetics,
        bool notifyUser = false)
    {
        _phylogeneticTemplate.UniqueProcess(stepToRun, notifyUser);
    }

    public void ChangeFileNaming(PhylogeneticPaths newPaths)
    {
        _phylogeneticTemplate.ChangeFileNaming(newPaths);
    }

    class TemplateBridgeClass : _PhylogeneticTemplate<T>
    {
        public string[] fileTypes;
        public TemplateBridgeClass(string[] foldersToProcess, string outputFolder, string[] fileTypes,
            _IEncodingGenerator<T> encoderGenerator, _IOwnershipGenerator<T> ownershipGenerator,
            _IDistance<T> iDistanceAlgorithm, _IPairing? pairingAlgorithm = null,
            _IOutputGenerator? outputDelegate = null, _ITreeCreator? treeGenerator = null) : base(foldersToProcess, outputFolder)
        {
            this.fileTypes = fileTypes;
            
            EncoderGenerator = encoderGenerator;
            OwnershipGenerator = ownershipGenerator;
            IDistanceAlgorithm = iDistanceAlgorithm;
            PairingAlgorithm = pairingAlgorithm ?? new Implementations.Pairing.MeanPairing();
            OutputDelegate = outputDelegate ?? new Implementations.OutputGenerator.DOTOutput();
            TreeGenerator = treeGenerator ?? new Implementations.TreeCreator.GraphvizTreeCreator();
        }

        protected override string GetName()
        {
            return "Tester";
        }

        protected override string[] GetFileTypes()
        {
            return fileTypes;
        }

        protected override _IEncodingGenerator<T> SetEncoderGenerator()
        {
            return EncoderGenerator;
        }

        protected override _IOwnershipGenerator<T> SetOwnershipGenerator()
        {
            return OwnershipGenerator;
        }

        protected override _IDistance<T> SetDistanceAlgorithm()
        {
            return IDistanceAlgorithm;
        }
    }
}