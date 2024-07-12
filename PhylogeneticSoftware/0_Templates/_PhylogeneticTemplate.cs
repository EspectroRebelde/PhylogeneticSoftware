#define PARALLEL_EXECUTION

using System.Diagnostics;
using System.Globalization;
using Pastel;
using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Implementations.OutputGenerator;
using PhylogeneticApp.Implementations.Pairing;
using PhylogeneticApp.Implementations.TreeCreator;
using PhylogeneticApp.Utils;

namespace PhylogeneticApp.Templates;

public abstract class _PhylogeneticTemplate<T>
{
    #region Given Parameters

    protected string[] FoldersToProcess;
    protected string OutputFolder;

    #endregion

    #region Predefined Defaults

    protected GeneticEncoding<T> GeneticEncoding;
    protected Gene<T>[] Genes;
    protected float[] CorrelationMatrix;
    protected GeneticStructures.OutputPair[] Pairs;
    protected string pathToOutput = string.Empty;
    protected string pathToTree = string.Empty;
    protected string[] GeneHeaders = Array.Empty<string>();
    
    #endregion

    #region Set By User

    protected _IEncodingGenerator<T> EncoderGenerator;
    protected _IOwnershipGenerator<T> OwnershipGenerator;
    protected _IDistance<T> IDistanceAlgorithm;
    
    #endregion

    #region Override by User

    protected _IPairing? PairingAlgorithm;
    protected _IOutputGenerator? OutputDelegate;
    protected _ITreeCreator? TreeGenerator;

    #endregion

    protected _PhylogeneticTemplate(string[] foldersToProcess, string outputFolder)
    {
        this.FoldersToProcess = foldersToProcess;
        this.OutputFolder = outputFolder;
        
        this.GeneticEncoding = new GeneticEncoding<T>();
        //TODO: Maybe set size of CorrelationMatrix and Pairs
        this.CorrelationMatrix = Array.Empty<float>();
        this.Pairs = Array.Empty<GeneticStructures.OutputPair>();
    }

    #region Run

    private void InitialSetUp()
    {
        Options.Instance.Value = GetOptions();
        Options.Instance.Value.OutputPath = OutputFolder;
        Options.Instance.DumpToFileJson();

        EncoderGenerator = SetEncoderGenerator();
        IDistanceAlgorithm = SetDistanceAlgorithm();

        if (EncoderGenerator.IsNonAlignedEncoding != IDistanceAlgorithm.IsNonAlignedEncoding)
        {
            Utils.ConsoleLogging.PrintErrorToConsole("The encoding and the distance algorithm must be both aligned or non-aligned.");
        }
        
        OwnershipGenerator = SetOwnershipGenerator();
        PairingAlgorithm = SetPairingAlgorithm();
        OutputDelegate = SetOutputAlgorithm();
        TreeGenerator = SetTreeGenerator();
    }
    
    /// <summary>
    /// Processes a segment of the phylogenetic tree.
    /// The files are expected at the output folder + PhylogeneticPaths.X where X is the step of the process.
    /// </summary>
    /// <param name="stepToStartFrom"></param>
    /// <param name="notifyUser"></param>
    public void UniqueProcess(PhylogeneticSteps stepToStartFrom = PhylogeneticSteps.Genetics, bool notifyUser = false)
    {
        InitialSetUp();
        switch (stepToStartFrom)
        {
            case PhylogeneticSteps.Genetics:
                ProcessGenetics(false);
                if (notifyUser) NotifyProgressGenetics(); 
                break;
            case PhylogeneticSteps.CorrelationMatrix:
                // Parse from file to GeneticEncoding (GeneticEncoding)
                ReadGenesFromFile(in OutputFolder, in Options.Instance.Value.FileNaming.Genetics);
                ProcessCorrelationMatrix(false, Options.Instance.Value.ProcessInParallel);
                if (notifyUser) NotifyProgressCorrelationMatrix();
                break;
            case PhylogeneticSteps.Pairs:
                // Parse from file to Matrix (Headers, Matrix)
                ReadMatrixFromFile(in OutputFolder, in Options.Instance.Value.FileNaming.CorrelationMatrix);
                ProcessPairs(false);
                if (notifyUser) NotifyProgressPairs();
                break;
            case PhylogeneticSteps.Output:
                // Parse from file to Pairs (Pairs, Headers)
                ReadPairsFromFile(OutputFolder, Options.Instance.Value.FileNaming.Pairs);
                ProcessOutput(false);
                if (notifyUser) NotifyProgressOutput();
                break;
            case PhylogeneticSteps.Tree:
                // Parse from file to Output (OutputPath)
                ProcessTree(true); 
                if (notifyUser) NotifyProgressTree();
                break;
        }
    }

