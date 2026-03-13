using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using Microsoft.AspNetCore.Identity;

namespace ByG_Backend.src.Interfaces
{
    /// <summary>
    /// Define los servicios de mensajería y comunicación por correo electrónico del sistema.
    /// Proporciona métodos para la seguridad de identidad y el envío de documentos adjuntos.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Procesa la solicitud de restablecimiento de contraseña.
        /// </summary>
        /// <param name="dto">Objeto con las credenciales y el código de verificación necesario.</param>
        /// <returns>Resultado de la operación de Identity.</returns>
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto);

        /// <summary>
        /// Envía un código de verificación (OTP) al correo del usuario para procesos de recuperación de cuenta.
        /// </summary>
        /// <param name="email">Dirección de correo destino.</param>
        /// <param name="code">Código alfanumérico o numérico generado.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        Task SendVerificationCodeAsync(string email, string code);

        /// <summary>
        /// Envía un archivo PDF adjunto a una dirección de correo específica.
        /// Utilizado principalmente para enviar solicitudes de cotización (RFQ) o facturas.
        /// </summary>
        /// <param name="email">Dirección de correo del destinatario.</param>
        /// <param name="pdfBytes">Contenido del archivo en formato binario.</param>
        /// <param name="fileName">Nombre del archivo con su extensión (ej: Solicitud.pdf).</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        Task SendPdfDocumentAsync(string email, byte[] pdfBytes, string fileName);

        Task SendPdfPurchaseOrderAsync(string email, byte[] pdfBytes, string fileName);
    }
}