using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Models
{
    /// <summary>
    /// Representa un token de seguridad temporal para el restablecimiento de contraseñas.
    /// Almacena códigos de verificación de un solo uso vinculados al correo electrónico de un usuario.
    /// </summary>
    public class PasswordResetToken
    {
        /// <summary>
        /// Identificador único del registro de token.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Correo electrónico del usuario que solicitó la recuperación de cuenta.
        /// Se utiliza para validar la propiedad del código durante el proceso de Reset.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Código de verificación generado (típicamente de 6 dígitos).
        /// Es el valor que el usuario debe ingresar manualmente desde su correo.
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// Fecha y hora en la que el código dejará de ser válido.
        /// Generalmente configurado para expirar pocos minutos después de su creación (ej: 3-10 min).
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Indica si el código ya ha sido procesado exitosamente. 
        /// Previene ataques de reutilización de tokens (Replay Attacks).
        /// </summary>
        public bool IsUsed { get; set; } = false;
    }
}