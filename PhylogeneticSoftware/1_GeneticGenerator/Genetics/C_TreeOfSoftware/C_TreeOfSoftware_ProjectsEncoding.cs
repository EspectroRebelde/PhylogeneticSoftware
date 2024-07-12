#define DATA_ANALYSIS

using System.Security.Policy;
using System.Text;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.Genetics
{
    public class C_TreeOfSoftware_ProjectsEncoding : _IEncodingGenerator<float>
    {
        private readonly _IEncodingGenerator<float> singleProjectFilesIEncoding;
        public C_TreeOfSoftware_ProjectsEncoding(_IEncodingGenerator<float> singleProjectFilesIEncoding) : base()
        {
            this.singleProjectFilesIEncoding = singleProjectFilesIEncoding;
        }
    
        public override float DefaultValueForGene()
        {
            return 0;
        }
        protected override bool GenerateGene(in string folderPath, out DataStructures.Gene<float> gene)
        {
            ProcessFilesInFolder(folderPath, out gene);
            return false;
        }
    
        public override void GenerateEncoding(in string[] files, ref DataStructures.Gene<float>[] genes, bool processInParallel = false)
        {
            genes = new DataStructures.Gene<float>[files.Length];
            var geneticCopy = genes;
            int length = files.Length;
            CountdownEvent countdownEvent = new CountdownEvent(length);
            for (int i = 0; i < length; i++)
            {
                int index = i;
                string file = files[index];
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    GenerateGene(file, out geneticCopy[index]);
                    countdownEvent.Signal();
                });
            }
            
            //Print a progress bar while processing the files
            using (Utils.ProgressBar progressBar = new Utils.ProgressBar())
            {
                while (!countdownEvent.IsSet)
                {
                    progressBar.Report((float)(length - countdownEvent.CurrentCount) / length);
                    Thread.Sleep(100);
                }
            }
            
            if (Options.Instance.Value.GeneticOptions.GroupOperation.IsGroupOperation)
            {
                if (Options.Instance.Value.GeneticOptions.GroupOperation.OutputGeneHeaderData)
                {
                    OutputGenesHeaderData(genes);
                }

                foreach (var operation in Options.Instance.Value.GeneticOptions.GroupOperation.GroupOperationFunctions)
                {
                    switch (operation)
                    {
                        // case OptionsDefinitions.GeneticalOptions.GroupOperations.GroupOperationType.Union:
                        //     UnionCodeGroup(files, ref genes);
                        //     break;
                        // case OptionsDefinitions.GeneticalOptions.GroupOperations.GroupOperationType.Intersection:
                        //     IntersectionCodeGroup(files, ref genes);
                        //     break;
                        case OptionsDefinitions.SGeneticalOptions.SGroupOperations.GroupOperationType.Difference:
                            genes = DifferenceCodeGroup(files, genes);
                            break;
                        case OptionsDefinitions.SGeneticalOptions.SGroupOperations.GroupOperationType.Segregation:
                            genes = SegregationCodeGroup(genes, Options.Instance.Value.GeneticOptions.GroupOperation.SegregationThreshold);
                            break;
                        case OptionsDefinitions.SGeneticalOptions.SGroupOperations.GroupOperationType.None:
                            break;
                    }
                }
                
                if (Options.Instance.Value.GeneticOptions.GroupOperation.OutputUniqueGeneHeaderData)
                {
                    string operationName = "";
                    foreach (var operation in Options.Instance.Value.GeneticOptions.GroupOperation.GroupOperationFunctions)
                    {
                        operationName += Utils.General.GroupOperationTypeToString(operation) + "_";
                    }
                    
                    OutputGenesHeaderData(genes, operationName + "GeneOrder.csv");
                }
            }
        }
    
        private void ProcessFilesInFolder(in string folderPath, out DataStructures.Gene<float> gene)
        {
            // If the filename of the folder is 311, stop for breakpoint
            // string folderName = folderPath.Split(Path.DirectorySeparatorChar).Last();
            // if (folderName == "79.AlaskanEmily_bufferfile")
            // {
            //     Console.WriteLine("Stop for breakpoint");
            // }
            DataStructures.Gene<float>[] projectGenes = Array.Empty<DataStructures.Gene<float>>();
            string[] files = Directory.GetFiles(folderPath, "*.c", SearchOption.AllDirectories);
            singleProjectFilesIEncoding.GenerateEncoding(files, ref projectGenes, true);
            
            // Call the flatten function to flatten the array of genes
            gene = Flatten(
                name: folderPath.Split(Path.DirectorySeparatorChar).Last(),
                genes: projectGenes,
                mergeFunction: (gene1, gene2) => gene1 + gene2
            );

            // Data analysis
            if (Options.Instance.Value.GeneticOptions.DataAnalysis.IsDataAnalysis && Options.Instance.Value.GeneticOptions.DataAnalysis.NumberOfGenesToKeepFromIndividuals > 0)
            {
                DataAnalysis(gene, Options.Instance.Value.GeneticOptions.DataAnalysis.NumberOfGenesToKeepFromIndividuals);
            }
        }
    }
}