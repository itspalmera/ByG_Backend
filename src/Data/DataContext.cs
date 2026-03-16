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
    /// Configura las tablas y relaciones para el sistema de compras, cotizaciones y proveedores de ByG Ingeniería.
    /// </summary>
    /// <param name="dbContextOptions">Opciones de configuración del contexto (Cadena de conexión, motor de DB, etc.).</param>
    public class DataContext(DbContextOptions<DataContext> dbContextOptions) : IdentityDbContext<User>(dbContextOptions)
    {
        #region Módulo de Cotizaciones y Licitaciones
        /// <summary>
        /// Tabla de Cotizaciones recibidas de proveedores en respuesta a una licitación.
        /// </summary>
        public DbSet<Quote> Quotes { get; set; } = null!;

        /// <summary>
        /// Tabla con el detalle de artículos, precios y cantidades incluidos en cada cotización.
        /// </summary>
        public DbSet<QuoteItem> QuoteItems { get; set; } = null!;

        /// <summary>
        /// Tabla de Solicitudes de Cotización (RFQ) generadas para invitar a proveedores a ofertar.
        /// </summary>
        public DbSet<RequestQuote> RequestQuotes { get; set; } = null!;

        /// <summary>
        /// Tabla intermedia (Join Table) que gestiona la relación muchos-a-muchos entre Solicitudes de Cotización y Proveedores.
        /// </summary>
        public DbSet<RequestQuoteSupplier> RequestQuoteSuppliers { get; set; } = null!;
        #endregion

        #region Módulo de Compras y Órdenes
        /// <summary>
        /// Tabla principal de Requerimientos de Compra (solicitudes internas de materiales).
        /// </summary>
        public DbSet<Purchase> Purchase { get; set; } = null!;

        /// <summary>
        /// Detalle de productos o servicios solicitados dentro de un requerimiento de compra.
        /// </summary>
        public DbSet<PurchaseItem> PurchaseItem { get; set; } = null!;

        /// <summary>
        /// Tabla de Órdenes de Compra (OC) formales emitidas tras adjudicar una cotización.
        /// </summary>
        public DbSet<PurchaseOrder> PurchaseOrder { get; set; } = null!;
        #endregion

        #region Entidades Maestras y Seguridad
        /// <summary>
        /// Maestro de Proveedores registrados y habilitados en el sistema.
        /// </summary>
        public DbSet<Supplier> Supplier { get; set; } = null!;
        
        /// <summary>
        /// Gestión de tokens temporales para el flujo de recuperación de acceso de usuarios.
        /// </summary>
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

        /// <summary>
        /// Tabla de Usuarios del sistema, extendiendo la funcionalidad base de ASP.NET Core Identity.
        /// </summary>
        public DbSet<User> User { get; set; } = null!;
        #endregion

        #region Integración con Sistemas Externos (Solo Lectura)
        /// <summary>
        /// Acceso a las solicitudes provenientes del sistema de gestión de materiales externo.
        /// </summary>
        public DbSet<Solicitud> Solicitudes { get; set; } = null!;
        public DbSet<DetalleSolicitud> DetalleSolicitudes { get; set; } = null!;
        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<Bodega> Bodegas { get; set; } = null!;
        #endregion

        /// <summary>
        /// Configura el mapeo de las entidades y las restricciones de la base de datos mediante Fluent API.
        /// </summary>
        /// <param name="builder">Constructor de modelos para la base de datos.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Carga la configuración base de Identity (AspNetUsers, AspNetRoles, etc.)
            base.OnModelCreating(builder);
            
        }
    }
}