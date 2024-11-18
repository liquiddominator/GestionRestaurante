using System;
using System.Collections.Generic;

namespace restaurante.Models;

public partial class DetallesCompra
{
    public int IdDetalleCompra { get; set; }

    public int? IdCompra { get; set; }

    public int? IdInventario { get; set; }

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public virtual Compra? IdCompraNavigation { get; set; }

    public virtual Inventario? IdInventarioNavigation { get; set; }
}
