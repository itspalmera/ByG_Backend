using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByG_Backend.src.Models
{
    public class PasswordResetToken
    {

        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public string Code { get; set; } = null!; // El código de 6 dígitos


        public DateTime ExpiryDate { get; set; } // Fecha de creación + 3 minutos

        public bool IsUsed { get; set; } = false;
    }
}