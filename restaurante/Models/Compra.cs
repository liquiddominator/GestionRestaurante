using System;
using System.Collections.Generic;

namespace restaurante.Models;

public partial class Compra
{
    public int IdCompra { get; set; }

    public int? IdProveedor { get; set; }

    public DateTime FechaCompra { get; set; }

    public decimal Total { get; set; }

    public virtual ICollection<DetallesCompra> DetallesCompras { get; set; } = new List<DetallesCompra>();

    public virtual Proveedore? IdProveedorNavigation { get; set; }
}
