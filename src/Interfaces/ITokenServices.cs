using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Interfaces
{
    public interface ITokenServices
    {
        string GenerateToken(User user, string role);
    }
}