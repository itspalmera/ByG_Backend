using System;
using System.Collections.Generic;
using System.Linq;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Resend;

public class EmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly IConfiguration _config;

    public EmailService(IResend resend, IConfiguration config)
    {
        _resend = resend;
        _config = config;
    }

    public Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task SendVerificationCodeAsync(string email, string code)
    {
        // Leemos la configuración de tu appsettings
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

        public async Task SendPdfPurchaseOrderAsync(string email, byte[] pdfBytes, string fileName)
    {
        var message = new EmailMessage
        {
            From = _config["EmailConfiguracion:from"] ?? "onboarding@resend.dev",
            To = email,
            Subject = "Orden de Compra N° {purchaseOrderNumber} - ByG Ingenieria",
            HtmlBody = "<p>Se envía el documento de la orden de compra número {purchaseOrderNumber}.</p>",
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