    public void Run(PhylogeneticSteps stepToStartFrom = PhylogeneticSteps.Genetics)
    {
        InitialSetUp();
        
        ProcessGenetics(stepToStartFrom > PhylogeneticSteps.Genetics);
        ProcessCorrelationMatrix(stepToStartFrom > PhylogeneticSteps.CorrelationMatrix,
            Options.Instance.Value.ProcessInParallel);
        ProcessPairs(stepToStartFrom > PhylogeneticSteps.Pairs);
        ProcessOutput(stepToStartFrom > PhylogeneticSteps.Output);
        ProcessTree(stepToStartFrom > PhylogeneticSteps.Tree);
    }
    
    public void RunWithTimer(PhylogeneticSteps stepToStartFrom = PhylogeneticSteps.Genetics, bool resetInEachStep = false)
    {
        InitialSetUp();

        ConsoleExtensions.Enable();
        
        Stopwatch mainStopwatch = new Stopwatch();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        mainStopwatch.Start();
        ProcessGenetics(stepToStartFrom > PhylogeneticSteps.Genetics);
                Utils.General.PrintStopwatchData(stopwatch, "Genetics & Ownership", resetInEachStep);
                ProcessCorrelationMatrix(stepToStartFrom > PhylogeneticSteps.CorrelationMatrix,
                    Options.Instance.Value.ProcessInParallel);
                Utils.General.PrintStopwatchData(stopwatch, "CorrelationMatrix", resetInEachStep);
        ProcessPairs(stepToStartFrom > PhylogeneticSteps.Pairs);
                Utils.General.PrintStopwatchData(stopwatch, "Pairs", resetInEachStep);
        ProcessOutput(stepToStartFrom > PhylogeneticSteps.Output);
                Utils.General.PrintStopwatchData(stopwatch, "Output", resetInEachStep);
        ProcessTree(stepToStartFrom > PhylogeneticSteps.Tree);
                Utils.General.PrintStopwatchData(stopwatch, "Tree", false, true);
                Utils.General.PrintStopwatchData(mainStopwatch, "Total", false, true);
        
        ConsoleExtensions.Disable();
    }
    public void RunWithNotifications(PhylogeneticSteps stepToStartFrom = PhylogeneticSteps.Genetics)
    {
        InitialSetUp();

        ProcessGenetics(stepToStartFrom > PhylogeneticSteps.Genetics);
                NotifyProgressGenetics();
                ProcessCorrelationMatrix(stepToStartFrom > PhylogeneticSteps.CorrelationMatrix,
                    Options.Instance.Value.ProcessInParallel);
                NotifyProgressCorrelationMatrix();
        ProcessPairs(stepToStartFrom > PhylogeneticSteps.Pairs);
                NotifyProgressPairs();
        ProcessOutput(stepToStartFrom > PhylogeneticSteps.Output);
                NotifyProgressOutput();
        ProcessTree(stepToStartFrom > PhylogeneticSteps.Tree);
                NotifyProgressTree();
    }

    #region Segmentations

    private void ProcessGenetics(bool isPreprocessed = false)
    {
        if (isPreprocessed)
        {
            ReadGenesFromFile(OutputFolder, Options.Instance.Value.FileNaming.Genetics);
            return;
        }

        string[] files = RetrieveFiles();
        
        EncoderGenerator.foldersInProcess = FoldersToProcess;
        EncoderGenerator.GenerateEncoding(files, ref Genes, Options.Instance.Value.ProcessInParallel);
        if (!EncoderGenerator.IsNonAlignedEncoding)
        {
            T defaultValue = EncoderGenerator.DefaultValueForGene()!;
            TransformGenesToGeneticEncoding(in Genes, in defaultValue, ref GeneticEncoding);
        }
        else
        {
            TransformGenesToNAGeneticEncoding(in Genes, ref GeneticEncoding);
        }
        SetGeneticHeaders(in GeneticEncoding);
        
        string geneNames = string.Join(";", GeneHeaders);

        if (!EncoderGenerator.IsNonAlignedEncoding)
        {
            Utils.Writers.WriteGenesToFile(in GeneticEncoding, in OutputFolder, Options.Instance.Value.FileNaming.Genetics);
        }
        else
        {
            Utils.Writers.WriteNAGenesToFile(in GeneticEncoding, in OutputFolder, Options.Instance.Value.FileNaming.Genetics);
        }
        if (Options.Instance.Value.CreateOwnershipFile)
        {
            OwnershipGenerator.GenerateOwnership(in OutputFolder, in Genes, Options.Instance.Value.FileNaming.Ownership);
        }
    }

