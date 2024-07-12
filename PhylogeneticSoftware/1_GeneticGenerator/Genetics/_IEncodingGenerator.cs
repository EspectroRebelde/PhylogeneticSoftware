using System.Text;
using PhylogeneticApp.Utils;

namespace PhylogeneticApp.Templates;

#nullable disable
public abstract class _IEncodingGenerator<T>
{
    public string[] foldersInProcess;
    public abstract T DefaultValueForGene();
    /// <summary>
    /// Generates a gene from a file allowing for multiple genes to be generated from the same file
    /// </summary>
    /// <param name="filePath"> The path to the file to be processed in this iteration</param>
    /// <param name="gene"> The gene that will be generated from the file iteration</param>
    /// <returns>True if more genes can be generated from the same file, false otherwise</returns>
    /// <remarks>For the multiple genes per file case, override the <b>CanReturnMultipleGenesPerFile</b> property to true</remarks>
    protected abstract bool GenerateGene(in string filePath, out DataStructures.Gene<T> gene);

    /// <summary>
    /// If the generator can return multiple genes per file, override this property to return true
    /// </summary>
    protected virtual bool CanReturnMultipleGenesPerFile => false;
    
    /// <summary>
    /// If the encoding is non-aligned, override this property to return true
    /// </summary>
    public virtual bool IsNonAlignedEncoding => false;
    
    /// <summary>
    /// Generates the encoding for the given files
    /// </summary>
    /// <param name="files"> The files paths to be processed</param>
    /// <param name="genes"> The genes that will be generated from the files</param>
    /// <param name="processInParallel"> If the files should be processed in parallel</param>
    /// <remarks> This method will call the <b>GenerateGene</b> method for each file in the files array.<br></br>
    /// <list type="bullet">
    /// <item> If <b>CanReturnMultipleGenesPerFile</b> is false, the method will create an array of genes with the same length as the files array.</item>
    /// <item> If <b>CanReturnMultipleGenesPerFile</b> is true, the method will create a list of genes from the
    /// files array with each file allowed to create multiple genes.</item>
    /// </list> </remarks>
    public virtual void GenerateEncoding(in string[] files, ref DataStructures.Gene<T>[] genes, bool processInParallel = false)
    {
        if (!CanReturnMultipleGenesPerFile) 
        {
            genes = new DataStructures.Gene<T>[files.Length];
            using (Utils.ProgressBar progress = new Utils.ProgressBar())
            {
                if (processInParallel)
                {
                    int length = files.Length;
                    DataStructures.Gene<T>[] geneticCopy = genes;
                    Parallel.ForEach(files, (file, state, index) =>
                    {
                        GenerateGene(file, out geneticCopy[index]);
                        progress.Report((float)index / length);
                    });
                    genes = geneticCopy;
                }
                else
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        GenerateGene(files[i], out genes[i]);
                        progress.Report((float)i / files.Length);
                    }
                }
            }
        } 
        else 
        {
            List<DataStructures.Gene<T>> genesList = new();
            using (Utils.ProgressBar progress = new Utils.ProgressBar())
            {
                for (int i = 0; i < files.Length; i++)
                {
                    while (GenerateGene(files[i], out DataStructures.Gene<T> gene))
                    {
                        genesList.Add(gene);
                    }
                    progress.Report((float)i / files.Length);
                }
            }
            genes = genesList.ToArray();
        }
    }
    
    /// <summary>
    /// Flattens the array of genes into a single gene
    /// </summary>
    /// <param name="name"> The name of the gene to be created</param>
    /// <param name="genes"> The array of genes to be flattened</param>
    /// <param name="mergeFunction"> The function to merge the values of the genes</param>
    /// <returns> The gene created from the array of genes</returns>
    protected DataStructures.Gene<T> Flatten(string name, DataStructures.Gene<T>[] genes, Func<T, T, T> mergeFunction)
    {
        List<string> allHeaders = new();
        foreach (var gene in genes)
        {
            allHeaders.AddRange(gene.Headers);
        }
        // We remove the duplicates
        allHeaders = allHeaders.Distinct().ToList();
        
        T[] values = new T[allHeaders.Count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = DefaultValueForGene();
        }
        foreach (var gene in genes)
        {
            for (int i = 0; i < gene.Headers.Length; i++)
            {
                int index = allHeaders.IndexOf(gene.Headers[i]);
                if (index != -1)
                {
                    values[index] = mergeFunction(values[index], gene.Value[i]);
                }
            }
        }
        
        return new DataStructures.Gene<T>(name, allHeaders.ToArray(), values);
    }
    
    protected DataStructures.Gene<T> NonAlignedFlatten(string name, DataStructures.Gene<T>[] genes)
    {
        List<T> allValues = new();
        foreach (var gene in genes)
        {
            allValues.AddRange(gene.Value);
        }
        return new DataStructures.Gene<T>(
            name, 
            Enumerable.Range(1, allValues.Count).Select(x => x.ToString()).ToArray(),
            allValues.ToArray());
    }
    
    /// <summary>
    /// Ensures the gene names are unique
    /// </summary>
    /// <param name="genes"> The array of genes to be checked</param>
    protected void EnsureUniqueNames(ref DataStructures.Gene<T>[] genes)
    {
        // Ensure the gene names are unique. If not, add a number to the name equal to the number of times it appears
        Dictionary<string, int> nameCount = new();
        for (int i = 0; i < genes.Length; i++)
        {
            if (!nameCount.TryAdd(genes[i].Name, 0))
            {
                nameCount[genes[i].Name]++;
                // Add the number to the name as a string concatenated "({name}{number})"
                genes[i].Name += $"({nameCount[genes[i].Name]})";
            }
        }
    }
