using ChatAppModels;
using Microsoft.EntityFrameworkCore;

namespace ChatBackend.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
            Database.EnsureCreated();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Dialog> Dialogs { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserDialog> UsersDialogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ChatNew1;Trusted_connection=true")
                .LogTo(message =>
                {
                    System.Console.WriteLine(message);
                });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(build =>
            {
                build.HasKey(e => e.Id);

                build.Property(e => e.Login)
                    .IsRequired()
                    .HasMaxLength(150);

                build.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                build.HasIndex(e => e.Email)
                    .IsUnique();

                build.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(150);

                build.Property(e => e.RoleId)
                  .HasDefaultValue(2);

                build.HasData(new User[]
                {
                    new User
                    {
                        Id = 1,
                        Email = "ulyanovskiy.01@mail.ru",
                        Password = "c42Yg2ACjJuaWVpcCUDIOVO07ZL/L/dqvzhzqJxYRBc=",
                        Login = "Vlad",
                        RoleId = 1
                    },
                    new User
                    {
                        Id = 2,
                        Email = "ulyanovskiy.2001@mail.ru",
                        Password = "c42Yg2ACjJuaWVpcCUDIOVO07ZL/L/dqvzhzqJxYRBc=",
                        Login = "fefrues",
                        RoleId = 1
                    }
                });
            });

            modelBuilder.Entity<Role>(build =>
            {
                build.HasKey(e => e.Id);

                build.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                build.HasData(new Role[]
                {
                    new Role { Id = 1, Name = "Admin" },
                    new Role { Id = 2, Name = "User" }
                });
            });

            modelBuilder.Entity<Friend>(build =>
            {
                build.HasKey(e => e.Id);

                build.HasIndex(e => new { e.UserId, e.ToUserId })
                    .IsUnique();

                build.Property(e => e.IsConfirmed)
                    .IsRequired()
                    .HasDefaultValue(false);

                build.HasOne(e => e.User)
                    .WithMany();

                build.HasOne(e => e.ToUser)
                    .WithMany()
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Dialog>(build =>
            {
                build.HasKey(e => e.Id);

                build.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                build.HasOne(e => e.User)
                    .WithMany()
                    .OnDelete(DeleteBehavior.NoAction);

                build.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<Message>(build =>
            {
                build.HasKey(e => e.Id);

                build.Property(e => e.Content)
                    .IsRequired();

                build.Property(e => e.DateCreate)
                    .IsRequired();

                build.Property(e => e.IsRead)
                    .IsRequired()
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<UserDialog>(build =>
            {
                build.HasKey(e => e.Id);

                build.HasOne(e => e.User)
                    .WithMany();

                build.HasOne(e => e.Dialog)
                    .WithMany()
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}