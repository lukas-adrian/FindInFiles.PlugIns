using System.Globalization;
using System.IO;
using System.Text;
using PlugInBase;
using System.Windows.Controls;

namespace PreviewText;

/// <summary>
/// Preview control for text files, uses AvalonEdit
/// </summary>
public class PreviewText : IPreviewPlugIn
{
   private ICSharpCode.AvalonEdit.TextEditor _textEditor;
   public string Name => "TextFile Preview";
   public String Description { get; } = "Preview control for text files, uses AvalonEdit";
   public Guid ID { get; } = new Guid("a851fde2-9628-46fd-bc1c-f610a5a65478");
   public List<string> Extensions { get; } = new List<string>();

   /// <summary>
   /// 
   /// </summary>
   /// <param name="filePath"></param>
   /// <returns></returns>
   public Control GetPreviewControl(String filePath)
   {
   
      _textEditor = new ICSharpCode.AvalonEdit.TextEditor
      {
         FontFamily = new System.Windows.Media.FontFamily("Consolas"),
         FontSize = 12,
         IsReadOnly = true, // Typically, previews are read-only
         ShowLineNumbers = true,
      };

      try
      {
         _textEditor.Text = File.ReadAllText(filePath);

         // Basic syntax highlighting based on extension
         string extension = Path.GetExtension(filePath)?.ToLowerInvariant();
         var highlightingManager = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance;
         var syntaxHighlighting = highlightingManager.GetDefinitionByExtension(extension);
         if (syntaxHighlighting != null)
         {
            _textEditor.SyntaxHighlighting = syntaxHighlighting;
         }
         else if (extension == ".log") // Custom handling for .log or other types if needed
         {
            // Could load a generic "Text" highlighting or leave as null
            _textEditor.SyntaxHighlighting = highlightingManager.GetDefinition("Text");
         }
      }
      catch (Exception ex)
      {
         // Handle file reading errors, maybe show an error message in the editor
         _textEditor.Text = $"Error loading file: {ex.Message}";
      }

      return _textEditor;
   }
   
   /// <summary>
   /// 
   /// </summary>
   /// <param name="param"></param>
   public void GoTo(ParameterHelper param)
   {
      
      if (_textEditor == null || !param.LineNumber.HasValue)
         return;
      
      // Use Dispatcher to delay execution until after layout is complete
      _textEditor.Dispatcher.BeginInvoke(new Action(() =>
      {
         try
         {
            _textEditor.ScrollToLine(param.LineNumber.Value);
         
            // Select the line
            int offset = _textEditor.Document.GetOffset(param.LineNumber.Value, 0);
            var line = _textEditor.Document.GetLineByNumber(param.LineNumber.Value);
            int lineLength = line?.Length ?? 0;
         
            _textEditor.SelectionStart = offset;
            _textEditor.SelectionLength = lineLength;
         }
         catch (Exception ex)
         {
            System.Diagnostics.Debug.WriteLine($"Error in GoTo: {ex.Message}");
         }
      }), System.Windows.Threading.DispatcherPriority.Background);
   }

}