#nullable restore
    
    public void TestGeneratedGene(string filePath, ConsoleLogging.TypeDescriptor? type)
    {
        //Newlines should move to the start of the next line
        GenerateGene(filePath, out DataStructures.Gene<T> gene);
        ConsoleLogging.PrintGeneToConsole(gene, type);
    }

    #region UTILS

            /// <summary>
        /// Writes a file with the headers and the ocurrance of each on each gene (each can only be or not be present counting from no occurance to genes.Length occurances)
        /// </summary>
        /// <param name="genes"></param>
        /// <param name="fileName"></param>
        protected void OutputGenesHeaderData(DataStructures.Gene<float>[] genes, string fileName = "GeneOrder.csv")
        {
            if (IsNonAlignedEncoding)
            {
                return;
            }
            
            Dictionary<string, int> headersData = new();
            //Foreach gene
            for (int i = 0; i < genes.Length; i++)
            {
                //Foreach header
                for (int j = 0; j < genes[i].Headers.Length; j++)
                {
                    //If the header is not in the dictionary, add it
                    if (!headersData.ContainsKey(genes[i].Headers[j]))
                    {
                        headersData.Add(genes[i].Headers[j], 0);
                    }
                    //Increment the occurance of the header in the gene
                    headersData[genes[i].Headers[j]]++;
                }
            }
            
            using (StreamWriter writer = new StreamWriter(Options.Instance.Value.OutputPath + "\\" + fileName, false, Encoding.UTF8))
            {
                writer.WriteLine("Header;Occurrences;;Total;SingleGeneOccurances;DoubleGeneOccurances");
                
                //Print in descending order
                int singleGeneOccurrences = 0;
                int doubleGeneOccurrences = 0;
                headersData = headersData.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                //Count the number of 2's and 1's
                foreach (var pair in headersData)
                {
                    if (pair.Value == 1)
                    {
                        singleGeneOccurrences++;
                    }
                    else if (pair.Value == 2)
                    {
                        doubleGeneOccurrences++;
                    }
                }
                writer.WriteLine($"{headersData.First().Key};{headersData.First().Value};;{headersData.Count};{singleGeneOccurrences};{doubleGeneOccurrences}");
                foreach (var pair in headersData.Skip(1))
                {
                    writer.WriteLine($"{pair.Key.Replace(";", "").Replace("\"","")};{pair.Value}");
                }

            }
        }
        
        protected DataStructures.Gene<float>[] DifferenceCodeGroup(string[] files, DataStructures.Gene<float>[] genes)
        {
            Dictionary<string, string> parentFolders = new();
            Dictionary<string, List<string>> parentFolderHeaders = new();
            for (var i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string parentFolder = file.Split(Path.DirectorySeparatorChar).Reverse().Skip(1).First();
                parentFolders.TryAdd(file, parentFolder);
                parentFolderHeaders.TryAdd(parentFolder, new List<string>());
                //Try to add the headers for that parent folder
                for (int j = 0; j < genes[i].Headers.Length; j++)
                {
                    // Add the headers to the parent folder (if not already added)
                    if (!parentFolderHeaders[parentFolder].Contains(genes[i].Headers[j]))
                    {
                        parentFolderHeaders[parentFolder].Add(genes[i].Headers[j]);
                    }
                }
            }
            
            // Get the unique headers for each parent folder (difference)
            Dictionary<string, List<string>> uniqueHeaders = new();
            foreach (var parentFolder in parentFolderHeaders.Keys)
            {
                uniqueHeaders.TryAdd(parentFolder, parentFolderHeaders[parentFolder]);
                foreach (var otherParentFolder in parentFolderHeaders.Keys)
                {
                    if (parentFolder == otherParentFolder) continue;
                    uniqueHeaders[parentFolder] = uniqueHeaders[parentFolder]
                        .Except(parentFolderHeaders[otherParentFolder]).ToList();
                }
            }
            
            // Remove the headers from the genes and only keep the unique ones for each parent folder
            for (var i = 0; i < genes.Length; i++)
            {
                DataStructures.Gene<float> gene = new DataStructures.Gene<float>();
                gene.Name = genes[i].Name;
                string parentFolder = parentFolders[files[i]];
                //Keep only the unique headers for that parent folder
                for (int j = 0; j < genes[i].Headers.Length; j++)
                {
                    // If contained in the unique headers, add it to the gene
                    if (uniqueHeaders[parentFolder].Contains(genes[i].Headers[j]))
                    {
                        gene.Headers = gene.Headers.Append(genes[i].Headers[j]).ToArray();
                        gene.Value = gene.Value.Append(genes[i].Value[j]).ToArray();
                    }
                }
                genes[i] = gene;
            }
            
            // Remove any empty genes
            genes = genes.Where(gene => gene.Headers.Length > 0).ToArray();
            
            return genes;
        }
        
        protected DataStructures.Gene<float>[] SegregationCodeGroup(DataStructures.Gene<float>[] genes, int segregationThreshold)
        {
            Dictionary<string, int> headersData = new();
            //Foreach gene
            for (int i = 0; i < genes.Length; i++)
            {
                //Foreach header
                for (int j = 0; j < genes[i].Headers.Length; j++)
                {
                    //If the header is not in the dictionary, add it
                    if (!headersData.ContainsKey(genes[i].Headers[j]))
                    {
                        headersData.Add(genes[i].Headers[j], 0);
                    }
                    //Increment the occurance of the header in the gene
                    headersData[genes[i].Headers[j]]++;
                }
            }
            
            // Get the headers that are present in more than the threshold of genes
            List<string> headersToKeep = headersData
                .Where(pair => pair.Value > segregationThreshold)
                .Select(pair => pair.Key)
                .ToList();
            
            // Remove the headers from the genes and only keep the ones that are present in more than the threshold of genes
            for (var i = 0; i < genes.Length; i++)
            {
                DataStructures.Gene<float> gene = new DataStructures.Gene<float>();
                gene.Name = genes[i].Name;
                //Keep only the headers that are present in more than the threshold of genes
                for (int j = 0; j < genes[i].Headers.Length; j++)
                {
                    // If contained in the headers to keep, add it to the gene
                    if (headersToKeep.Contains(genes[i].Headers[j]))
                    {
                        gene.Headers = gene.Headers.Append(genes[i].Headers[j]).ToArray();
                        gene.Value = gene.Value.Append(genes[i].Value[j]).ToArray();
                    }
                }
                genes[i] = gene;
            }
            
            // Remove any empty genes
            genes = genes.Where(gene => gene.Headers.Length > 0).ToArray();
            
            return genes;
        }

        protected void DataAnalysis(DataStructures.Gene<float> gene, int defaultDataAnalysisValues)
        {
            int[] maxValuesIndexes = gene.Value
                .Select((value, index) => new { Value = value, Index = index })
                .OrderByDescending(x => x.Value)
                .Take(defaultDataAnalysisValues)
                .Select(x => x.Index)
                .ToArray();
        
            // Reduce the gene to those indexes
            gene = new DataStructures.Gene<float>(
                name: gene.Name,
                headers: maxValuesIndexes.Select(index => gene.Headers[index]).ToArray(),
                value: maxValuesIndexes.Select(index => gene.Value[index]).ToArray()
            );
        }

        public virtual void Initialize()
        {
            // Initialize the encoding generator
        }

        #endregion
}