    private void ProcessCorrelationMatrix(bool isPreprocessed = false, bool executeInParallel = true)
    {
        if (isPreprocessed)
        {
            ReadMatrixFromFile(OutputFolder, Options.Instance.Value.FileNaming.CorrelationMatrix);
            return;
        }
        
        SetGeneticHeaders(in GeneticEncoding);
        if (executeInParallel)
        {
            IDistanceAlgorithm.ParallelConvertToCorrelationMatrix(in GeneticEncoding, out CorrelationMatrix);
        }
        else {
            IDistanceAlgorithm.ConvertToCorrelationMatrix(in GeneticEncoding, out CorrelationMatrix);
        }
        
        Utils.Writers.WriteMatrixToFile(in GeneHeaders, in CorrelationMatrix, in OutputFolder, Options.Instance.Value.FileNaming.CorrelationMatrix);
    }

    private void ProcessPairs(bool isPreprocessed = false)
    {
        if (isPreprocessed)
        {
            ReadPairsFromFile(OutputFolder, Options.Instance.Value.FileNaming.Pairs);
            return;
        }
        
        SetGeneticHeaders(in GeneticEncoding);
        PairingAlgorithm.GeneratePairs(in GeneHeaders, in CorrelationMatrix, ref Pairs);
        Utils.Writers.WritePairsToFile(in Pairs, in GeneHeaders, in OutputFolder, Options.Instance.Value.FileNaming.Pairs);
    }

    private void ProcessOutput(bool isPreprocessed = false)
    {
        if (isPreprocessed)
        {
            return;
        }
        
        string[] files = RetrieveFiles();
        OutputDelegate.GenerateOutput(in Pairs, in GeneHeaders, in OutputFolder, files, out pathToOutput, Options.Instance.Value.FileNaming.Output);
    }

    private void ProcessTree(bool isPreprocessed = false)
    {
        if (isPreprocessed)
        {
            pathToOutput = OutputFolder + "\\" + Options.Instance.Value.FileNaming.Output;
        }
        
        TreeGenerator.GenerateTree(in pathToOutput, in OutputFolder, out pathToTree, Options.Instance.Value.FileNaming.Tree);
    }

    #endregion

    #region Run Methods

    private void TransformGenesToGeneticEncoding(in Gene<T>[] genes, in T defaultValue, ref GeneticEncoding<T> geneticEncoding)
    {
        geneticEncoding.TransferGenes(in genes, in defaultValue);
    }
    
    private void TransformGenesToNAGeneticEncoding(in Gene<T>[] genes, ref GeneticEncoding<T> geneticEncoding)
    {
        geneticEncoding.TransferNAGenes(in genes);
    }
    
    private void SetGeneticHeaders(in GeneticEncoding<T> geneticEncoding)
    {
        GeneHeaders = new string[geneticEncoding.Genes.Length];
        for (var i = 0; i < geneticEncoding.Genes.Length; i++)
        {
            GeneHeaders[i] = geneticEncoding.Genes[i].Name;
        }
    }

    #endregion

    #endregion

    #region Abstract methods

    protected abstract string GetName();
    
    /// <summary>
    /// File types are formatted as ".extension" (e.g. <i>".txt"</i>, <i>".csv"</i>, <i>".fasta"</i>).
    /// </summary>
    /// <returns> An array of file types to retrieve from the folders. </returns>
    /// <remarks>In case of wanting to retrieve folders, use <i>".directory"</i> or <i>".folder"</i>.</remarks>
    protected abstract string[] GetFileTypes();
    protected abstract _IEncodingGenerator<T> SetEncoderGenerator();
    protected abstract _IOwnershipGenerator<T> SetOwnershipGenerator();
    protected abstract _IDistance<T> SetDistanceAlgorithm();
    
    #endregion

    #region Overridable
    
    protected virtual bool SearchRecursively()
    {
        return true;
    }
    
    protected virtual _IPairing SetPairingAlgorithm()
    {
        return new MeanPairing();
    }
    
    protected virtual _IOutputGenerator SetOutputAlgorithm()
    {
        return new DOTOutput();
    }
    
