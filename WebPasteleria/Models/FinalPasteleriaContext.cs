using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebPasteleria.Models;

public partial class FinalPasteleriaContext : DbContext
{
    public FinalPasteleriaContext()
    {
    }

    public FinalPasteleriaContext(DbContextOptions<FinalPasteleriaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Carrito> Carrito { get; set; }

    public virtual DbSet<CarritoDetalle> CarritoDetalle { get; set; }

    public virtual DbSet<CategoriaProducto> CategoriaProducto { get; set; }

    public virtual DbSet<Cliente> Cliente { get; set; }

    public virtual DbSet<DetallePedido> DetallePedido { get; set; }

    public virtual DbSet<Empleado> Empleado { get; set; }

    public virtual DbSet<MetodoPago> MetodoPago { get; set; }

    public virtual DbSet<Pedido> Pedido { get; set; }

    public virtual DbSet<Producto> Producto { get; set; }

    public virtual DbSet<SeguimientoPedido> SeguimientoPedido { get; set; }

    public virtual DbSet<TokenSeguimiento> TokenSeguimiento { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-2T7BHRV\\SQLEXPRESS;Database=FinalPasteleria;User ID=usrpasteleria;Password=123456;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Carrito>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Carrito__3213E83F1CAE3E63");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.UsuarioRegistro)
                .HasMaxLength(50)
                .HasDefaultValueSql("(suser_name())")
                .HasColumnName("usuarioRegistro");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Carrito)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK_Carrito_Cliente");
        });

        modelBuilder.Entity<CarritoDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CarritoD__3213E83FABD48715");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.IdCarrito).HasColumnName("idCarrito");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("precioUnitario");
            entity.Property(e => e.SubTotal)
                .HasComputedColumnSql("([cantidad]*[precioUnitario])", true)
                .HasColumnType("decimal(23, 2)")
                .HasColumnName("subTotal");

            entity.HasOne(d => d.IdCarritoNavigation).WithMany(p => p.CarritoDetalle)
                .HasForeignKey(d => d.IdCarrito)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarritoDetalle_Carrito");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.CarritoDetalle)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarritoDetalle_Producto");
        });

        modelBuilder.Entity<CategoriaProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3213E83F46F5B2D4");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Estado)
                .HasDefaultValue((short)1)
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cliente__3213E83FA45EE676");

            entity.HasIndex(e => e.CedulaIdentidad, "UQ__Cliente__9FE1EA245E2A87B4").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .HasColumnName("apellido");
            entity.Property(e => e.CedulaIdentidad)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("cedulaIdentidad");
            entity.Property(e => e.Celular)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("celular");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.EsInvitado)
                .HasDefaultValue(false)
                .HasColumnName("esInvitado");
            entity.Property(e => e.Estado)
                .HasDefaultValue((byte)1)
                .HasColumnName("estado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.Preferencias)
                .HasMaxLength(500)
                .HasColumnName("preferencias");
            entity.Property(e => e.TipoCliente)
                .HasMaxLength(50)
                .HasColumnName("tipoCliente");
            entity.Property(e => e.UsuarioRegistro)
                .HasMaxLength(50)
                .HasDefaultValueSql("(suser_name())")
                .HasColumnName("usuarioRegistro");
        });

        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DetalleP__3213E83F5DC5BFE9");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.Descuento)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("descuento");
            entity.Property(e => e.Estado)
                .HasDefaultValue((byte)1)
                .HasColumnName("estado");
            entity.Property(e => e.EstadoDetalle)
                .HasDefaultValue((byte)1)
                .HasColumnName("estadoDetalle");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.IdPedido).HasColumnName("idPedido");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("precioUnitario");
            entity.Property(e => e.SubTotal)
                .HasComputedColumnSql("([cantidad]*[precioUnitario]-[descuento])", true)
                .HasColumnType("decimal(24, 2)")
                .HasColumnName("subTotal");
            entity.Property(e => e.UsuarioRegistro)
                .HasMaxLength(50)
                .HasDefaultValueSql("(suser_name())")
                .HasColumnName("usuarioRegistro");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.DetallePedido)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePedido_Pedido");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetallePedido)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePedido_Producto");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Empleado__3213E83FAEC29352");

            entity.HasIndex(e => e.CedulaIdentidad, "UQ__Empleado__9FE1EA2437D39505").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApellidoMaterno)
                .HasMaxLength(50)
                .HasColumnName("apellidoMaterno");
            entity.Property(e => e.ApellidoPaterno)
                .HasMaxLength(50)
                .HasColumnName("apellidoPaterno");
            entity.Property(e => e.Cargo)
                .HasMaxLength(50)
                .HasColumnName("cargo");
            entity.Property(e => e.CedulaIdentidad)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("cedulaIdentidad");
            entity.Property(e => e.Celular)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("celular");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasDefaultValue((byte)1)
                .HasColumnName("estado");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fechaNacimiento");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.UsuarioRegistro)
                .HasMaxLength(50)
                .HasDefaultValueSql("(suser_name())")
                .HasColumnName("usuarioRegistro");
        });

        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MetodoPa__3213E83F80CE789A");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pedido__3213E83F08E73212");

            entity.HasIndex(e => e.CodigoSeguimiento, "UQ__Pedido__7791A65D86D60B2F").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CelularCliente)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("celularCliente");
            entity.Property(e => e.CodigoSeguimiento)
                .HasMaxLength(50)
                .HasColumnName("codigoSeguimiento");
            entity.Property(e => e.EmailCliente)
                .HasMaxLength(100)
                .HasColumnName("emailCliente");
            entity.Property(e => e.Estado)
                .HasDefaultValue((byte)1)
                .HasColumnName("estado");
            entity.Property(e => e.EstadoPedido)
                .HasDefaultValue((byte)0)
                .HasColumnName("estadoPedido");
            entity.Property(e => e.FechaEntrega)
                .HasColumnType("datetime")
                .HasColumnName("fechaEntrega");
            entity.Property(e => e.FechaPedido)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaPedido");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.MetodoPago).HasColumnName("metodoPago");
            entity.Property(e => e.NombreCliente)
                .HasMaxLength(100)
                .HasColumnName("nombreCliente");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .HasColumnName("observaciones");
            entity.Property(e => e.TipoEntrega)
                .HasMaxLength(50)
                .HasColumnName("tipoEntrega");
            entity.Property(e => e.Total)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total");
            entity.Property(e => e.UsuarioRegistro)
                .HasMaxLength(50)
                .HasDefaultValueSql("(suser_name())")
                .HasColumnName("usuarioRegistro");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Pedido)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK_Pedido_Cliente");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Pedido)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedido_Usuario");

            entity.HasOne(d => d.MetodoPagoNavigation).WithMany(p => p.Pedido)
                .HasForeignKey(d => d.MetodoPago)
                .HasConstraintName("FK_Pedido_MetodoPago");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3213E83F23A4AEA9");

            entity.HasIndex(e => e.Codigo, "UQ__Producto__40F9A20660633D61").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasDefaultValue((byte)1)
                .HasColumnName("estado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(500)
                .HasColumnName("imagenUrl");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("precio");
            entity.Property(e => e.Stock)
                .HasDefaultValue(0)
                .HasColumnName("stock");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .HasColumnName("tipo");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20)
                .HasColumnName("unidadMedida");
            entity.Property(e => e.UsuarioRegistro)
                .HasMaxLength(50)
                .HasDefaultValueSql("(suser_name())")
                .HasColumnName("usuarioRegistro");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Producto)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Producto_Categoria");
        });

        modelBuilder.Entity<SeguimientoPedido>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Seguimie__3213E83FA48F32E6");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.EstadoPedido).HasColumnName("estadoPedido");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdPedido).HasColumnName("idPedido");
            entity.Property(e => e.NotasInternas)
                .HasMaxLength(500)
                .HasColumnName("notasInternas");
            entity.Property(e => e.UsuarioRegistro)
                .HasMaxLength(50)
                .HasDefaultValueSql("(suser_name())")
                .HasColumnName("usuarioRegistro");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.SeguimientoPedido)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeguimientoPedido_Pedido");
        });

        modelBuilder.Entity<TokenSeguimiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TokenSeg__3213E83F28836090");

            entity.HasIndex(e => e.Token, "UQ__TokenSeg__CA90DA7A41BA2D4E").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FechaExpiracion)
                .HasColumnType("datetime")
                .HasColumnName("fechaExpiracion");
            entity.Property(e => e.IdPedido).HasColumnName("idPedido");
            entity.Property(e => e.Token)
                .HasMaxLength(100)
                .HasColumnName("token");
            entity.Property(e => e.Usado)
                .HasDefaultValue(false)
                .HasColumnName("usado");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.TokenSeguimiento)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Token_Pedido");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3213E83F76149B61");

            entity.HasIndex(e => e.Usuario1, "UQ__Usuario__9AFF8FC64919CA0D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Clave)
                .HasMaxLength(255)
                .HasColumnName("clave");
            entity.Property(e => e.Estado)
                .HasDefaultValue((byte)1)
                .HasColumnName("estado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.IdEmpleado).HasColumnName("idEmpleado");
            entity.Property(e => e.IntentosFallidos)
                .HasDefaultValue(0)
                .HasColumnName("intentosFallidos");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.UltimoLogin)
                .HasColumnType("datetime")
                .HasColumnName("ultimoLogin");
            entity.Property(e => e.Usuario1)
                .HasMaxLength(50)
                .HasColumnName("usuario");
            entity.Property(e => e.UsuarioRegistro)
                .HasMaxLength(50)
                .HasDefaultValueSql("(suser_name())")
                .HasColumnName("usuarioRegistro");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Usuario)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Empleado");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
