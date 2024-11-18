using System;
using System.Collections.Generic;

namespace restaurante.Models;

public partial class DetallesPedido
{
    public int IdDetalle { get; set; }

    public int? IdPedido { get; set; }

    public int? IdItem { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public string? Notas { get; set; }

    public virtual Item? IdItemNavigation { get; set; }

    public virtual Pedido? IdPedidoNavigation { get; set; }
}
