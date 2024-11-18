using System;
using System.Collections.Generic;

namespace restaurante.Models;

public partial class CategoriasMenu
{
    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
