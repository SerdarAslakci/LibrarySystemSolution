using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LibrarySystem.API.DataContext
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {


        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasIndex(u => u.NormalizedUserName)
                .IsUnique(false);

            builder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            builder.Entity<Book>()
                .HasIndex(b => b.ISBN)
                .IsUnique();

            builder.Entity<BookCopy>()
                .HasIndex(bc => bc.BarcodeNumber)
                .IsUnique();

            builder.Entity<Loan>()
                .HasOne(l => l.AppUser)
                .WithMany()
                .HasForeignKey(l => l.UserId);

            builder.Entity<Fine>()
                .HasOne(f => f.Loan)
                .WithMany()
                .HasForeignKey(f => f.LoanId)
                .IsRequired(false);
        }


        // Konum Yönetimi
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Shelf> Shelves { get; set; }

        // Kitap Yönetimi
        public DbSet<Author> Authors { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }

        // Çoktan Çoğa İlişkisi İçin Ara Tablo
        public DbSet<BookAuthor> BookAuthors { get; set; }

        // İşlem ve Ceza Yönetimi
        public DbSet<Loan> Loans { get; set; }
        public DbSet<FineType> FineTypes { get; set; }
        public DbSet<Fine> Fines { get; set; }
    }
}
