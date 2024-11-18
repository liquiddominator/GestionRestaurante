using System;
using System.Collections.Generic;

namespace restaurante.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdMesa { get; set; }

    public DateTime FechaHora { get; set; }

    public int NumeroPersonas { get; set; }

    public string? Estado { get; set; }

    public virtual Mesa? IdMesaNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
