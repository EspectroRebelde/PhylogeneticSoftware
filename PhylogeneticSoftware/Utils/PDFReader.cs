using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PhylogeneticApp.Utils;

public static class PDFReader
{
    /// <summary>
    /// Reads the words from a PDF file
    /// </summary>
    /// <param name="path"></param>
    /// <returns> An array of strings containing the words in the PDF file</returns>
    public static string[] ReadWordsPDF(string path)
    {
        using (PdfDocument document = PdfDocument.Open(path))
        {
            StringBuilder words = new StringBuilder();
            foreach (Page page in document.GetPages())
            {
                foreach (Word word in page.GetWords())
                {
                    words.Append(word.Text);
                    words.Append(" ");
                }
            }
            return words.ToString().Split(' ');
        }
    }
}