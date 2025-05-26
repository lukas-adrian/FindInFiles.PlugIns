using System.IO;
using System.Text;
using System.Windows;
using SearchMSOffice;
using SearchPdf;
using PlugInBase;
using SearchText;


namespace TestApp
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private IPreviewPlugIn objPreview = new PreviewPdf.PreviewPdf();
   
   
      public MainWindow()
      {
         InitializeComponent();


         //TXT
         /*string sPath = @"C:\Users\lukas\Development\AdiSoft\FindInFiles.PlugIns\";
         string sExt = @"*.cs";
         string sSearchText = @"addrange";
         
         ISearchInFolderPlugIn objPlugIn = new SearchText();*/

         //PDF
         /*string sPath = @"C:\Users\lukas\Development\AdiSoft\FindInFiles.PlugIns\PreviewPdf";
         string sExt = @"*.pdf";
         string sSearchText = @"SCHEMATA";
         ISearchInFolderPlugIn objPlugIn = new SearchPdf();*/

         /*objPreview = new PreviewPdf.PreviewPdf();
         ParameterHelper cParam = new ParameterHelper();
         cParam.PageNumber = 4;
         System.Windows.Controls.Control preveiw = objPreview.GetPreviewControl(@"C:\Users\lukas\Development\AdiSoft\FindInFiles.PlugIns\PreviewPdf\test2.pdf");
         PreviewHost.Content = preveiw;*/

         //Office
         string sPath = @"C:\tmp\officetest";
         string sExt = @"*.docx";
         string sSearchText = @"Seite 2";
         ISearchInFolderPlugIn objPlugIn = new SearchMSOffice.SearchMSOffice();
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

      private void ButtonBase_OnClick(Object sender, RoutedEventArgs e)
      {
         /*int nPage = Int32.Parse(tbPage.Text);
         objPreview.SetPage(nPage);

         if (nPage == 10)
         {
            
         }*/
      }
   }
}