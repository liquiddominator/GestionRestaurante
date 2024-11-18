using System;
using System.Collections.Generic;

namespace restaurante.Models;

public partial class UsuarioRole
{
    public int IdUsuarioRol { get; set; }

    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public virtual Role IdRolNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
