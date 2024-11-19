using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace restaurante.Models;

public partial class RestauranteWebContext : DbContext
{
    public RestauranteWebContext()
    {
    }

    public RestauranteWebContext(DbContextOptions<RestauranteWebContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CategoriasMenu> CategoriasMenus { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<DetallesCompra> DetallesCompras { get; set; }

    public virtual DbSet<DetallesPedido> DetallesPedidos { get; set; }

    public virtual DbSet<Inventario> Inventarios { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Mesa> Mesas { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<Receta> Recetas { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioRole> UsuarioRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-D9IF75B\\SQL2019;Database=restauranteWeb;User Id=DESKTOP-D9IF75B\\T403-22;Password='';Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoriasMenu>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__categori__CD54BC5AF43FE946");

            entity.ToTable("categorias_menu");

            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.IdCompra).HasName("PK__compras__C4BAA6045C12CC70");

            entity.ToTable("compras");

            entity.Property(e => e.IdCompra).HasColumnName("id_compra");
            entity.Property(e => e.FechaCompra)
                .HasColumnType("datetime")
                .HasColumnName("fecha_compra");
            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdProveedor)
                .HasConstraintName("FK__compras__id_prov__3F466844");
        });

        modelBuilder.Entity<DetallesCompra>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCompra).HasName("PK__detalles__BD16E2791EE70464");

            entity.ToTable("detalles_compra");

            entity.Property(e => e.IdDetalleCompra).HasColumnName("id_detalle_compra");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad");
            entity.Property(e => e.IdCompra).HasColumnName("id_compra");
            entity.Property(e => e.IdInventario).HasColumnName("id_inventario");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_unitario");

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.DetallesCompras)
                .HasForeignKey(d => d.IdCompra)
                .HasConstraintName("FK__detalles___id_co__4222D4EF");

            entity.HasOne(d => d.IdInventarioNavigation).WithMany(p => p.DetallesCompras)
                .HasForeignKey(d => d.IdInventario)
                .HasConstraintName("FK__detalles___id_in__4316F928");
        });

        modelBuilder.Entity<DetallesPedido>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PK__detalles__4F1332DED7F254EC");

            entity.ToTable("detalles_pedido");

            entity.Property(e => e.IdDetalle).HasColumnName("id_detalle");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.IdPedido).HasColumnName("id_pedido");
            entity.Property(e => e.Notas)
                .HasColumnType("text")
                .HasColumnName("notas");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("subtotal");

            entity.HasOne(d => d.IdItemNavigation).WithMany(p => p.DetallesPedidos)
                .HasForeignKey(d => d.IdItem)
                .HasConstraintName("FK__detalles___id_it__34C8D9D1");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.DetallesPedidos)
                .HasForeignKey(d => d.IdPedido)
                .HasConstraintName("FK__detalles___id_pe__33D4B598");
        });

        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.IdInventario).HasName("PK__inventar__013AEB51FAAF1370");

            entity.ToTable("inventario");

            entity.Property(e => e.IdInventario).HasColumnName("id_inventario");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.Unidad)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("unidad");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.IdItem).HasName("PK__items__87C9438B92D2C0A8");

            entity.ToTable("items");

            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Disponible)
                .HasDefaultValue(true)
                .HasColumnName("disponible");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("imagen_url");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");
            entity.Property(e => e.TiempoPreparacion).HasColumnName("tiempo_preparacion");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("FK__items__id_catego__1FCDBCEB");
        });

        modelBuilder.Entity<Mesa>(entity =>
        {
            entity.HasKey(e => e.IdMesa).HasName("PK__mesas__68A1E15956D16608");

            entity.ToTable("mesas");

            entity.HasIndex(e => e.Numero, "UQ__mesas__FC77F2116D5EEDE3").IsUnique();

            entity.Property(e => e.IdMesa).HasColumnName("id_mesa");
            entity.Property(e => e.Capacidad).HasColumnName("capacidad");
            entity.Property(e => e.Estado)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("libre")
                .HasColumnName("estado");
            entity.Property(e => e.Numero).HasColumnName("numero");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.IdPedido).HasName("PK__pedidos__6FF014894E76AEDE");

            entity.ToTable("pedidos");

            entity.Property(e => e.IdPedido).HasColumnName("id_pedido");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaHora)
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora");
            entity.Property(e => e.IdMesa).HasColumnName("id_mesa");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("metodo_pago");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.IdMesaNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdMesa)
                .HasConstraintName("FK__pedidos__id_mesa__30F848ED");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__pedidos__id_usua__300424B4");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PK__proveedo__8D3DFE2884B38617");

            entity.ToTable("proveedores");

            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.Contacto)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("contacto");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Receta>(entity =>
        {
            entity.HasKey(e => new { e.IdItem, e.IdInventario }).HasName("PK__recetas__87DAED3ED76F3926");

            entity.ToTable("recetas");

            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.IdInventario).HasColumnName("id_inventario");
            entity.Property(e => e.CantidadRequerida)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad_requerida");

            entity.HasOne(d => d.IdInventarioNavigation).WithMany(p => p.Receta)
                .HasForeignKey(d => d.IdInventario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__recetas__id_inve__3A81B327");

            entity.HasOne(d => d.IdItemNavigation).WithMany(p => p.Receta)
                .HasForeignKey(d => d.IdItem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__recetas__id_item__398D8EEE");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__reservas__423CBE5DDEADB9CF");

            entity.ToTable("reservas");

            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.Estado)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaHora)
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora");
            entity.Property(e => e.IdMesa).HasColumnName("id_mesa");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NumeroPersonas).HasColumnName("numero_personas");

            entity.HasOne(d => d.IdMesaNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdMesa)
                .HasConstraintName("FK__reservas__id_mes__2A4B4B5E");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__reservas__id_usu__29572725");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__roles__6ABCB5E08B93A6A0");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Nombre, "UQ__roles__72AFBCC69F0CEFF7").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__usuarios__4E3E04AD8B8C77E7");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Email, "UQ__usuarios__AB6E61649CD8A467").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("contrasena");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.UltimoAcceso)
                .HasColumnType("datetime")
                .HasColumnName("ultimo_acceso");
        });

        modelBuilder.Entity<UsuarioRole>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioRol).HasName("PK__usuario___D1F881FE3B3F46B9");

            entity.ToTable("usuario_roles");

            entity.HasIndex(e => new { e.IdUsuario, e.IdRol }, "UQ_usuario_rol").IsUnique();

            entity.Property(e => e.IdUsuarioRol).HasColumnName("id_usuario_rol");
            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_asignacion");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.UsuarioRoles)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__usuario_r__id_ro__1A14E395");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.UsuarioRoles)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__usuario_r__id_us__1920BF5C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
