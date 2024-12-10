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

         string sPath = @"C:\Users\lukas\Development\AdiSoft\FindInFiles.PlugIns\";
         string sExt = @"cs";
         string sSearchText = @"addrange";

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