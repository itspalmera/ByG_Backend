using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ByG_Backend.src.Data;
using ByG_Backend.src.DTOs;
using ByG_Backend.src.Helpers;
using ByG_Backend.src.Mappers;
using ByG_Backend.src.Models;
using ByG_Backend.src.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace ByG_Backend.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController(DataContext context) : ControllerBase
    {
        private readonly DataContext _context = context;

        // CREAR COMPRA
        // POST CreatePurchase
        
        // OBTENER COMPRAS
        // GET GetPurchases

        // OBTENER COMPRA POR ID
        // GET GetPurchaseById

        // ACTUALIZAR ESTADO DE COMPRA
        // UpdateStatusPurchase

    }
}