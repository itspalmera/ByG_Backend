using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Representa a un proveedor dentro del sistema.
    /// Almacena información legal, de contacto y operativa necesaria para el proceso de adquisiciones.
    /// </summary>
    public class Supplier
    {
        /// <summary>
        /// Identificador único autoincremental del proveedor.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Rol Único Tributario (RUT) que identifica legalmente al proveedor en Chile.
        /// Se utiliza como identificador de negocio único.
        /// </summary>
        public string Rut { get; set; } = null!;

        /// <summary>
        /// Razón Social o nombre legal de la empresa proveedora.
        /// </summary>
        public string BusinessName { get; set; } = null!;

        /// <summary>
        /// Nombre de la persona de contacto principal en la empresa proveedora.
        /// </summary>
        public string? ContactName { get; set; }

        /// <summary>
        /// Correo electrónico institucional para el envío de Solicitudes de Cotización (RFQ).
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Número telefónico de contacto.
        /// </summary>
        public string? Phone { get; set; } 

        /// <summary>
        /// Dirección física de la oficina o bodega del proveedor.
        /// </summary>
        public string? Address { get; set; } 

        /// <summary>
        /// Ciudad o comuna de residencia del proveedor.
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Etiquetas o categorías de productos/servicios que ofrece (ej: "EPP, Herramientas, Construcción").
        /// Útil para el filtrado al momento de invitar proveedores a cotizar.
        /// </summary>
        public string? ProductCategories { get; set; } 
        
        /// <summary>
        /// Fecha y hora en la que el proveedor fue dado de alta en el sistema.
        /// </summary>
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Estado lógico del proveedor. Permite inhabilitar proveedores sin eliminar sus registros históricos.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // =========================
        // RELACIONES
        // =========================

        /// <summary>
        /// Relación con las invitaciones a cotizar recibidas (Muchos a Muchos mediante tabla intermedia).
        /// </summary>
        public List<RequestQuoteSupplier> RequestQuoteSuppliers { get; set; } = new();
        
        /// <summary>
        /// Lista de cotizaciones formales que este proveedor ha emitido en respuesta a solicitudes.
        /// </summary>
        public List<Quote> Quotes { get; set; } = new();
    }
}