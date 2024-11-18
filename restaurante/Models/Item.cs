using System;
using System.Collections.Generic;

namespace restaurante.Models;

public partial class Item
{
    public int IdItem { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int? IdCategoria { get; set; }

    public bool? Disponible { get; set; }

    public string? ImagenUrl { get; set; }

    public int? TiempoPreparacion { get; set; }

    public virtual ICollection<DetallesPedido> DetallesPedidos { get; set; } = new List<DetallesPedido>();

    public virtual CategoriasMenu? IdCategoriaNavigation { get; set; }

    public virtual ICollection<Receta> Receta { get; set; } = new List<Receta>();
}
