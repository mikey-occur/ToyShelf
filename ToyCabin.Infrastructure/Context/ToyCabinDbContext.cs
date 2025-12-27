using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Infrastructure.Context
{
	public class ToyCabinDbContext : DbContext
	{
		public ToyCabinDbContext(DbContextOptions<ToyCabinDbContext> options) : base(options) {}
		public DbSet<Account> Accounts { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<AccountRole> AccountRoles { get; set; }
		public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }
		public DbSet<Store> Stores { get; set; }
		public DbSet<Cabin> Cabins { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<ProductCategory> ProductCategories { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// ================== ACCOUNT ==================
			modelBuilder.Entity<Account>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Provider)
					  .IsRequired()
					  .HasConversion<string>() 
					  .HasMaxLength(20);

				entity.Property(e => e.PasswordHash)
					  .HasMaxLength(255);

				entity.Property(e => e.Salt)
					  .HasMaxLength(255);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				entity.Property(e => e.IsFirstLogin)
					  .IsRequired()
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.LastLoginAt);

				// ===== Relationship =====
				entity.HasOne(e => e.User)
					  .WithMany(u => u.Accounts)
					  .HasForeignKey(e => e.UserId)
					  .HasConstraintName("FK_Account_User");

				entity.HasMany(e => e.PasswordResetOtps)
					  .WithOne(a => a.Account)
					  .HasForeignKey(a => a.AccountId);

				// ===== Đảm bảo User ko bị trùng Provider ( 1 User chỉ có 1 Account / Provider ) =====
				entity.HasIndex(e => new { e.UserId, e.Provider })
					  .IsUnique();
			});

			// ================== USER ==================
			modelBuilder.Entity<User>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Email)
					  .IsRequired()
					  .HasMaxLength(255);

				// Tránh trùng email
				entity.HasIndex(e => e.Email)
					  .IsUnique();

				entity.Property(e => e.FullName)
					  .IsRequired()
					  .HasMaxLength(200);

				entity.Property(e => e.AvatarUrl)
					  .HasMaxLength(500);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt);

				// ===== Relationship =====
				entity.HasMany(e => e.Accounts)
					  .WithOne(a => a.User)
					  .HasForeignKey(a => a.UserId);
			});


			// ================== ROLE ==================
			modelBuilder.Entity<Role>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(e => e.Description)
					  .HasMaxLength(255);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt)
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");
			});

			// ================== ACCOUNT_ROLE ==================
			modelBuilder.Entity<AccountRole>(entity =>
			{
				// Composite PK
				entity.HasKey(e => new { e.AccountId, e.RoleId });

				// FK -> Account
				entity.HasOne(e => e.Account)
					  .WithMany(a => a.AccountRoles)
					  .HasForeignKey(e => e.AccountId)
					  .HasConstraintName("FK_AccountRole_Account");

				// FK -> Role
				entity.HasOne(e => e.Role)
					  .WithMany(r => r.AccountRoles)
					  .HasForeignKey(e => e.RoleId)
					  .HasConstraintName("FK_AccountRole_Role");
			});

			// ================== PASSWORD_RESET_OTP ==================
			modelBuilder.Entity<PasswordResetOtp>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.OtpCode)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.Purpose)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(50);

				entity.Property(e => e.IsUsed)
					  .HasDefaultValue(false);

				entity.Property(e => e.ExpiredAt)
					  .IsRequired();

				entity.Property(e => e.CreatedAt)
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				// FK -> Account
				entity.HasOne(e => e.Account)
					  .WithMany(a => a.PasswordResetOtps)
					  .HasForeignKey(e => e.AccountId)
					  .HasConstraintName("FK_PasswordResetOtp_Account");
			});

			// ================== STORE ==================
			modelBuilder.Entity<Store>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(200);

				entity.Property(e => e.StoreAddress)
					  .IsRequired()
					  .HasMaxLength(300);

				entity.Property(e => e.PhoneNumber)
					  .HasMaxLength(20);

				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt);

				// FK -> Cabin
				entity.HasMany(e => e.Cabins)
					  .WithOne(a => a.Store)
					  .HasForeignKey(a => a.StoreId);
			});

			// ================== Cabin ==================
			modelBuilder.Entity<Cabin>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()	
					  .HasMaxLength(50);

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(200);

				entity.Property(e => e.LocationDescription)
					  .IsRequired()
					  .HasMaxLength(300);

				entity.Property(e => e.IsOnline)
					  .HasDefaultValue(false);

				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.LastHeartbeatAt);
				// FK -> Store
				entity.HasOne(e => e.Store)
					  .WithMany(s => s.Cabins)
					  .HasForeignKey(e => e.StoreId)
					  .HasConstraintName("FK_Cabin_Store");
			});

			// ================== Product ==================
			modelBuilder.Entity<Product>(entity => 
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.SKU)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.HasIndex(e => e.SKU)
					  .IsUnique();

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(200);

				entity.Property(e => e.Price)
					  .IsRequired()
					  .HasColumnType("decimal(18,2)");

				entity.Property(e => e.Description)
					  .HasMaxLength(1000);

				// ===== MEDIA / QR =====
				entity.Property(e => e.QrCode)
					  .HasMaxLength(200);

				entity.Property(e => e.Model3DUrl)
					  .HasMaxLength(500);

				entity.Property(e => e.ImageUrl)
					  .HasMaxLength(500);

				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);

				entity.Property(e => e.IsConsignment)
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt);

				// FK -> ProductCategory
				entity.HasOne(e => e.ProductCategory)
					  .WithMany(c => c.Products)
					  .HasForeignKey(e => e.ProductCategoryId)
					  .HasConstraintName("FK_Product_ProductCategory");
			});

			// ================== ProductCategory ==================
			modelBuilder.Entity<ProductCategory>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(200);

				entity.Property(e => e.Description)
					  .HasMaxLength(500);

				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt);

				// FK -> Product
				entity.HasMany(e => e.Products)
					  .WithOne(a => a.ProductCategory)
					  .HasForeignKey(a => a.ProductCategoryId);
			});
		}
	}
}
