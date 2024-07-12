using Microsoft.Extensions.Caching.Memory;
using PhylogeneticApp.Implementations.Genetics;
using PhylogeneticApp.Templates;
using PhylogeneticApp.Utils;

namespace PhylogeneticApp;

public class ManualMain
{
    public void Run()
    {
        RunInitialSetup();
        
        // SET THE OUTPUT FOLDER
        string projectPath = ProjectSourcePath.Value;
        string outputPath = projectPath + "Results";

        string scrMLTestPath = "C:\\Users\\danie\\Documentos\\SVIT\\C_Unzipped\\scrML_test";
        string[] scrMLTest = new[]
        {
            scrMLTestPath + "\\repos_logging_library_unzip",
            scrMLTestPath + "\\repos_maps_library_unzip",
            scrMLTestPath + "\\URBOS",
            scrMLTestPath + "\\Elevator_C",
            scrMLTestPath + "\\emailclient",
            scrMLTestPath + "\\minepump",
        };
        
        var scrMLPhylogenetic = new Implementations.Phylogenetic.srcML_TreeOfSoftware_Projects_Phylogenetic(
            foldersToProcess: scrMLTest,
            outputFolder: outputPath);
        
        // RUN THE STEPS FROM THE SPECIFIED ENUM (Genetics, CorrelationMatrix, Pairs, Output, Tree)
        scrMLPhylogenetic.RunWithTimer(PhylogeneticSteps.Genetics);

    }
    
    private void RunInitialSetup()
    {
        //Memoization.Memoization.DefaultCache = new MemoryCache(new MemoryCacheOptions());
    }
}