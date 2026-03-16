using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para el servicio de mensajería.
    /// Encapsula el archivo binario y la información necesaria para el envío de documentos PDF por correo electrónico.
    /// </summary>
    public record SendPdfDto
    {
        /// <summary>
        /// Dirección de correo electrónico del destinatario (ej: el proveedor o el área de finanzas).
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Representación binaria del documento PDF generado en memoria.
        /// Este arreglo de bytes se adjunta directamente a la trama del correo.
        /// </summary>
        public byte[] PdfBytes { get; set; } = null!;

        /// <summary>
        /// Nombre que tendrá el archivo adjunto (ej: "OC-2026-001.pdf").
        /// Es fundamental para que el receptor identifique el documento correctamente.
        /// </summary>
        public string FileName { get; set; } = null!;
    }
}