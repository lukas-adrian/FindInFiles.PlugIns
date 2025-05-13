using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using PlugInBase;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.Json;
using Text = DocumentFormat.OpenXml.Drawing.Text;

namespace MSOffice
{
   public class SearchInMSOfficeFiles : ISearchInFolderPlugIn
   {
      public event EventHandler<FileSearchEventArgs> FileSearchCompleted;
      public event EventHandler<string> DebugOutput;
      private Dictionary<string, List<string>> _definitions;
      
      public SearchInMSOfficeFiles()
      {
         string resourceName = "MSOffice.defintions.json";
         using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
         using var reader = new StreamReader(stream);
         string json = reader.ReadToEnd();

         _definitions = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
      }

      public List<string> GetExtensions()
      {
         List<string> lstExt = new();
         foreach (List<String> lstExtTmp in _definitions.Values)
         {
            lstExt.AddRange(lstExtTmp);
         }
     
         return lstExt;
      }

      // Method to raise the event
      public void OnFileSearchCompleted(FileSearchEventArgs e)
      {
         FileSearchCompleted?.Invoke(this, e);
      }

      public void OnDebugOutput(string e)
      {
         DebugOutput?.Invoke(this, e);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="lstAllFiles"></param>
      /// <param name="searchTerm"></param>
      /// <param name="matchCase"></param>
      /// <param name="wholeWord"></param>
      /// <param name="progress"></param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      public async Task<List<FileSearchEventArgs>> SearchInFolder(
         List<String> lstAllFiles,
         String searchTerm,
         bool matchCase,
         bool wholeWord,
         IProgress<Int32> progress,
         CancellationToken cancellationToken)
      {
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
               string sExt = fileInfo.Extension;
               
               List<FoundItem> lstResultFile = null;
               KeyValuePair<String, List<String>> kvpMatch = _definitions.FirstOrDefault(kvp => kvp.Value.Contains(sExt, StringComparer.OrdinalIgnoreCase));
                  
               if (string.Compare(kvpMatch.Key, "Word") == 0)
               {//Word
                  lstResultFile = SearchInWordDocument(file, searchTerm, matchCase, wholeWord);
               }
               else if (string.Compare(kvpMatch.Key, "Excel") == 0)
               {//Excel
                  lstResultFile = SearchInExcelDocument(file, searchTerm, matchCase, wholeWord);
               }
               else if (string.Compare(kvpMatch.Key, "PowerPoint") == 0)
               {//PowerPoint
                  lstResultFile = SearchInPowerPointDocument(file, searchTerm, matchCase, wholeWord);
               }

               SearchResultFile resultFile = null;
               
               
               if (lstResultFile != null && lstResultFile.Count > 0)
               {
                  resultFile = new SearchResultFile()
                  {
                     FilePath = file
                  };

                  foreach (FoundItem item in lstResultFile)
                  {
                     resultFile.FoundItems.Add(item);
                  }
               }

               
               if (resultFile != null) foundResults.Add(resultFile);

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
      /// Searches the in word document.
      /// </summary>
      /// <param name="sFilePath">The file path.</param>
      /// <param name="sSearchTerm">The search term.</param>
      /// <param name="bMatchCaseIn"></param>
      /// <param name="bWholeWordIn"></param>
      /// <returns></returns>
      private List<FoundItem> SearchInWordDocument(string sFilePath, string sSearchTerm, bool bMatchCaseIn, bool bWholeWordIn)
      {
         List<FoundItem> lstResult = new List<FoundItem>();
         using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(sFilePath, false))
         {
            var body = wordDoc.MainDocumentPart.Document.Body;
            if (body != null)
            {
               foreach (Paragraph paraObj in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
               {
                  string sParaText = paraObj.InnerText;

                  bool bFound = false;
                  if (bWholeWordIn)
                     bFound = !string.IsNullOrEmpty(sParaText) && sParaText.Equals(sSearchTerm, bMatchCaseIn ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                  else
                     bFound = !string.IsNullOrEmpty(sParaText) && sParaText.IndexOf(sSearchTerm, bMatchCaseIn ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0;

                  if (bFound)
                  {
                     FoundItem serachItem = new FoundItem
                     {
                        Result = sParaText
                     };
                     lstResult.Add(serachItem);
                  }
               }
            }
         }

         return lstResult;
      }

      /// <summary>
      /// Searches the in excel document.
      /// </summary>
      /// <param name="sFilePathIn">The file path.</param>
      /// <param name="sSearchTerm">The search term.</param>
      /// <param name="bMatchCaseIn"></param>
      /// <param name="bWholeWordIn"></param>
      /// <returns></returns>
      private List<FoundItem> SearchInExcelDocument(string sFilePathIn, string sSearchTerm, bool bMatchCaseIn, bool bWholeWordIn)
      {
         List<FoundItem> lstResult = new List<FoundItem>();
         using (SpreadsheetDocument excelDoc = SpreadsheetDocument.Open(sFilePathIn, false))
         {
            foreach (WorksheetPart worksheetPart in excelDoc.WorkbookPart.WorksheetParts)
            {

               Sheet sheet = GetSheetFromWorkSheet(excelDoc.WorkbookPart, worksheetPart);
               string sheetName = sheet.Name;

               var rows = worksheetPart.Worksheet.Descendants<Row>();
               int rowIndex = 1; // Excel rows are 1-based, so start from 1
               foreach (Row row in rows)
               {
                  int columnIndex = 0;
                  foreach (Cell cell in row.Elements<Cell>())
                  {
                     columnIndex++;
                     string cellValue = GetCellValue(cell, excelDoc);


                     bool bFound = false;
                     if (bWholeWordIn)
                        bFound = !string.IsNullOrEmpty(cellValue) && cellValue.Equals(sSearchTerm, bMatchCaseIn ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                     else
                        bFound = !string.IsNullOrEmpty(cellValue) && cellValue.IndexOf(sSearchTerm, bMatchCaseIn ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0;


                     if (bFound)
                     {
                        string columnLetter = GetColumnNameFromIndex(columnIndex);
                        FoundItem searchItem = new FoundItem
                        {
                           Column = columnLetter,
                           Row = rowIndex,
                           Result = cellValue,
                           Sheet = sheetName
                        };
                        lstResult.Add(searchItem);
                     }
                  }

                  rowIndex++;
               }
            }
         }

         return lstResult;
      }

      private Sheet GetSheetFromWorkSheet(WorkbookPart workbookPart, WorksheetPart worksheetPart)
      {
         string relationshipId = workbookPart.GetIdOfPart(worksheetPart);
         IEnumerable<Sheet> sheets = workbookPart.Workbook.Sheets.Elements<Sheet>();
         return sheets.FirstOrDefault(s => s.Id.HasValue && s.Id.Value == relationshipId);
      }

      /// <summary>
      /// Get column letter from column index (1-based index)
      /// </summary>
      /// <param name="index"></param>
      /// <returns></returns>
      private string GetColumnNameFromIndex(int index)
      {
         string columnName = string.Empty;
         while (index > 0)
         {
            int remainder = (index - 1) % 26;
            columnName = (char)(remainder + 65) + columnName;
            index = (index - 1) / 26;
         }

         return columnName;
      }

      /// <summary>
      /// Gets the cell value.
      /// </summary>
      /// <param name="cell">The cell.</param>
      /// <param name="document">The document.</param>
      /// <returns></returns>
      private string GetCellValue(Cell cell, SpreadsheetDocument document)
      {
         if (cell.CellValue == null) return string.Empty;

         var value = cell.CellValue.InnerText;

         if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
         {
            var stringTable = document.WorkbookPart.SharedStringTablePart.SharedStringTable;
            return stringTable.ChildElements[int.Parse(value)].InnerText;
         }

         return value;
      }

      /// <summary>
      /// Searches the in power point document.
      /// </summary>
      /// <param name="sFilePathIn">The file path.</param>
      /// <param name="sSearchTermIn">The search term.</param>
      /// <param name="bMatchCaseIn"></param>
      /// <param name="bWholeWordIn"></param>
      /// <returns></returns>
      private List<FoundItem> SearchInPowerPointDocument(string sFilePathIn, string sSearchTermIn, bool bMatchCaseIn, bool bWholeWordIn)
      {
         List<FoundItem> lstResult = new List<FoundItem>();
         using (PresentationDocument pptDoc = PresentationDocument.Open(sFilePathIn, false))
         {
            var slides = pptDoc.PresentationPart.SlideParts;
            int slideIndex = 1;
            foreach (SlidePart slide in slides)
            {
               IEnumerable<Text> iTexts = slide.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>();
               foreach (Text slideTxt in iTexts)
               {
                  string sText = slideTxt.Text;

                  bool bFound = false;
                  if (bWholeWordIn)
                     bFound = !string.IsNullOrEmpty(sText) && sText.Equals(sSearchTermIn, bMatchCaseIn ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                  else
                     bFound = !string.IsNullOrEmpty(sText) && sText.IndexOf(sSearchTermIn, bMatchCaseIn ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0;


                  if (bFound)
                  {
                     lstResult.Add(new FoundItem
                     {
                        Page = slideIndex, // Slide number
                        Result = sText
                     });
                  }
               }

               slideIndex++;
            }
         }

         return lstResult;
      }
   }

}