using System;
using System.Collections.Generic;

namespace restaurante.Models;

public partial class Compra
{
    public int IdCompra { get; set; }

    public int? IdUsuario { get; set; }

    public DateTime FechaCompra { get; set; }

    public decimal Total { get; set; }

    public virtual ICollection<DetallesCompra> DetallesCompras { get; set; } = new List<DetallesCompra>();

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
