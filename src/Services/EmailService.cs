using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Resend;

/// <summary>
/// Servicio encargado de la gestión y envío de correos electrónicos.
/// Implementa la integración con la API de Resend para notificaciones de sistema y envío de documentos.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly IConfiguration _config;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="EmailService"/>.
    /// </summary>
    /// <param name="resend">Cliente de la API Resend.</param>
    /// <param name="config">Configuración de la aplicación para extraer credenciales y plantillas.</param>
    public EmailService(IResend resend, IConfiguration config)
    {
        _resend = resend;
        _config = config;
    }

    /// <summary>
    /// Método para restablecer contraseña (actualmente no implementado).
    /// </summary>
    /// <param name="dto">DTO con la información de restablecimiento.</param>
    /// <exception cref="NotImplementedException">Lanzada porque la lógica reside actualmente en el controlador.</exception>
    public Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Envía un correo electrónico con un código de verificación de 6 dígitos para la recuperación de cuentas.
    /// </summary>
    /// <remarks>
    /// Utiliza una plantilla HTML interna y extrae el tiempo de expiración y remitente desde el archivo de configuración.
    /// </remarks>
    /// <param name="email">Dirección de correo del destinatario.</param>
    /// <param name="code">Código numérico generado.</param>
    /// <returns>Una tarea que representa la operación de envío asíncrono.</returns>
    public async Task SendVerificationCodeAsync(string email, string code)
    {
        // Recuperación de valores desde appsettings.json
        var subject = _config["EmailConfiguracion:VerificationSubject"];
        var from = _config["EmailConfiguracion:from"];
        var expiration = _config["VerificationCode:ExpirationTimeInMinutes"];

        var message = new EmailMessage
        {
            From = from ?? "onboarding@resend.dev",
            To = email,
            Subject = subject ?? "Código de Verificación",
            HtmlBody = $@"
                <div style='font-family: sans-serif; border: 1px solid #eee; padding: 20px;'>
                    <h2>Restablecer Contraseña - ByG</h2>
                    <p>Has solicitado un cambio de contraseña. Usa el siguiente código:</p>
                    <h1 style='color: #007bff; letter-spacing: 5px;'>{code}</h1>
                    <p>Este código expirará en <strong>{expiration} minutos</strong>.</p>
                    <hr />
                    <small>Si no solicitaste este cambio, ignora este correo.</small>
                </div>"
        };

        await _resend.EmailSendAsync(message);
    }

    /// <summary>
    /// Envía un documento PDF como archivo adjunto a un destinatario específico.
    /// </summary>
    /// <remarks>
    /// Ideal para el envío masivo de solicitudes de cotización a proveedores.
    /// </remarks>
    /// <param name="email">Correo del proveedor o cliente.</param>
    /// <param name="pdfBytes">Contenido del archivo PDF en arreglo de bytes.</param>
    /// <param name="fileName">Nombre con el que aparecerá el archivo adjunto (ej: Solicitud_RFQ.pdf).</param>
    /// <returns>Una tarea que representa la operación de envío asíncrono.</returns>
    public async Task SendPdfDocumentAsync(string email, byte[] pdfBytes, string fileName)
    {
        var message = new EmailMessage
        {
            From = _config["EmailConfiguracion:from"] ?? "onboarding@resend.dev",
            To = email,
            Subject = "Tu Documento de Cotización - ByG",
            HtmlBody = "<p>Adjunto encontrarás el documento PDF solicitado.</p>",
            Attachments = new List<EmailAttachment>
            {
                new EmailAttachment
                {
                    Filename = fileName,
                    Content = pdfBytes
                }
            }
        };

        await _resend.EmailSendAsync(message);
    }
}