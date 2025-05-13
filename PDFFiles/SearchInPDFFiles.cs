using System.Collections.Concurrent;
using PlugInBase;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PDFFiles;

public sealed class SearchInPDFFiles : ISearchInFolderPlugIn
{
   public event EventHandler<FileSearchEventArgs> FileSearchCompleted;
   public event EventHandler<string> DebugOutput;
   
   public List<string> GetExtensions()
   {
      return new List<String>{"*.pdf"};
   }

   /// <summary>
   /// 
   /// </summary>
   /// <param name="lstAllFiles"></param>
   /// <param name="searchTerm"></param>
   /// <param name="wholeWord"></param>
   /// <param name="progress"></param>
   /// <param name="cancellationToken"></param>
   /// <param name="matchCase"></param>
   /// <returns></returns>
   public async Task<List<FileSearchEventArgs>> SearchInFolder(
      List<String> lstAllFiles,
      String searchTerm,
      bool matchCase,
      bool wholeWord,
      IProgress<Int32> progress,
      CancellationToken cancellationToken)
   {
      OnDebugOutput("Start");
   
   
      ConcurrentBag<SearchResultFile> foundResults = new();
      int totalFilesAllOut = 0;
      ConcurrentDictionary<String, UInt64>? dicLineNumbers = new();

      try
      {
         int processedFiles = 0;
         Parallel.ForEach(lstAllFiles, file =>
         {
          
            FileInfo fileInfo = new(file);
         
            totalFilesAllOut++;
            if (cancellationToken.IsCancellationRequested)
            {
               progress?.Report(0);
               return;
            }
         
            SearchResultFile? foundInFile = SearchInFile(file, searchTerm, matchCase, wholeWord, dicLineNumbers);
            if (foundInFile != null)
            {
               OnDebugOutput($"found in {foundInFile.FilePath}");
            
               foundInFile.FileSizeBytes = Convert.ToUInt64(fileInfo.Length);
               foundInFile.FileSize = $"Size: {Convert.ToUInt64(fileInfo.Length / 1024.0)} (kB)";
               foundResults.Add(foundInFile);
            }

            processedFiles++;
            int percentage = (int)(processedFiles / (float)lstAllFiles.Count * 100);
            progress?.Report(percentage);
         
         });
         
         OnFileSearchCompleted(new FileSearchEventArgs(
            FileSearchEventArgs.Status.Completed,
            resultList: foundResults.ToList(),
            dicLineNumbers: dicLineNumbers.ToDictionary(),
            totalFilesAllOut: totalFilesAllOut));

         return new List<FileSearchEventArgs>
         {
            new(
               FileSearchEventArgs.Status.Completed,
               resultList: foundResults.ToList(),
               dicLineNumbers: dicLineNumbers.ToDictionary(),
               totalFilesAllOut: totalFilesAllOut)
         };
      }
      catch (Exception ex)
      {
         OnFileSearchCompleted(new FileSearchEventArgs(
            FileSearchEventArgs.Status.Error,
            resultList: foundResults.ToList(),
            dicLineNumbers: dicLineNumbers.ToDictionary(),
            totalFilesAllOut: totalFilesAllOut,
            value: ex.Message));

         return new List<FileSearchEventArgs>
         {
            new(
               FileSearchEventArgs.Status.Error,
               resultList: foundResults.ToList(),
               dicLineNumbers: dicLineNumbers.ToDictionary(),
               totalFilesAllOut: totalFilesAllOut,
               value: ex.Message)
         };
      }
   }

   /// <summary>
   /// 
   /// </summary>
   /// <param name="sFilePathIn"></param>
   /// <param name="searchTermIn"></param>
   /// <param name="bWholeWordIn"></param>
   /// <param name="dicLineNumbers"></param>
   /// <param name="bMatchCaseIn"></param>
   /// <returns></returns>
   private SearchResultFile? SearchInFile(string sFilePathIn, string searchTermIn, bool bMatchCaseIn, bool bWholeWordIn, ConcurrentDictionary<String, UInt64>? dicLineNumbers = null)
   {
      SearchResultFile? foundResults = null;

      try
      {
         OnDebugOutput($"start open document {sFilePathIn}");
         using (PdfDocument document = PdfDocument.Open(sFilePathIn))
         {
            OnDebugOutput($"success open document {sFilePathIn}");
         
            Parallel.ForEach(document.GetPages(), page =>
            {
               // Extract all words with positions
               var words = page.GetWords();
               
               // Group words into lines based on Y-coordinates
               var groupedLines = words
                  .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 2)) // Group by Y-coordinate
                  .OrderByDescending(g => g.Key) // Sort top-to-bottom
                  .ToList();
            
               int lineNumber = 1;
               foreach (IGrouping<Double, Word> line in groupedLines)
               {
                  string lineText = string.Join(" ", line.Select(w => w.Text));

                  bool bFound;
                  if (bWholeWordIn)
                     bFound = lineText.Equals(searchTermIn, bMatchCaseIn ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                  else
                     bFound = lineText.IndexOf(searchTermIn, bMatchCaseIn ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0;

                  if (bFound)
                  {
                     if (foundResults == null)
                     {
                        foundResults = new SearchResultFile
                        {
                           FilePath = sFilePathIn
                        };
                     }
                     
                     FoundItem item = new FoundItem();
                     item.Page = page.Number;
                     item.LineNumber = lineNumber;
                     item.Result = lineText.Trim();
                     
                     foundResults.FoundItems.Add(item);
                  }
                  
                  lineNumber++;
               }
            });
         }
      }
      catch (Exception ex)
      {
         OnFileSearchCompleted(new FileSearchEventArgs(
            FileSearchEventArgs.Status.Error,
            resultList: null,
            dicLineNumbers: dicLineNumbers.ToDictionary(),
            totalFilesAllOut: null,
            value: $"Error reading file {sFilePathIn}: {ex.Message}"));
      }
      
      return foundResults;
   }
   
   // Method to raise the event
   public void OnFileSearchCompleted(FileSearchEventArgs e)
   {
      FileSearchCompleted?.Invoke(this, e);
   }
   
   // Method to raise the event
   public void OnDebugOutput(string e)
   {
      DebugOutput?.Invoke(this, e);
   }
}