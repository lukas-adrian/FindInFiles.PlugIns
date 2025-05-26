using PlugInBase;
using System.Windows.Forms.Integration;

namespace PreviewPdf;

/// <summary>
/// Preview control for PDF Files, uses PdfiumViewer and pdfium
/// </summary>
public class PreviewPdf : IPreviewPlugIn
{
   private PdfiumViewer.PdfViewer _pdfViewer;
   
   public string Name => "PDFPreview (PdfiumViewer)";
   public string Description => "Preview control for PDF Files, uses PdfiumViewer and pdfium";
   public Guid ID { get; } = new("d5a68830-ae7e-4537-a32b-398eadb8b2b1");
   public List<string> Extensions { get; } = [".pdf"];

   /// <summary>
   /// 
   /// </summary>
   /// <param name="filePath"></param>
   /// <returns></returns>
   public System.Windows.Controls.Control GetPreviewControl(String filePath)
   {
      _pdfViewer = new PdfiumViewer.PdfViewer();
      _pdfViewer.Document = PdfiumViewer.PdfDocument.Load(filePath);
      
      WindowsFormsHost host = new WindowsFormsHost();
      host.Child = _pdfViewer;
      var wrapper = new System.Windows.Controls.ContentControl();
      wrapper.Content = host;
      
      return wrapper;
   }
   
   /// <summary>
   /// 
   /// </summary>
   /// <param name="param"></param>
   public void GoTo(ParameterHelper param)
   {
      if (_pdfViewer == null)
         return;
         
      if (_pdfViewer.Document != null)
      {
         if (param.PageNumber.HasValue && param.PageNumber < _pdfViewer.Document.PageCount)
         {
            _pdfViewer.Renderer.Page = param.PageNumber.Value - 1;
            //int nPage = _pdfViewer.Renderer.Page;

            //Select the line???
            //var textPage = pdfDocument.GetPdfTextPage(0);
            //var searchText = "your text";
            //int startIndex = -1;
            //int length = 0;
            //pdfViewer.Renderer.Sele .Select(startIndex, length);
         }
      }
   }
}