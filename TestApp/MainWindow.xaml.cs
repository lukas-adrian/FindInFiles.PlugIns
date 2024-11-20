using System.Text;
using System.Windows;
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

         string sPath = @"C:\AB_DATE\Organizations\";
         string sExt = @"abs";
         string sSearchText = @"cadbea20-544c-497a-b486-faa5dd54c6c";

         TextFilesKMP.SearchInTextFilesKMP test = new SearchInTextFilesKMP();
         IProgress<Int32> prog = new Progress<Int32>();
         test.FileSearchCompleted += this.Test_FileSearchCompleted;

         test.SearchInFolder(sPath, sExt, sSearchText, true, 0, 0, prog, CancellationToken.None);
      }

      private void Test_FileSearchCompleted(object? sender, FileSearchEventArgs e)
      {

      }
   }
}