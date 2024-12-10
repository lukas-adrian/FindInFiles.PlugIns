using PlugInBase;
using System.Collections.Concurrent;

namespace TextFilesKMP
{
   public sealed class SearchInTextFilesKMP : ISearchInFolderPlugIn
   {
      public event EventHandler<FileSearchEventArgs> FileSearchCompleted;

      public List<string> GetExtensions()
      {
         return new List<String>();
      }

      public async Task<List<FileSearchEventArgs>> SearchInFolder(
         String path,
         String extension,
         String searchTerm,
         Boolean subDirs,
         Int32 minFileSizeMB,
         Int32 maxFileSizeMB,
         IProgress<Int32> progress,
         CancellationToken cancellationToken)
      {
         ConcurrentBag<SearchResult> foundResults = new ConcurrentBag<SearchResult>();

         int totalFilesAllOut = 0;
         ConcurrentDictionary<String, UInt64>? dicLineNumbers = new();

         try
         {

            int processedFiles = 0;

            List<String> lstAllFiles = new();
            if (subDirs)
               lstAllFiles.AddRange(Directory.EnumerateFiles(path, $"*.{extension}", SearchOption.AllDirectories).ToList());
            else
               lstAllFiles.AddRange(Directory.EnumerateFiles(path, $"*.{extension}", SearchOption.TopDirectoryOnly).ToList());

            Parallel.ForEach(lstAllFiles, file =>
            {
               FileInfo fileInfo = new(file);

               int FileSizeMB = Convert.ToInt32(fileInfo.Length / (1024.0 * 1024.0));
               if (!(minFileSizeMB == 0 && maxFileSizeMB == 0) &&
                   (FileSizeMB < minFileSizeMB || FileSizeMB > maxFileSizeMB))
               {
                  return;
               }

               totalFilesAllOut++;

               if (cancellationToken.IsCancellationRequested)
               {
                  progress?.Report(0);
                  return;
               }


               SearchResult? foundInFile = FileContainsTermUsingKMP(file, searchTerm, dicLineNumbers);
               if (foundInFile != null)
               {
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
               new FileSearchEventArgs(
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
               new FileSearchEventArgs(
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
      /// <param name="filePath"></param>
      /// <param name="searchTerm"></param>
      /// <param name="dicLineNumbers"></param>
      private SearchResult? FileContainsTermUsingKMP(string filePath, string searchTerm, ConcurrentDictionary<String, UInt64>? dicLineNumbers = null)
      {
         SearchResult? foundResults = null;

         try
         {
            using (StreamReader reader = new StreamReader(filePath))
            {
               string line;
               int lineNumber = 0;
               while ((line = reader.ReadLine()) != null)
               {
                  lineNumber++;
                  string lineLower = line.ToLower();
                  string searchTermLower = searchTerm.ToLower();

                  int nFoundIndex = KMPAlgorithm(lineLower, searchTermLower);
                  if (nFoundIndex >= 0)
                  {
                     if (foundResults == null)
                     {
                        foundResults = new SearchResult
                        {
                           FilePath = filePath,
                        };
                     }

                     FoundItem item = new FoundItem();
                     item.LineNumber = lineNumber;
                     item.Result = line.Trim();

                     foundResults.FoundItems.Add(item);
                  }

                  if (foundResults != null)
                     foundResults.Count = $"Count: {Convert.ToUInt32(foundResults.FoundItems.Count)}";
               }

               if (dicLineNumbers != null && !dicLineNumbers.ContainsKey(filePath))
                  dicLineNumbers.TryAdd(filePath, (UInt64)new FileInfo(filePath).Length);
            }
         }
         catch (Exception ex)
         {
            OnFileSearchCompleted(new FileSearchEventArgs(
               FileSearchEventArgs.Status.Error,
               resultList: null,
               dicLineNumbers: dicLineNumbers.ToDictionary(),
               totalFilesAllOut: null,
               value: $"Error reading file {filePath}: {ex.Message}"));
         }

         return foundResults;
      }

      private int KMPAlgorithm(string text, string pattern)
      {
         int[] lps = new int[pattern.Length];
         int j = 0;
         int i = 1;
         while (i < pattern.Length)
         {
            if (pattern[i] == pattern[j])
            {
               j++;
               lps[i] = j;
               i++;
            }
            else
            {
               if (j != 0)
                  j = lps[j - 1];
               else
                  lps[i] = 0;
               i++;
            }
         }

         i = 0;
         j = 0;
         while (i < text.Length)
         {
            if (pattern[j] == text[i])
            {
               i++;
               j++;
            }
            if (j == pattern.Length)
               return i - j;

            if (i < text.Length && pattern[j] != text[i])
            {
               if (j != 0)
                  j = lps[j - 1];
               else
                  i++;
            }
         }

         return -1;
      }

      // Method to raise the event
      public void OnFileSearchCompleted(FileSearchEventArgs e)
      {
         FileSearchCompleted?.Invoke(this, e);
      }
   }
}
