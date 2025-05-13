using System.IO;
using System.Text;
using System.Windows;
using MSOffice;
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


         //TXT
         /*string sPath = @"C:\Users\lukas\Development\AdiSoft\FindInFiles.PlugIns\";
         string sExt = @"*.cs";
         string sSearchText = @"addrange";
         
         ISearchInFolderPlugIn objPlugIn = new SearchInTextFilesKMP();*/

         //PDF
         /*string sPath = @"C:\tmp\pdftest";
         string sExt = @"*.pdf";
         string sSearchText = @"some text";
         ISearchInFolderPlugIn objPlugIn = new SearchInPDFFiles();*/
         
         //Office
         string sPath = @"C:\tmp\officetest";
         string sExt = @"*.docx";
         string sSearchText = @"test";
         ISearchInFolderPlugIn objPlugIn = new SearchInMSOfficeFiles();
         objPlugIn.FileSearchCompleted += this.TestOnFileSearchCompleted;
         
         
         
         IProgress<Int32> prog = new Progress<Int32>();
         bool SubDirs = true;
         
         
         List<String> lstAllFiles = new();
         if (SubDirs)
            lstAllFiles.AddRange(Directory.EnumerateFiles(sPath, $"{sExt}", SearchOption.AllDirectories).ToList());
         else
            lstAllFiles.AddRange(Directory.EnumerateFiles(sPath, $"{sExt}", SearchOption.TopDirectoryOnly).ToList());

         List<string> lstFilesChecked = new List<String>();
         foreach (String sFile in lstAllFiles)
         {
            lstFilesChecked.Add(sFile);
         }
         
         objPlugIn.SearchInFolder(lstFilesChecked, sSearchText, false, false, prog, CancellationToken.None);
      }

      private void TestOnFileSearchCompleted(Object? sender, FileSearchEventArgs e)
      {
         if (e.ResultList != null)
            foreach (SearchResultFile result in e.ResultList)
            {
               Console.WriteLine(result.FilePath);
            }
      }

   }
}