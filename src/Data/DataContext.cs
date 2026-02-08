using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByG_Backend.src.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace ByG_Backend.src.Data
{
    public class DataContext(DbContextOptions<DataContext> dbContextOptions) : IdentityDbContext<User>(dbContextOptions)
    {
        public DbSet<Quote> Quotes { get; set; } = null!;
        public DbSet<QuoteItem> QuoteItems { get; set; } = null!;
        public DbSet<RequestQuote> RequestQuotes { get; set; } = null!;
        public DbSet<RequestQuoteSupplier> RequestQuoteSuppliers { get; set; } = null!;

        //Carga identity
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}