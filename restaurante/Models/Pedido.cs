using System;
using System.Collections.Generic;

namespace restaurante.Models;

public partial class Pedido
{
    public int IdPedido { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdMesa { get; set; }

    public DateTime FechaHora { get; set; }

    public string Estado { get; set; } = null!;

    public decimal Total { get; set; }

    public string? MetodoPago { get; set; }

    public virtual ICollection<DetallesPedido> DetallesPedidos { get; set; } = new List<DetallesPedido>();

    public virtual Mesa? IdMesaNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
