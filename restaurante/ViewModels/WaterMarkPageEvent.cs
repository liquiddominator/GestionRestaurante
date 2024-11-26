using iTextSharp.text.pdf;
using iTextSharp.text;

namespace restaurante.ViewModels
{
    public class WatermarkPageEvent : PdfPageEventHelper
    {
        private readonly string _logoPath;
        private Image _watermark;
        private PdfGState _gState;

        public WatermarkPageEvent(string logoPath)
        {
            _logoPath = logoPath;
        }

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                if (System.IO.File.Exists(_logoPath))
                {
                    _watermark = Image.GetInstance(_logoPath);

                    // Calcular el tamaño para que ocupe gran parte de la página
                    float width = document.PageSize.Width * 0.8f;
                    float height = document.PageSize.Height * 0.35f;

                    _watermark.ScaleAbsolute(width, height);

                    // Centrar la imagen
                    _watermark.SetAbsolutePosition(
                        (document.PageSize.Width - width) / 2,
                        (document.PageSize.Height - height) / 2
                    );

                    // Configurar la transparencia
                    _gState = new PdfGState
                    {
                        FillOpacity = 0.1f,
                        StrokeOpacity = 0.1f
                    };
                }
            }
            catch (Exception)
            {
                _watermark = null;
            }
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            if (_watermark != null)
            {
                PdfContentByte under = writer.DirectContentUnder;
                under.SaveState();
                under.SetGState(_gState);
                under.AddImage(_watermark);
                under.RestoreState();
            }
        }
    }
}
