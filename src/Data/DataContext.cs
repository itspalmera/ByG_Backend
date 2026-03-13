using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Models;
using ByG_Backend.src.Models.MaterialRequest;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ByG_Backend.src.Data
{
    /// <summary>
    /// Contexto de datos principal de la aplicación.
    /// Hereda de IdentityDbContext para integrar la gestión de usuarios, roles y autenticación de ASP.NET Core Identity.
    /// Configura las tablas y relaciones para el sistema de compras, cotizaciones y proveedores.
    /// </summary>
    /// <param name="dbContextOptions">Opciones de configuración del contexto (Cadena de conexión, motor de DB, etc.).</param>
    public class DataContext(DbContextOptions<DataContext> dbContextOptions) : IdentityDbContext<User>(dbContextOptions)
    {
        /// <summary>
        /// Tabla de Cotizaciones recibidas de proveedores.
        /// </summary>
        public DbSet<Quote> Quotes { get; set; } = null!;

        /// <summary>
        /// Tabla con el detalle de artículos incluidos en cada cotización.
        /// </summary>
        public DbSet<QuoteItem> QuoteItems { get; set; } = null!;

        /// <summary>
        /// Tabla de Solicitudes de Cotización (RFQ) generadas por la empresa.
        /// </summary>
        public DbSet<RequestQuote> RequestQuotes { get; set; } = null!;

        /// <summary>
        /// Tabla intermedia que gestiona la relación de muchos a muchos entre Solicitudes y Proveedores.
        /// </summary>
        public DbSet<RequestQuoteSupplier> RequestQuoteSuppliers { get; set; } = null!;

        /// <summary>
        /// Tabla principal de Solicitudes de Compra (Requerimientos internos).
        /// </summary>
        public DbSet<Purchase> Purchase { get; set; } = null!;

        /// <summary>
        /// Detalle de productos o servicios solicitados en una compra.
        /// </summary>
        public DbSet<PurchaseItem> PurchaseItem { get; set; } = null!;

        /// <summary>
        /// Tabla de Órdenes de Compra formales emitidas tras la aprobación de una cotización.
        /// </summary>
        public DbSet<PurchaseOrder> PurchaseOrder { get; set; } = null!;

        /// <summary>
        /// Maestro de Proveedores registrados en el sistema.
        /// </summary>
        public DbSet<Supplier> Supplier { get; set; } = null!;
        
        /// <summary>
        /// Almacén temporal de tokens y códigos para la recuperación de contraseñas.
        /// </summary>
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

        /// <summary>
        /// Tabla de Usuarios (extensión de IdentityUser).
        /// </summary>
        public DbSet<User> User { get; set; } = null!;

        // Tablas externas (Lectura compartida)
        public DbSet<Solicitud> Solicitudes { get; set; } = null!;
        public DbSet<DetalleSolicitud> DetalleSolicitudes { get; set; } = null!;
        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<Bodega> Bodegas { get; set; } = null!;

        //Carga identity
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
        }
    }
}