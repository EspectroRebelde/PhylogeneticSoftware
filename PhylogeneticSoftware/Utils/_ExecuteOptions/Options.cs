using System.Text.Json;

namespace PhylogeneticApp.Templates
{
    /// <summary>
    /// Options singleton class
    /// </summary>
    public class Options
    {
        private static Options instance;
        public OptionsDefinitions Value;
        
        public static Options Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Options();
                }
                return instance;
            }
        }
        
        private Options()
        {
            Value = new OptionsDefinitions();
        }
        
        public void ResetToDefault()
        {
            Value = new OptionsDefinitions();
        }
        
        public void DumpToFileJson()
        {
            using (StreamWriter writer = new StreamWriter(Value.OutputPath + "\\" + Value.FileNaming.OptionJson))
            {
                writer.Write(Value.ToJson());
            }
        }
    }
    
    public struct OptionsDefinitions
    {
        public string OutputPath;
        public PhylogeneticPaths FileNaming;
        public bool CreateOwnershipFile;
        public bool ProcessInParallel;
        public double Epsilon;

        public struct SGeneticalOptions
        {
            public struct SDataAnalysis
            {
                public bool IsDataAnalysis;
                public int NumberOfGenesToKeepFromIndividuals;
                public bool UsePercentiles;
                public bool UsePercentileFromLengthOfPopulation;
                public Tuple<float, float> PercentileToKeepFromPopulation;
                public float OutlinerThresholdFromMedianMultiplier;
                public int NumberOfGenesToKeepFromPopulation;
            }
            public SDataAnalysis DataAnalysis;
            public struct SGroupOperations
            {
                public bool IsGroupOperation;
                public bool OutputGeneHeaderData;
                public bool OutputUniqueGeneHeaderData;
                public GroupOperationType[] GroupOperationFunctions;
                public int SegregationThreshold;
            
                public enum GroupOperationType
                {
                    None,
                    Union,
                    Intersection,
                    Segregation,
                    Difference,
                }
            }
            public SGroupOperations GroupOperation;
        }
        public SGeneticalOptions GeneticOptions;
        
        
        public struct SOutputOptions
        {
            public bool OutputAlgorithmicEvaluation;
            public bool OutputAlgorithmicEvaluationPerGroup;
        }
        
        public SOutputOptions OutputOptions;
        
        
        public OptionsDefinitions()
        {
            OutputPath = "";
            FileNaming = new PhylogeneticPaths();
            CreateOwnershipFile = false;
            ProcessInParallel = true;
            Epsilon = 1e-7;
            GeneticOptions = new SGeneticalOptions
            {
                DataAnalysis = new SGeneticalOptions.SDataAnalysis
                {
                    IsDataAnalysis = false,
                    NumberOfGenesToKeepFromIndividuals = 0,
                    UsePercentiles = false,
                    UsePercentileFromLengthOfPopulation = false,
                    PercentileToKeepFromPopulation = new Tuple<float, float>(0.00f, 1f),
                    OutlinerThresholdFromMedianMultiplier = 0f,
                    NumberOfGenesToKeepFromPopulation = 0
                },
                GroupOperation = new SGeneticalOptions.SGroupOperations
                {
                IsGroupOperation = false,
                OutputGeneHeaderData = false,
                OutputUniqueGeneHeaderData = false,
                GroupOperationFunctions = new SGeneticalOptions.SGroupOperations.GroupOperationType[0],
                SegregationThreshold = 0
                }
            };
            
        }
        
        public string ToJson()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            });
            return json;
        }
        
        public static OptionsDefinitions FromJson(string json)
        {
            return JsonSerializer.Deserialize<OptionsDefinitions>(json);
        }
    }
}