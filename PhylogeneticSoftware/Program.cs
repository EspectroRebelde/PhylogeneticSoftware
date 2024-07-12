namespace PhylogeneticApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*/
            ApplicationConfiguration.Initialize();
            Application.Run(new UI.Phylogenetic_Creator());
            /*/
            var manualMain = new ManualMain();
            manualMain.Run();
            //*/
        }
    }
}