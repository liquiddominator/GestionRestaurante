using System;
using System.Collections.Generic;

namespace restaurante.Models;

public partial class Inventario
{
    public int IdInventario { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public string Unidad { get; set; } = null!;

    public decimal PrecioUnitario { get; set; }

    public virtual ICollection<DetallesCompra> DetallesCompras { get; set; } = new List<DetallesCompra>();

    public virtual ICollection<Receta> Receta { get; set; } = new List<Receta>();
}
