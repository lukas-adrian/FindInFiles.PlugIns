using System.IO;
using System.Text;
using System.Windows;
using PDFFiles;
using PlugInBase;
using TextFilesKMP;


namespace TestApp
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();

         bool SubDirs = true;

         //TXT
         //  string sPath = @"C:\Users\lukas\Development\AdiSoft\FindInFiles.PlugIns\";
         //  string sExt = @"*.cs";
         //  string sSearchText = @"addrange";
         //
         // TextFilesKMP.SearchInTextFilesKMP textTest = new SearchInTextFilesKMP();
         //  IProgress<Int32> prog = new Progress<Int32>();
         //  textTest.FileSearchCompleted += TestOnFileSearchCompleted;
         //
         //  textTest.SearchInFolder(sPath, sExt, sSearchText, true, 0, 0, prog, CancellationToken.None);

         //PDF
         string sPath = @"C:\Users\lukas\GDrive_lukasadrian\Stellplaetze\konto";
         string sExt = @"*.pdf";
         string sSearchText = @"Entsorgung Herne Anstalt des offentlichen Rechts SEPA";
         
         SearchInPDFFiles pdfTest = new SearchInPDFFiles();
         IProgress<Int32> prog = new Progress<Int32>();
         pdfTest.FileSearchCompleted += this.TestOnFileSearchCompleted;
         
         List<String> lstAllFiles = new();
         if (SubDirs)
            lstAllFiles.AddRange(Directory.EnumerateFiles(sPath, $"{sExt}", SearchOption.AllDirectories).ToList());
         else
            lstAllFiles.AddRange(Directory.EnumerateFiles(sPath, $"{sExt}", SearchOption.TopDirectoryOnly).ToList());

         List<string> lstFilesChecked = new List<String>();
         foreach (String sFile in lstAllFiles)
         {
            //FileInfo fi = new FileInfo(sFile);
            //if(ValidateFileSize(fi, 0, 0))
               lstFilesChecked.Add(sFile);
         }
         
         pdfTest.SearchInFolder(lstFilesChecked, sSearchText, prog, CancellationToken.None);
      }

      private void TestOnFileSearchCompleted(Object? sender, FileSearchEventArgs e)
      {
         if (e.ResultList != null)
            foreach (SearchResult result in e.ResultList)
            {
               Console.WriteLine(result.FilePath);
            }
      }

   }
}