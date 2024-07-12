using System.Diagnostics;
using PhylogeneticApp.DataStructures;
using PhylogeneticApp.Templates;

namespace PhylogeneticApp.Implementations.Genetics;

public class srcML_TreeOfSoftware_ProjectsEncoding : _IEncodingGenerator<float>
{
    private readonly _IEncodingGenerator<float> singleProjectFilesIEncoding;
    private readonly string srcMLPath = @"C:\Program Files\srcML\srcml.exe";
    private readonly bool createFilesIfFolderDoesExist = false;
    
    public override bool IsNonAlignedEncoding => singleProjectFilesIEncoding.IsNonAlignedEncoding;
    public srcML_TreeOfSoftware_ProjectsEncoding(_IEncodingGenerator<float> singleProjectFilesIEncoding) : base()
    {
        this.singleProjectFilesIEncoding = singleProjectFilesIEncoding;
    }
    
    public override float DefaultValueForGene()
    {
        return 0;
    }

    protected override bool GenerateGene(in string folderPath, out Gene<float> gene)
    {
        ProcessFilesInFolder(folderPath, out gene);
        return false;
    }

    public override void GenerateEncoding(in string[] files, ref DataStructures.Gene<float>[] genes,
        bool processInParallel = false)
    {
        genes = new DataStructures.Gene<float>[files.Length];
        var geneticCopy = genes;
        string[] newPaths = new string[files.Length];
        int length = files.Length;
        CountdownEvent countdownEvent = new CountdownEvent(length);
        for (int i = 0; i < length; i++)
        {
            int index = i;
            string file = files[index];
            ThreadPool.QueueUserWorkItem(_ =>
            {
                ConvertToSrcML(file, out newPaths[index]);
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
        
        singleProjectFilesIEncoding.foldersInProcess = newPaths;
        singleProjectFilesIEncoding.Initialize();
        
        // Now we process the files in the new paths
        countdownEvent.Reset(length);
        for (int i = 0; i < length; i++)
        {
            int index = i;
            string path = newPaths[index];
            ThreadPool.QueueUserWorkItem(_ =>
            {
                ProcessFilesInFolder(path, out geneticCopy[index]);
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
    }
    
    private void ConvertToSrcML(string folderPath, out string outputFolder)
    {
        //We create a new folder inside the folder path to store the output files
        outputFolder = folderPath + "\\srcML_Encoding";
        if (createFilesIfFolderDoesExist || !Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            
            string[] files = Directory.GetFiles(folderPath, "*.c", SearchOption.AllDirectories);
            // Then, we will execute the srcml FILENAME.C -o FILENAME.xml command for each file to generate the xml files and put them in the output folder
            foreach (var file in files)
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    string xmlFile = outputFolder + "\\" + fileName + ".xml";
                    string arguments = $"\"{file}\" -o \"{xmlFile}\"";
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = srcMLPath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
            
                    using (Process process = Process.Start(startInfo))
                    {
                        process.WaitForExit();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    private void ProcessFilesInFolder(in string folderPath, out DataStructures.Gene<float> gene)
    {
        DataStructures.Gene<float>[] projectGenes = Array.Empty<DataStructures.Gene<float>>();
        string[] files = Directory.GetFiles(folderPath, "*.xml", SearchOption.AllDirectories);
        singleProjectFilesIEncoding.GenerateEncoding(files, ref projectGenes, true);
        
        // Call the flatten function to flatten the array of genes
        if (!singleProjectFilesIEncoding.IsNonAlignedEncoding)
        {
            gene = Flatten(name: folderPath.Split(Path.DirectorySeparatorChar)[^2], genes: projectGenes,
                mergeFunction: (gene1, gene2) => gene1 + gene2);
        }
        else
        {
            gene = NonAlignedFlatten(name: folderPath.Split(Path.DirectorySeparatorChar)[^2], genes: projectGenes);
        }
    }
}