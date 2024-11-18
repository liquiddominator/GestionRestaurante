using System;
using System.Collections.Generic;

namespace restaurante.Models;

public partial class Receta
{
    public int IdItem { get; set; }

    public int IdInventario { get; set; }

    public decimal CantidadRequerida { get; set; }

    public virtual Inventario IdInventarioNavigation { get; set; } = null!;

    public virtual Item IdItemNavigation { get; set; } = null!;
}
