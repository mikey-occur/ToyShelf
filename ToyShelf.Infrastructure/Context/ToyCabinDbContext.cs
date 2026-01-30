using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Infrastructure.Context
{
	public class ToyCabinDbContext : DbContext
	{
		public ToyCabinDbContext(DbContextOptions<ToyCabinDbContext> options) : base(options) {}
		public DbSet<Account> Accounts { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<AccountRole> AccountRoles { get; set; }
		public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }

		public DbSet<Partner> Partners { get; set; }
		public DbSet<UserStore> UserStores { get; set; }
			
		public DbSet<Store> Stores { get; set; }
		public DbSet<StoreInvitation> StoreInvitations { get; set; }
		public DbSet<Warehouse> Warehouses { get; set; }

		public DbSet<Cabin> Cabins { get; set; }
		public DbSet<CabinSnapshot> CabinSnapshots { get; set; }
		public DbSet<Shelf> Shelves { get; set; }
		public DbSet<ShelfSlot> ShelfSlots { get; set; }

		public DbSet<Product> Products { get; set; }
		public DbSet<ProductCategory> ProductCategories { get; set; }
		public DbSet<ProductColor> ProductColors { get; set; }


		public DbSet<Shipment> Shipments { get; set; }
		public DbSet<ShipmentItem> ShipmentItems { get; set; }
		public DbSet<ShipmentMedia> ShipmentMedias { get; set; } // Bỏ s

		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }	

		public DbSet<Inventory> Inventories { get; set; }
		public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
		public DbSet<InventoryDisposition> InventoryDispositions { get; set; }
		public DbSet<InventoryLocation> InventoryLocations { get; set; }

		public DbSet<DamageReport> DamageReports { get; set; }
		public DbSet<DamageMedia> DamageMedia { get; set; }

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

				entity.HasMany(e => e.StoreInvitations)
					  .WithOne(s => s.User)
					  .HasForeignKey(s => s.UserId);

				entity.HasOne(e => e.Partner)
					  .WithMany(a => a.Users)
					  .HasForeignKey(e => e.PartnerId)
					  .HasConstraintName("FK_User_Partner");

				entity.HasMany(e => e.CabinSnapshots)
					  .WithOne(a => a.User)
					  .HasForeignKey(a => a.UserId);

				// Damage

				entity.HasMany(e => e.ReportedDamageReports)
					  .WithOne(a => a.ReportedByUser)
					  .HasForeignKey(a => a.ReportedByUserId);

				entity.HasMany(e => e.ReviewedDamageReports)
					  .WithOne(a => a.ReviewedByUser)
					  .HasForeignKey(a => a.ReviewedByUserId);

				// Shipment

				entity.HasMany(e => e.RequestedShipments)
					  .WithOne(a => a.RequestedByUser)
					  .HasForeignKey(a => a.RequestedByUserId);

				entity.HasMany(e => e.ApprovedShipments)
				      .WithOne(a => a.ApprovedByUser)
				      .HasForeignKey(a => a.ApprovedByUserId);

				entity.HasMany(e => e.UploadedShipmentMedia)
				      .WithOne(a => a.UploadedByUser)
				      .HasForeignKey(a => a.UploadedByUserId);

				// Order 
				entity.HasMany(e => e.Orders)
					  .WithOne(a => a.Staff)
					  .HasForeignKey(a => a.StaffId);
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

			// ================== Store ==================
			modelBuilder.Entity<Store>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.HasIndex(e => e.Code)
					  .IsUnique();

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

				// FK 
				entity.HasMany(e => e.Cabins)
					  .WithOne(a => a.Store)
					  .HasForeignKey(a => a.StoreId);

				entity.HasMany(e => e.StoreInvitations)
				      .WithOne(s => s.Store)
				      .HasForeignKey(s => s.StoreId);

				entity.HasMany(e => e.InventoryLocations)
					  .WithOne(s => s.Store)
					  .HasForeignKey(s => s.StoreId);

				entity.HasMany(e => e.Orders)
		              .WithOne(s => s.Store)
		              .HasForeignKey(s => s.StoreId);

				entity.HasOne(e => e.Partner)
					  .WithMany(a => a.Stores)
					  .HasForeignKey(e => e.PartnerId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Store_Partner");
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
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Cabin_Store");

				entity.HasMany(e => e.CabinSnapshots)
					  .WithOne(a => a.Cabin)
					  .HasForeignKey(a => a.CabinId);

				entity.HasMany(e => e.Shelves)
					  .WithOne(a => a.Cabin)
					  .HasForeignKey(a => a.CabinId);
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

				entity.Property(e => e.BasePrice)
					  .IsRequired()
					  .HasColumnType("decimal(18,2)");

				entity.Property(e => e.Description)
					  .HasMaxLength(1000);

				entity.Property(e => e.Brand)
					  .HasMaxLength(200);

				entity.Property(e => e.Material)
					  .HasMaxLength(200);

				entity.Property(e => e.OriginCountry)
					  .HasMaxLength(100);

				entity.Property(e => e.AgeRange)
					  .HasMaxLength(50);

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

				entity.HasMany(e => e.ProductColors)
				      .WithOne(c => c.Product)
				      .HasForeignKey(c => c.ProductId);
			});

			// ================== ProductColor ==================
			modelBuilder.Entity<ProductColor>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Sku)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(e => e.HexCode)
					  .HasMaxLength(10);

				 //===== MEDIA / QR =====
				entity.Property(e => e.QrCode)
					  .HasMaxLength(500);

				entity.Property(e => e.Model3DUrl)
					  .HasMaxLength(500);

				entity.Property(e => e.ImageUrl)
					  .HasMaxLength(500);

				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);

				// Product
				entity.HasOne(e => e.Product)
					  .WithMany(p => p.ProductColors)
					  .HasForeignKey(e => e.ProductId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_ProductColor_Product");

				// Inventory
				entity.HasMany(e => e.Inventories)
					  .WithOne(i => i.ProductColor)
					  .HasForeignKey(i => i.ProductColorId);

				// OrderItem
				entity.HasMany(e => e.OrderItems)
					  .WithOne(o => o.ProductColor)
					  .HasForeignKey(o => o.ProductColorId);

				// ShipmentItem
				entity.HasMany(e => e.ShipmentItems)
					  .WithOne(s => s.ProductColor)
					  .HasForeignKey(s => s.ProductColorId);

				// InventoryTransaction
				entity.HasMany(e => e.InventoryTransactions)
					  .WithOne(t => t.ProductColor)
					  .HasForeignKey(t => t.ProductColorId);

				// DamageReport
				entity.HasMany(e => e.DamageReports)
					  .WithOne(d => d.ProductColor)
					  .HasForeignKey(d => d.ProductColorId);

				// SKU của ProductColor cũng phải unique
				entity.HasIndex(e => e.Sku)
					  .IsUnique();
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

				// ===== Parent - Child (Self reference) =====
				entity.HasOne(e => e.Parent)
					  .WithMany(e => e.Children)
					  .HasForeignKey(e => e.ParentId)
					  .OnDelete(DeleteBehavior.Restrict);


				// FK -> Product
				entity.HasMany(e => e.Products)
					  .WithOne(a => a.ProductCategory)
					  .HasForeignKey(a => a.ProductCategoryId);
			});

			// ================== Partner ==================
			modelBuilder.Entity<Partner>(entity =>
			{

				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.CompanyName)
					  .IsRequired()
					  .HasMaxLength(200);

				entity.Property(e => e.Tier)
					  .IsRequired()
					  .HasMaxLength(50)
					  .HasDefaultValue("STANDARD");

				entity.Property(e => e.RevenueSharePercent)
					  .HasPrecision(5, 2) // ví dụ: 30.00 (%)
					  .HasComment("Revenue share percentage from 0 to 100");

				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt);

				// FK
				entity.HasMany(e => e.Stores)
					  .WithOne(s => s.Partner)
					  .HasForeignKey(s => s.PartnerId);

				entity.HasMany(e => e.Users)
					  .WithOne(s => s.Partner)
					  .HasForeignKey(s => s.PartnerId);
			});

			// ================== UserStore ==================
			modelBuilder.Entity<UserStore>(entity =>
			{
				entity.HasKey(e => new { e.UserId, e.StoreId });

				entity.Property(e => e.StoreRole)
					  .IsRequired()
					  .HasConversion<string>() // Manager / Staff
					  .HasMaxLength(20);

				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);

				// ===== FK -> User =====
				entity.HasOne(e => e.User)
					  .WithMany(u => u.UserStores)
					  .HasForeignKey(e => e.UserId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_UserStore_User");

				// ===== FK -> Store ===== -> Không cho xoá Store nếu có User liên kết
				entity.HasOne(e => e.Store)
					  .WithMany(s => s.UserStores)
					  .HasForeignKey(e => e.StoreId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_UserStore_Store");
			});

			// ================== StoreInvitation ==================
			modelBuilder.Entity<StoreInvitation>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.StoreRole)
					  .IsRequired()
					  .HasConversion<string>() // Manager / Staff
					  .HasMaxLength(20);

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasConversion<string>() // Manager / Staff
					  .HasMaxLength(20)
					  .HasDefaultValue(InvitationStatus.Pending);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.ExpiredAt);

				// ===== Relationships =====

				entity.HasOne(e => e.Store)
					  .WithMany(s => s.StoreInvitations)
					  .HasForeignKey(e => e.StoreId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_StoreInvitation_Store");

				entity.HasOne(e => e.User)
					  .WithMany(u => u.StoreInvitations)
					  .HasForeignKey(e => e.UserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_StoreInvitation_User");

				// 1 user chỉ có 1 invite pending cho 1 store
				entity.HasIndex(e => new { e.StoreId, e.UserId, e.Status })
					  .HasDatabaseName("IX_StoreInvitation_Store_User_Status");
			});

			// ================== CabinSnapshot ==================
			modelBuilder.Entity<CabinSnapshot>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.EventType)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.ImageUrl)
					  .IsRequired()
					  .HasMaxLength(500);

				entity.Property(e => e.TakenAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.HasOne(e => e.Cabin)
					  .WithMany(c => c.CabinSnapshots)
					  .HasForeignKey(e => e.CabinId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_CabinSnapshot_Cabin");

				entity.HasOne(e => e.User)
					  .WithMany(u => u.CabinSnapshots)
					  .HasForeignKey(e => e.UserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_CabinSnapshot_User");

				// Query nhanh theo cabin + thời gian
				entity.HasIndex(e => new { e.CabinId, e.TakenAt });
			});

			// ================== Shelf ==================
			modelBuilder.Entity<Shelf>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()
					  .HasMaxLength(50);

				entity.Property(e => e.Level)
					  .IsRequired();

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				entity.HasOne(e => e.Cabin)
					  .WithMany(c => c.Shelves)
					  .HasForeignKey(e => e.CabinId)	
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_Shelf_Cabin");

				entity.HasMany(e => e.ShelfSlots)
					  .WithOne(s => s.Shelf)
					  .HasForeignKey(s => s.ShelfId);

				// Mỗi cabin không được có 2 shelf trùng code
				entity.HasIndex(e => new { e.CabinId, e.Code })
					  .IsUnique();
			});

			// ================== ShelfSlot ==================
			modelBuilder.Entity<ShelfSlot>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()
					  .HasMaxLength(50);

				entity.Property(e => e.DisplayCapacity)
					  .IsRequired();

				entity.Property(e => e.IdealWeight)
					  .HasPrecision(10, 2);

				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);

				entity.HasOne(e => e.Shelf)
					  .WithMany(s => s.ShelfSlots)
					  .HasForeignKey(e => e.ShelfId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_ShelfSlot_Shelf");

				// Mỗi shelf không được có 2 slot trùng code
				entity.HasIndex(e => new { e.ShelfId, e.Code })
					  .IsUnique();
			});

			// ================== DamageReport ==================
			modelBuilder.Entity<DamageReport>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Quantity)
					  .IsRequired();

				entity.Property(e => e.Description);

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.ReportedByUserId)
					  .IsRequired();

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.ReviewedByUserId);

				entity.Property(e => e.ReviewedAt);

				// ===== Relationships =====

				entity.HasOne(e => e.InventoryLocation)
					  .WithMany(l => l.DamageReports)
					  .HasForeignKey(e => e.InventoryLocationId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_DamageReport_InventoryLocation");

				entity.HasOne(e => e.ProductColor)
					  .WithMany(p => p.DamageReports)
					  .HasForeignKey(e => e.ProductColorId)	
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_DamageReport_ProductColor");

				entity.HasOne(e => e.ReportedByUser)
					  .WithMany(u => u.ReportedDamageReports)
					  .HasForeignKey(e => e.ReportedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_DamageReport_ReportedByUser");

				entity.HasOne(e => e.ReviewedByUser)
					  .WithMany(u => u.ReviewedDamageReports)
					  .HasForeignKey(e => e.ReviewedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_DamageReport_ReviewedByUser");

				entity.HasMany(e => e.DamageMedia)
					  .WithOne(m => m.DamageReport)
					  .HasForeignKey(m => m.DamageReportId);
			});

			// ==================== DamageMedia ==================
			modelBuilder.Entity<DamageMedia>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.MediaUrl)
					  .IsRequired()
					  .HasMaxLength(500);

				entity.Property(e => e.MediaType)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP"); 

				// ===== Relationships =====

				entity.HasOne(e => e.DamageReport)
					  .WithMany(r => r.DamageMedia)
					  .HasForeignKey(e => e.DamageReportId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_DamageMedia_DamageReport");
			});

			// ================== Warehouse ==================
			modelBuilder.Entity<Warehouse>(entity =>
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

				entity.Property(e => e.Address)
					  .HasMaxLength(500);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt);

				// ===== Relationships =====

				entity.HasMany(e => e.InventoryLocations)
					  .WithOne(l => l.Warehouse)
					  .HasForeignKey(l => l.WarehouseId)
					  .OnDelete(DeleteBehavior.Restrict);

				// ===== Index =====

				entity.HasIndex(e => e.Code)
					  .IsUnique();
			});

			// ================== Inventory ==================
			modelBuilder.Entity<Inventory>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Quantity)
					  .IsRequired();

				// ===== Relationships =====

				entity.HasOne(e => e.InventoryLocation)
					  .WithMany(l => l.Inventories)
					  .HasForeignKey(e => e.InventoryLocationId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Inventory_InventoryLocation");

				entity.HasOne(e => e.ProductColor)
					  .WithMany(p => p.Inventories)
					  .HasForeignKey(e => e.ProductColorId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Inventory_ProductColor");

				entity.HasOne(e => e.Disposition)
					  .WithMany(d => d.Inventories)
					  .HasForeignKey(e => e.DispositionId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Inventory_Disposition");

				// ===== Index =====

				entity.HasIndex(e => new { e.InventoryLocationId, e.ProductColorId, e.DispositionId })
					  .IsUnique();
			});

			// ================== InventoryTransaction ==================
			modelBuilder.Entity<InventoryTransaction>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Quantity)
					  .IsRequired();

				entity.Property(e => e.ReferenceType)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				// ===== Relationships =====

				entity.HasOne(e => e.ProductColor)
					  .WithMany(p => p.InventoryTransactions)
					  .HasForeignKey(e => e.ProductColorId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_InventoryTransaction_ProductColor");

				entity.HasOne(e => e.FromLocation)
					  .WithMany(l => l.OutgoingInventoryTransactions)
					  .HasForeignKey(e => e.FromLocationId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_InventoryTransaction_FromLocation");

				entity.HasOne(e => e.ToLocation)
					  .WithMany(l => l.IncomingInventoryTransactions)
					  .HasForeignKey(e => e.ToLocationId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_InventoryTransaction_ToLocation");

				entity.HasOne(e => e.FromDisposition)
					  .WithMany(d => d.FromInventoryTransactions)
					  .HasForeignKey(e => e.FromDispositionId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_InventoryTransaction_FromDisposition");

				entity.HasOne(e => e.ToDisposition)
					  .WithMany(d => d.ToInventoryTransactions)
					  .HasForeignKey(e => e.ToDispositionId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_InventoryTransaction_ToDisposition");
			});

			// ================== InventoryLocation ==================
			modelBuilder.Entity<InventoryLocation>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Type)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(150);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				// ===== Relationships =====

				entity.HasOne(e => e.Warehouse)
					  .WithMany(w => w.InventoryLocations)
					  .HasForeignKey(e => e.WarehouseId)
					  .HasConstraintName("FK_InventoryLocation_Warehouse");

				entity.HasOne(e => e.Store)
					  .WithMany(s => s.InventoryLocations)
					  .HasForeignKey(e => e.StoreId)
					  .HasConstraintName("FK_InventoryLocation_Store");

				// Inventory
				entity.HasMany(e => e.Inventories)
					  .WithOne(i => i.InventoryLocation)
					  .HasForeignKey(i => i.InventoryLocationId);

				// Shipments
				entity.HasMany(e => e.FromShipments)
					  .WithOne(s => s.FromLocation)
					  .HasForeignKey(s => s.FromLocationId);

				entity.HasMany(e => e.ToShipments)
					  .WithOne(s => s.ToLocation)
					  .HasForeignKey(s => s.ToLocationId);

				// DamageReports
				entity.HasMany(e => e.DamageReports)	
					  .WithOne(d => d.InventoryLocation)
					  .HasForeignKey(d => d.InventoryLocationId);

				// InventoryTransactions
				entity.HasMany(e => e.OutgoingInventoryTransactions)
					  .WithOne(t => t.FromLocation)
					  .HasForeignKey(t => t.FromLocationId);

				entity.HasMany(e => e.IncomingInventoryTransactions)
					  .WithOne(t => t.ToLocation)
					  .HasForeignKey(t => t.ToLocationId);
			});

			// ================== InventoryDisposition ==================
			modelBuilder.Entity<InventoryDisposition>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()
					  .HasMaxLength(30);

				entity.Property(e => e.Description)
					  .HasMaxLength(255);

				// ===== Relationships =====

				entity.HasMany(e => e.Inventories)
					  .WithOne(i => i.Disposition)
					  .HasForeignKey(i => i.DispositionId);

				entity.HasMany(e => e.FromInventoryTransactions)
					  .WithOne(t => t.FromDisposition)
					  .HasForeignKey(t => t.FromDispositionId);

				entity.HasMany(e => e.ToInventoryTransactions)
					  .WithOne(t => t.ToDisposition)
					  .HasForeignKey(t => t.ToDispositionId);

				// ===== Index =====
				entity.HasIndex(e => e.Code)
					  .IsUnique();
			});

			// ==================== Shipment ==================
			modelBuilder.Entity<Shipment>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()
					  .HasMaxLength(50);

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.RequestedByUserId)
					  .IsRequired();

				// ===== Relationships =====

				entity.HasOne(e => e.FromLocation)
					  .WithMany(l => l.FromShipments)
					  .HasForeignKey(e => e.FromLocationId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Shipment_FromLocation");

				entity.HasOne(e => e.ToLocation)
					  .WithMany(l => l.ToShipments)
					  .HasForeignKey(e => e.ToLocationId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Shipment_ToLocation");

				entity.HasOne(e => e.RequestedByUser)
					  .WithMany(u => u.RequestedShipments)
					  .HasForeignKey(e => e.RequestedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Shipment_RequestedByUser");

				entity.HasOne(e => e.ApprovedByUser)
					  .WithMany(u => u.ApprovedShipments)
					  .HasForeignKey(e => e.ApprovedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Shipment_ApprovedByUser");

				// Shipment

				entity.HasMany(e => e.Items)
					  .WithOne(i => i.Shipment)
					  .HasForeignKey(i => i.ShipmentId);

				entity.HasMany(e => e.Media)
			          .WithOne(i => i.Shipment)
			          .HasForeignKey(i => i.ShipmentId);

				// ===== Index =====

				entity.HasIndex(e => e.Code)
					  .IsUnique();

				entity.HasIndex(e => new { e.FromLocationId, e.ToLocationId });
			});

			// ================== ShipmentItem ==================
			modelBuilder.Entity<ShipmentItem>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.ExpectedQuantity)
					  .IsRequired();

				entity.Property(e => e.ReceivedQuantity)
					  .IsRequired();

				// ===== Relationships =====

				entity.HasOne(e => e.Shipment)
					  .WithMany(s => s.Items)
					  .HasForeignKey(e => e.ShipmentId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_ShipmentItem_Shipment");

				entity.HasOne(e => e.ProductColor)
					  .WithMany(p => p.ShipmentItems)
					  .HasForeignKey(e => e.ProductColorId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShipmentItem_ProductColor");

				// ===== Index =====

				entity.HasIndex(e => new { e.ShipmentId, e.ProductColorId })
					  .IsUnique();
			});

			// ================== ShipmentMedia ==================
			modelBuilder.Entity<ShipmentMedia>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.MediaUrl)
					  .IsRequired()
					  .HasMaxLength(500);

				entity.Property(e => e.MediaType)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.Purpose)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				// ===== Relationships =====

				entity.HasOne(e => e.Shipment)
					  .WithMany(s => s.Media)
					  .HasForeignKey(e => e.ShipmentId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_ShipmentMedia_Shipment");

				entity.HasOne(e => e.UploadedByUser)
					  .WithMany(u => u.UploadedShipmentMedia)
					  .HasForeignKey(e => e.UploadedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShipmentMedia_UploadedByUser");

				// ===== Index =====

				entity.HasIndex(e => e.ShipmentId);
			});

			// ================== Order ==================
			modelBuilder.Entity<Order>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.TotalAmount)
					  .IsRequired()
					  .HasPrecision(18, 2);

				entity.Property(e => e.PaymentMethod)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				// ===== Relationships =====

				entity.HasOne(e => e.Store)
					  .WithMany(s => s.Orders)
					  .HasForeignKey(e => e.StoreId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Order_Store");

				entity.HasOne(e => e.Staff)
					  .WithMany(u => u.Orders)
					  .HasForeignKey(e => e.StaffId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Order_Staff");

				entity.HasMany(e => e.OrderItems)
					  .WithOne(i => i.Order)
					  .HasForeignKey(i => i.OrderId)
					  .OnDelete(DeleteBehavior.Cascade);
			});

			// ================== OrderItem ==================
			modelBuilder.Entity<OrderItem>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Quantity)
					  .IsRequired();

				entity.Property(e => e.Price)
					  .IsRequired()
					  .HasPrecision(18, 2);

				// ===== Relationships =====

				entity.HasOne(e => e.Order)
					  .WithMany(o => o.OrderItems)
					  .HasForeignKey(e => e.OrderId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_OrderItem_Order");

				entity.HasOne(e => e.ProductColor)
					  .WithMany(p => p.OrderItems)
					  .HasForeignKey(e => e.ProductColorId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_OrderItem_ProductColor");

				// ===== Index =====

				entity.HasIndex(e => new { e.OrderId, e.ProductColorId })
					  .IsUnique();
			});
		}
	}
}
