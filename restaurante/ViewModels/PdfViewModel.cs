using restaurante.Models;

namespace restaurante.ViewModels
{
    // Models/PdfViewModel.cs
    public class PdfViewModel
    {
        public Pedido Pedido { get; set; }
        public string LogoPath { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
    }
}
