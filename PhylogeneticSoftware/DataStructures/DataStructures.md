flowchart BT
Function(Function)
Gene(Gene)
GeneStripped(GeneStripped)
GeneticDictionary(GeneticDictionary)
GeneticEncoding(GeneticEncoding)
GeneticStructures(GeneticStructures)
NodeConfig(NodeConfig)
OutputPair(OutputPair)
Pair(Pair)
PhylogeneticDictionary(PhylogeneticDictionary)
Structures(Structures)
TreeTypes(TreeTypes)
Visualization(Visualization)

Gene  -->  GeneStripped 
Gene  -->  GeneStripped 
GeneticEncoding  -->  Gene 
GeneticEncoding  -->  GeneStripped 
GeneticEncoding  -..->  GeneStripped 
GeneticStructures  -->  GeneticDictionary 
GeneticStructures  -->  OutputPair 
GeneticStructures  -->  Pair 
GeneticStructures  -->  PhylogeneticDictionary 
Pair  -->  Pair 
PhylogeneticDictionary  -->  GeneticStructures 
PhylogeneticDictionary  -->  Pair 
PhylogeneticDictionary  -->  Pair 
Visualization  -->  NodeConfig 
Visualization  -..->  NodeConfig 
Visualization  -->  Structures 
Visualization  -->  TreeTypes 