    protected virtual _ITreeCreator SetTreeGenerator()
    {
        return new GraphvizTreeCreator();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <remarks>OptionsDefinitions has a Deserialize method to convert the JSON string to the object</remarks>
    protected virtual OptionsDefinitions GetOptions()
    {
        return new OptionsDefinitions();
    }

    #endregion
    
    #region DataToFile

    // Moved to Utils.Writers

    #endregion

    #region FileToData

    private void ReadGenesFromFile(in string pathToOutputFolder, in string filename = "Genetics.csv")
    {
        GeneticEncoding.ReadGenes(pathToOutputFolder + "\\" + filename);
    }

    private void ReadMatrixFromFile(in string pathToOutputFolder, in string filename = "CorrelationMatrix.csv")
    {
        string[] readLines = File.ReadAllLines(pathToOutputFolder + "\\" + filename);

        // Extract headers
        GeneHeaders = readLines[0].Split(';').Skip(1).ToArray();

        // Extract matrix values
        CorrelationMatrix = new float[GeneHeaders.Length * GeneHeaders.Length];
        for (int i = 1; i < readLines.Length; i++)
        {
            string[] values = readLines[i].Split(';').Skip(1).ToArray();
            for (int j = 0; j < values.Length; j++)
            {
                CorrelationMatrix[(i - 1) * GeneHeaders.Length + j] = float.Parse(values[j], CultureInfo.InvariantCulture);
            }
        }
    }

    private void ReadPairsFromFile(in string pathToOutputFolder, in string filename = "OutputPairs.csv")
    {
        string[] readLines = File.ReadAllLines(pathToOutputFolder + "\\" + filename);
        
        // Extract headers
        GeneHeaders = readLines[0].Split(';').Skip(1).ToArray();
        
        // Extract pairs
        Pairs = new GeneticStructures.OutputPair[readLines.Length - 1];
        for (int i = 1; i < readLines.Length; i++)
        {
            string[] values = readLines[i].Split(';');
            Pairs[i - 1] = new GeneticStructures.OutputPair(values[0], values[1], float.Parse(values[2], CultureInfo.InvariantCulture));
        }
    }
    
    #endregion

    #region Notifications
    protected virtual void NotifyProgressGenetics()
    {
        Console.WriteLine("Genetics generated.");
    }
    protected virtual void NotifyProgressCorrelationMatrix()
    {
        Console.WriteLine("Correlation matrix generated.");
    }
    protected virtual void NotifyProgressPairs()
    {
        Console.WriteLine("Pairs generated.");
    }
    protected virtual void NotifyProgressOutput()
    {
        Console.WriteLine("Output generated.");
    }
    protected virtual void NotifyProgressTree()
    {
        Console.WriteLine("Tree generated.");
    }
    
    #endregion

    #region Helpers
    
    public void ChangeFileNaming(PhylogeneticPaths fileNaming)
    {
        Options.Instance.Value.FileNaming = fileNaming;
    }
    
    protected string[] RetrieveFiles()
    {
        string[] files = Utils.General.RetrieveFilesOfType(GetFileTypes(), FoldersToProcess, SearchRecursively());
        FilterOutPaths(ref files);
        return files;
    }

    protected virtual void FilterOutPaths(ref string[] paths)
    {
        return;
    }
    #endregion
}

public enum PhylogeneticSteps
{
    Genetics,
    CorrelationMatrix,
    Pairs,
    Output,
    Tree
}

public struct PhylogeneticPaths
{
    public readonly string Genetics;            // Base case
    public readonly string Ownership;           // Genetics
    public readonly string CorrelationMatrix;   // Will need GeneticEncoding
    public readonly string Pairs;               // GeneHeaders and CorrelationMatrix
    public readonly string Output;              // Pairs and GeneHeaders
    public readonly string Tree;                // OutputPath
    public readonly string OptionJson;        // OutputPath
    
    public PhylogeneticPaths()
    {
        Genetics = "Genetics.csv";
        Ownership = "Ownership.csv";
        CorrelationMatrix = "CorrelationMatrix.csv";
        Pairs = "OutputPairs.csv";
        Output = "Output.dot"; 
        Tree = "Tree.svg"; 
        OptionJson = "Options.json";
    }
    
    public PhylogeneticPaths(string geneticPath = "Genetics.csv", string ownershipPath = "Ownership.csv",
        string correlationMatrixPath = "CorrelationMatrix.csv", string pairsPath = "OutputPairs.csv", string outputPath = "Output.dot",
        string treePath = "Tree.svg", string optionsPath = "Options.json")
    {
        Genetics = geneticPath;
        Ownership = ownershipPath;
        CorrelationMatrix = correlationMatrixPath;
        Pairs = pairsPath;
        Output = outputPath;
        Tree = treePath;
        OptionJson = optionsPath;
    }
}