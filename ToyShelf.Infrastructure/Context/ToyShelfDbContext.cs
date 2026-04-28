using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Infrastructure.Context
{
	public class ToyShelfDbContext : DbContext
	{
		public ToyShelfDbContext(DbContextOptions<ToyShelfDbContext> options) : base(options) {}
		public DbSet<Account> Accounts { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<UserWarehouse> UserWarehouses { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<AccountRole> AccountRoles { get; set; }
		public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }

		public DbSet<Partner> Partners { get; set; }
		public DbSet<PartnerTier> PartnerTiers { get; set; }
		public DbSet<UserStore> UserStores { get; set; }
			
		public DbSet<Store> Stores { get; set; }
		public DbSet<StoreOrder> StoreOrders { get; set; }
		public DbSet<StoreOrderItem> StoreOrderItems { get; set; }
		public DbSet<StoreCreationRequest> StoreCreationRequests { get; set; }
		public DbSet<StoreInvitation> StoreInvitations { get; set; }
		public DbSet<Warehouse> Warehouses { get; set; }

		public DbSet<ShelfOrder> ShelfOrders { get; set; }
		public DbSet<ShelfOrderItem> ShelfOrderItems { get; set; }
		public DbSet<ShelfShipmentItem> ShelfShipmentItems { get; set; }


		public DbSet<Shelf> Shelves { get; set; }
		public DbSet<ShelfTransaction> ShelfTransactions { get; set; }
		public DbSet<ShelfType> ShelfTypes { get; set; }
		public DbSet<ShelfTypeLevel> shelfTypeLevels { get; set; }

		public DbSet<InventoryShelf> InventoryShelves { get; set; }

		public DbSet<Product> Products { get; set; }
		public DbSet<ProductCategory> ProductCategories { get; set; }
		public DbSet<ProductColor> ProductColors { get; set; }
		public DbSet<Color> Colors { get; set; }

		public DbSet<CommissionTable> CommissionTables { get; set; }
		public DbSet<CommissionTableApply> CommissionTableApplies { get; set; }
		public DbSet<CommissionItemCategory> CommissionItemCategories { get; set; }
		public DbSet<CommissionItem> CommissionItems { get; set; }


		public DbSet<Shipment> Shipments { get; set; }
		public DbSet<ShipmentItem> ShipmentItems { get; set; }
		public DbSet<ShipmentMedia> ShipmentMedias { get; set; } // Bỏ s
		public DbSet<ShipmentAssignment> ShipmentAssignments { get; set; }
		public DbSet<AssignmentShelfOrder> AssignmentShelfOrders { get; set; }
		public DbSet<AssignmentShelfOrderItem> AssignmentShelfOrderItems { get; set; }
		public DbSet<AssignmentStoreOrder> AssignmentStoreOrders { get; set; }
		public DbSet<AssignmentStoreOrderItem> AssignmentStoreOrderItems { get; set; }
		public DbSet<AssignmentDamageReport> AssignmentDamageReports { get; set; }

		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }	
        public DbSet<CommissionHistory> CommissionHistories { get; set; }

		public DbSet<MonthlySettlement> MonthlySettlements { get; set; }

		public DbSet<Inventory> Inventories { get; set; }
		public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
		public DbSet<InventoryLocation> InventoryLocations { get; set; }

		public DbSet<DamageReport> DamageReports { get; set; }
		public DbSet<DamageMedia> DamageMedia { get; set; }
		public DbSet<DamageReportItem> DamageReportItems { get; set; }

		public DbSet<Notification> Notifications { get; set; }

		public DbSet<City> Cities { get; set; }

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

				entity.HasMany(e => e.ApprovedStoreOrders)
					  .WithOne(s => s.ApprovedByUser)
					  .HasForeignKey(s => s.ApprovedByUserId);

				entity.HasOne(e => e.Partner)
					  .WithMany(a => a.Users)
					  .HasForeignKey(e => e.PartnerId)
					  .HasConstraintName("FK_User_Partner");

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

				entity.HasMany(e => e.UploadedShipmentMedia)
				      .WithOne(a => a.UploadedByUser)
				      .HasForeignKey(a => a.UploadedByUserId);

				entity.HasMany(e => e.Shipments)
					  .WithOne(a => a.Shipper)
					  .HasForeignKey(a => a.ShipperId);

				// Order 
				entity.HasMany(e => e.Orders)
					  .WithOne(a => a.Staff)
					  .HasForeignKey(a => a.StaffId);

				// StoreCreationRequest
				entity.HasMany(e => e.CreatedStoreRequests)
					  .WithOne(a => a.RequestedByUser)
					  .HasForeignKey(a => a.RequestedByUserId);

				entity.HasMany(e => e.ReviewedStoreRequests)
					  .WithOne(a => a.ReviewedByUser)
					  .HasForeignKey(a => a.ReviewedByUserId);

				// StoreOrder
				entity.HasMany(e => e.RequestStoreOrders)
					  .WithOne(a => a.RequestedByUser)
					  .HasForeignKey(a => a.RequestedByUserId);

				entity.HasMany(e => e.RejectedStoreOrders)
					  .WithOne(a => a.RejectedByUser)
					  .HasForeignKey(a => a.RejectedByUserId);

				// UserWarehouse
				entity.HasMany(e => e.UserWarehouses)
					  .WithOne(a => a.User)
					  .HasForeignKey(a => a.UserId);
			});

			// ================== USER WAREHOUSE ==================
			modelBuilder.Entity<UserWarehouse>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Role)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt);

				// Tránh duplicate (1 user - 1 warehouse)
				entity.HasIndex(e => new { e.UserId, e.WarehouseId })
					  .IsUnique();

				// ===== Relationship =====

				entity.HasOne(e => e.User)
					  .WithMany(u => u.UserWarehouses)
					  .HasForeignKey(e => e.UserId)
					  .HasConstraintName("FK_UserWarehouse_User");

				entity.HasOne(e => e.Warehouse)
					  .WithMany(w => w.UserWarehouses)
					  .HasForeignKey(e => e.WarehouseId)
					  .HasConstraintName("FK_UserWarehouse_Warehouse");
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

				entity.Property(e => e.Latitude)
					  .HasColumnType("double precision");

				entity.Property(e => e.Longitude)
					  .HasColumnType("double precision");

				entity.Property(e => e.PhoneNumber)
					  .HasMaxLength(20);

				entity.Property(e => e.CurrentShelfCount)
					  .IsRequired()
					  .HasDefaultValue(0);

				// Chống xung đột dữ liệu (Race Condition) khi cộng/trừ số lượng
				entity.Property(e => e.RowVersion)
					  .IsRowVersion();

				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt);


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

				entity.HasOne(e => e.City)
					  .WithMany(a => a.Stores)
					  .HasForeignKey(e => e.CityId)
					  .HasConstraintName("FK_Store_City");
			});

			// ================== StoreOrder ==================
			modelBuilder.Entity<StoreOrder>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()
					  .HasMaxLength(30);

				entity.HasIndex(e => e.Code)
					  .IsUnique();

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.ApprovedAt);

				entity.Property(e => e.PartnerAdminApprovedAt);

				entity.Property(e => e.RejectedAt);

				// FK

				entity.HasOne(e => e.StoreLocation)
					  .WithMany(a => a.StoreOrders)
					  .HasForeignKey(e => e.StoreLocationId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_StoreOrder_StoreLocation");

				entity.HasOne(e => e.RequestedByUser)
					  .WithMany(a => a.RequestStoreOrders)
					  .HasForeignKey(e => e.RequestedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_StoreOrder_RequestedByUser");

				entity.HasMany(e => e.Items)
					  .WithOne(a => a.StoreOrder)
					  .HasForeignKey(a => a.StoreOrderId);

				entity.HasOne(e => e.ApprovedByUser)
					  .WithMany(a => a.ApprovedStoreOrders)
					  .HasForeignKey(e => e.ApprovedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_StoreOrder_ApprovedByUser");

				entity.HasOne(e => e.RejectedByUser)
					  .WithMany(a => a.RejectedStoreOrders)
					  .HasForeignKey(e => e.RejectedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_StoreOrder_RejectedByUser");

				entity.HasOne(e => e.PartnerAdminApprovedByUser)
					  .WithMany(a => a.PartnerAdminStoreOrders)
					  .HasForeignKey(e => e.PartnerAdminApprovedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_StoreOrder_PartnerApprovedByUser");
			});

			// ================== StoreOrderItem ==================
			modelBuilder.Entity<StoreOrderItem>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Quantity)
					  .IsRequired();

				entity.Property(e => e.FulfilledQuantity)
					  .IsRequired()
					  .HasDefaultValue(0);
				// FK

				entity.HasOne(e => e.StoreOrder)
					  .WithMany(a => a.Items)
					  .HasForeignKey(e => e.StoreOrderId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_StoreOrderItem_StoreOrder");

				entity.HasOne(e => e.ProductColor)
					  .WithMany(a => a.StoreOrderItems)
					  .HasForeignKey(e => e.ProductColorId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_StoreOrderItem_ProductColor");
			});


			// ================== StoreCreationRequest ==================
			modelBuilder.Entity<StoreCreationRequest>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(200);

				entity.Property(e => e.StoreAddress)
					  .IsRequired()
					  .HasMaxLength(300);

				entity.Property(e => e.Latitude)
					  .HasColumnType("double precision");

				entity.Property(e => e.Longitude)
					  .HasColumnType("double precision");

				entity.Property(e => e.PhoneNumber)
					  .HasMaxLength(20);

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20)
					  .HasDefaultValue(StoreRequestStatus.Pending);

				entity.Property(e => e.RejectReason)
					  .HasMaxLength(500);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.ReviewedAt);

				// Index
				entity.HasIndex(e => e.Status);
				entity.HasIndex(e => e.PartnerId);
				entity.HasIndex(e => e.CreatedAt);

				// FK Partner
				entity.HasOne(e => e.Partner)
					  .WithMany(p => p.StoreCreationRequests)
					  .HasForeignKey(e => e.PartnerId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_StoreCreationRequest_Partner");

				// FK RequestedByUser
				entity.HasOne(e => e.RequestedByUser)
					  .WithMany(u => u.CreatedStoreRequests)
					  .HasForeignKey(e => e.RequestedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_StoreCreationRequest_RequestedByUser");

				// FK ReviewedByUser
				entity.HasOne(e => e.ReviewedByUser)
					  .WithMany(u => u.ReviewedStoreRequests)
					  .HasForeignKey(e => e.ReviewedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_StoreCreationRequest_ReviewedByUser");

				entity.HasOne(e => e.City)
					  .WithMany(u => u.StoreCreationRequests)
					  .HasForeignKey(e => e.CityId)
					  .HasConstraintName("FK_StoreCreationRequest_City");
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

				entity.Property(e => e.Barcode)
					  .HasMaxLength(200);

				// THÊM INDEX CHO BARCODE Ở ĐÂY
				entity.HasIndex(e => e.Barcode)
					  .HasDatabaseName("IX_Product_Barcode");

				entity.HasIndex(p => p.Barcode).IsUnique();

				entity.Property(e => e.Brand)
					  .HasMaxLength(200);

				entity.Property(e => e.Material)
					  .HasMaxLength(200);

				entity.Property(e => e.OriginCountry)
					  .HasMaxLength(100);

				entity.Property(e => e.AgeRange)
					  .HasMaxLength(50);

				entity.Property(e => e.Width)
					  .HasColumnType("decimal(10,2)");

				entity.Property(e => e.Length)
				      .HasColumnType("decimal(10,2)");

				entity.Property(e => e.Height)
					  .HasColumnType("decimal(10,2)");

				entity.Property(e => e.Weight)
					  .HasColumnType("decimal(10,2)");

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

			// ================== Color ==================
			modelBuilder.Entity<Color>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(50);

				entity.Property(e => e.HexCode)
					  .IsRequired()
					  .HasMaxLength(7);

				entity.Property(e => e.SkuCode)
			          .HasMaxLength(10)
			          .IsRequired();

				entity.HasIndex(e => e.SkuCode).IsUnique();

				// Quan hệ Color - ProductColor (1 - N)
				entity.HasMany(e => e.ProductColors)
					  .WithOne(pc => pc.Color)
					  .HasForeignKey(pc => pc.ColorId)
					  .OnDelete(DeleteBehavior.Cascade);

				// Indexes
				entity.HasIndex(e => e.Name)
					  .IsUnique();

				entity.HasIndex(e => e.HexCode)
					  .IsUnique();
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

				entity.Property(e => e.Price)
					  .IsRequired()
					  .HasColumnType("decimal(18,2)");

				entity.Property(e => e.QrCode)
					  .HasColumnType("text");

				entity.Property(e => e.Model3DUrl)
					  .HasMaxLength(500);

				entity.Property(e => e.ImageUrl)
					  .HasMaxLength(500);

				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);

                entity.Property(e => e.HasFile)
                      .HasDefaultValue(false);

                // ================== RELATIONSHIPS ==================

                // ProductColor → Product (N - 1)
                entity.HasOne(e => e.Product)
					  .WithMany(p => p.ProductColors)
					  .HasForeignKey(e => e.ProductId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_ProductColor_Product");

				// ProductColor → Color (N - 1)
				entity.HasOne(e => e.Color)
					  .WithMany(c => c.ProductColors)
					  .HasForeignKey(e => e.ColorId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ProductColor_Color");

				// ================== CHILD COLLECTIONS ==================

				entity.HasMany(e => e.Inventories)
					  .WithOne(i => i.ProductColor)
					  .HasForeignKey(i => i.ProductColorId);

				entity.HasMany(e => e.OrderItems)
					  .WithOne(o => o.ProductColor)
					  .HasForeignKey(o => o.ProductColorId);

				entity.HasMany(e => e.ShipmentItems)
					  .WithOne(s => s.ProductColor)
					  .HasForeignKey(s => s.ProductColorId);

				entity.HasMany(e => e.InventoryTransactions)
					  .WithOne(t => t.ProductColor)
					  .HasForeignKey(t => t.ProductColorId);

				entity.HasMany(e => e.StoreOrderItems)
					  .WithOne(a => a.ProductColor)
				      .HasForeignKey(a => a.ProductColorId);

				// ================== INDEX ==================

				// SKU phải unique
				entity.HasIndex(e => e.Sku)
					  .IsUnique();
			});


			
			// ================== CommissionItem ==================
			modelBuilder.Entity<CommissionItem>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.CommissionRate)
					  .IsRequired()
					  .HasColumnType("decimal(5,4)");
				// VD: 0.1500 = 15%

				// CommissionItem → CommissionTable (N - 1)
				entity.HasOne(e => e.CommissionTable)
					  .WithMany(pt => pt.CommissionItems)
					  .HasForeignKey(e => e.CommissionTableId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_CommissionItem_CommissionTable");

				// CommissionItem → CommissionItemCategory (N - 1)
				entity.HasMany(e => e.ItemCategories)
					  .WithOne(ic => ic.CommissionItem)
					  .HasForeignKey(ic => ic.CommissionItemId)
					  .OnDelete(DeleteBehavior.Cascade);

			});

			// ================== CommissionTable ==================
			modelBuilder.Entity<CommissionTable>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(255);

				entity.Property(e => e.Type)
					  .IsRequired()
					  .HasConversion<string>()   // Tier / Clearance
					  .HasMaxLength(20);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				// Quan hệ CommissionTable - PartnerTier (N - 1, nullable cho Clearance)
				entity.HasOne(e => e.PartnerTier)
					  .WithMany(pt => pt.CommissionTables)
					  .HasForeignKey(e => e.PartnerTierId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_CommissionTable_PartnerTier");

				// Quan hệ CommissionTable - CommissionItem (1 - N)
				entity.HasMany(e => e.CommissionItems)
					  .WithOne(pi => pi.CommissionTable)
					  .HasForeignKey(pi => pi.CommissionTableId);

				// Quan hệ CommissionTable - CommissionTableApply (1 - N)
				entity.HasMany(e => e.CommissionTableApplies)
					  .WithOne(pta => pta.CommissionTable)
					  .HasForeignKey(pta => pta.CommissionTableId);
			});

			// ================== CommissionTableApply ==================
			modelBuilder.Entity<CommissionTableApply>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Name)
					  .HasMaxLength(255);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				entity.Property(e => e.StartDate)
					  .IsRequired();

				entity.Property(e => e.EndDate);

				// Quan hệ CommissionTableApply - Partner (N - 1)
				entity.HasOne(e => e.Partner)
					  .WithMany(p => p.CommissionTableApplies)
					  .HasForeignKey(e => e.PartnerId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_CommissionTableApply_Partner");

				// Quan hệ CommissionTableApply - CommissionTable (N - 1)
				entity.HasOne(e => e.CommissionTable)
					  .WithMany(pt => pt.CommissionTableApplies)
					  .HasForeignKey(e => e.CommissionTableId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_CommissionTableApply_CommissionTable");
			});


			modelBuilder.Entity<CommissionItemCategory>(entity =>
			{
				// Khai báo khóa chính ghép
				entity.HasKey(cic => new { cic.CommissionItemId, cic.ProductCategoryId });

				entity.HasOne(cic => cic.CommissionItem)
					  .WithMany(i => i.ItemCategories)
					  .HasForeignKey(cic => cic.CommissionItemId);

				entity.HasOne(cic => cic.ProductCategory)
					  .WithMany(pc => pc.CommissionItemCategories)
					  .HasForeignKey(cic => cic.ProductCategoryId);
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

				// FK -> CommissionItemCategory
				entity.HasMany(e => e.CommissionItemCategories)
					  .WithOne(cic => cic.ProductCategory)
					  .HasForeignKey(cic => cic.ProductCategoryId);
			});

			// ================== Partner ==================
			modelBuilder.Entity<Partner>(entity =>
			{

				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.HasIndex(e => e.Code)
					  .IsUnique();

				entity.Property(e => e.CompanyName)
					  .IsRequired()
					  .HasMaxLength(200);

                entity.Property(e => e.BankName)
					  .HasMaxLength(100)
					  .IsRequired(false);

                entity.Property(e => e.BankAccountNumber)
					  .HasMaxLength(50)
					  .IsRequired(false);

                entity.Property(e => e.BankAccountName)
					  .HasMaxLength(150)
					  .IsRequired(false);

                entity.Property(e => e.Address)
					  .IsRequired()
					  .HasMaxLength(300);

				entity.Property(e => e.Latitude)
					  .HasColumnType("double precision");

				entity.Property(e => e.Longitude)
					  .HasColumnType("double precision");

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

				//entity.HasMany(e => e.Shelves)
				//	  .WithOne(s => s.Partner)
				//	  .HasForeignKey(s => s.PartnerId);

				entity.HasMany(e => e.CommissionTableApplies)
					  .WithOne(s => s.Partner)
					  .HasForeignKey(s => s.PartnerId);

				entity.HasOne(e => e.PartnerTier)
					  .WithMany(e => e.Partners)
					  .HasForeignKey(e => e.PartnerTierId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Partner_PartnerTier");

				entity.HasMany(e => e.CommissionHistories)
					  .WithOne(i => i.Partner)
					  .HasForeignKey(i => i.PartnerId);

				entity.HasMany(e => e.StoreCreationRequests)
					  .WithOne(i => i.Partner)
					  .HasForeignKey(i => i.PartnerId);
			});

			// ================== PartnerTier ==================
			modelBuilder.Entity<PartnerTier>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(e => e.Priority)
					  .IsRequired();

				
				entity.HasIndex(e => e.Priority)
					  .IsUnique();

				
				entity.HasIndex(e => e.Name)
					  .IsUnique();

				entity.Property(e => e.MaxShelvesPerStore)
		               .IsRequired(false);

				entity.HasMany(e => e.Partners)
					  .WithOne(s => s.PartnerTier)
					  .HasForeignKey(s => s.PartnerTierId);

				entity.HasMany(e => e.CommissionTables)
					  .WithOne(s => s.PartnerTier)
					  .HasForeignKey(s => s.PartnerTierId);

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


			// ================== ShelfOrder ==================
			modelBuilder.Entity<ShelfOrder>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()
					  .HasMaxLength(30);

				entity.HasIndex(e => e.Code)
					  .IsUnique();

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.ApprovedAt);

				entity.Property(e => e.PartnerAdminApprovedAt);

				entity.Property(e => e.RejectedAt);

				entity.Property(e => e.Note)
					  .HasMaxLength(500);

				entity.Property(e => e.AdminNote)
					  .HasMaxLength(500);

				// ================= FK =================

				entity.HasOne(e => e.StoreLocation)
					  .WithMany(a => a.ShelfOrders) 
					  .HasForeignKey(e => e.StoreLocationId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShelfOrder_StoreLocation");

				entity.HasOne(e => e.RequestedByUser)
					  .WithMany(a => a.RequestShelfOrders)
					  .HasForeignKey(e => e.RequestedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShelfOrder_RequestedByUser");

				entity.HasOne(e => e.ApprovedByUser)
					  .WithMany(a => a.ApprovedShelfOrders)
					  .HasForeignKey(e => e.ApprovedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShelfOrder_ApprovedByUser");

				entity.HasOne(e => e.RejectedByUser)
					  .WithMany(a => a.RejectedShelfOrders)
					  .HasForeignKey(e => e.RejectedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShelfOrder_RejectedByUser");

				entity.HasOne(e => e.PartnerAdminApprovedByUser)
					  .WithMany(a => a.PartnerAdminShelfOrders)
					  .HasForeignKey(e => e.PartnerAdminApprovedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShelfOrder_PartnerAdminApprovedByUser");

				// ================= RELATION =================

				entity.HasMany(e => e.Items)
					  .WithOne(a => a.ShelfOrder)
					  .HasForeignKey(a => a.ShelfOrderId)
					  .OnDelete(DeleteBehavior.Cascade);

			});

			// ================== ShelfOrderItem ==================
			modelBuilder.Entity<ShelfOrderItem>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Quantity)
					  .IsRequired();

				entity.Property(e => e.FulfilledQuantity)
					  .IsRequired()
					  .HasDefaultValue(0);

				entity.Property(e => e.ShelfTypeName)
					  .IsRequired()
					  .HasMaxLength(200);

				entity.Property(e => e.ImageUrl)
					  .HasMaxLength(500);

				// ================= FK =================

				entity.HasOne(e => e.ShelfOrder)
					  .WithMany(a => a.Items)
					  .HasForeignKey(e => e.ShelfOrderId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_ShelfOrderItem_ShelfOrder");

				entity.HasOne(e => e.ShelfType)
					  .WithMany(a => a.ShelfOrderItems) 
					  .HasForeignKey(e => e.ShelfTypeId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShelfOrderItem_ShelfType");
			});

			// ================== ShelfShipmentItem ==================
			modelBuilder.Entity<ShelfShipmentItem>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				// ================= FK =================

				entity.HasOne(e => e.Shipment)
					  .WithMany(s => s.ShelfShipmentItems)
					  .HasForeignKey(e => e.ShipmentId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_ShelfShipmentItem_Shipment");

				entity.HasOne(e => e.Shelf)
					  .WithMany(st => st.ShelfShipmentItems)
					  .HasForeignKey(e => e.ShelfId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShelfShipmentItem_Shelf");

				entity.HasOne(e => e.ShelfOrderItem)
					  .WithMany(s => s.ShelfShipmentItems)
					  .HasForeignKey(e => e.ShelfOrderItemId)
					  .HasConstraintName("FK_ShelfShipmentItem_ShelfOrderItem");
			});

			// ================== ShelfTransaction ==================
			modelBuilder.Entity<ShelfTransaction>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.FromStatus)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.ToStatus)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.ReferenceType)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				// ===== Relationships =====

				entity.HasOne(e => e.Shelf)
					  .WithMany(st => st.ShelfTransactions) 
					  .HasForeignKey(e => e.ShelfId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShelfTransaction_Shelf");

				entity.HasOne(e => e.FromLocation)
					  .WithMany(il => il.OutgoingShelfTransactions) 
					  .HasForeignKey(e => e.FromLocationId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShelfTransaction_FromLocation");

				entity.HasOne(e => e.ToLocation)
					  .WithMany(il => il.IncomingShelfTransactions)
					  .HasForeignKey(e => e.ToLocationId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShelfTransaction_ToLocation");
			});


			// ================== InventoryShelf ==================
			modelBuilder.Entity<InventoryShelf>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Quantity)
					  .IsRequired()
					  .HasDefaultValue(0);

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				// Mối quan hệ với InventoryLocation
				entity.HasOne(e => e.InventoryLocation)
					  .WithMany(l => l.InventoryShelves)
					  .HasForeignKey(e => e.InventoryLocationId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_InventoryShelf_InventoryLocation");

				// Mối quan hệ với ShelfType
				entity.HasOne(e => e.ShelfType)
					  .WithMany(st => st.InventoryShelves) 
					  .HasForeignKey(e => e.ShelfTypeId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_InventoryShelf_ShelfType");

				entity.HasIndex(e => new { e.InventoryLocationId, e.ShelfTypeId, e.Status }).IsUnique();
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

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasConversion<string>() 
					  .HasMaxLength(20);

				entity.Property(e => e.AssignedAt)
					  .IsRequired(false);
	
				entity.Property(e => e.UnassignedAt)
					  .IsRequired(false);

				// ===== Relationships =====
				//entity.HasOne(e => e.Store)
				//	  .WithMany(c => c.Shelves)
				//	  .HasForeignKey(e => e.StoreId)
				//	  .HasConstraintName("FK_Shelf_Store");

				//entity.HasOne(e => e.Partner)
				//	  .WithMany(c => c.Shelves)
				//	  .HasForeignKey(e => e.PartnerId)
				//	  .HasConstraintName("FK_Shelf_Partner");

				entity.HasOne(e => e.InventoryLocation)
					  .WithMany(c => c.Shelves)
					  .HasForeignKey(e => e.InventoryLocationId)
					  .HasConstraintName("FK_Shelf_InventoryLocation");

				entity.HasOne(e => e.ShelfType)
					  .WithMany(st => st.Shelves)
					  .HasForeignKey(e => e.ShelfTypeId)
					  .HasConstraintName("FK_Shelf_ShelfType");

				entity.HasIndex(e => e.Code)
					  .IsUnique();
			});

			// ================== ShelfType ==================
			modelBuilder.Entity<ShelfType>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(255); 

				entity.Property(e => e.Description)
					  .HasMaxLength(1000);

				entity.Property(e => e.ImageUrl)
					  .HasMaxLength(500);

				entity.Property(e => e.SuitableProductCategoryTypes)
					  .HasMaxLength(500);

				entity.Property(e => e.DisplayGuideline)
					  .HasMaxLength(1000);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);
			});

			// ================== ShelfTypeLevel ==================
			modelBuilder.Entity<ShelfTypeLevel>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(e => e.SuitableProductCategoryTypes)
					  .HasMaxLength(500);

				entity.Property(e => e.DisplayGuideline)
					  .HasMaxLength(1000);

				entity.HasOne(e => e.ShelfType)
					  .WithMany(st => st.ShelfTypeLevels)
					  .HasForeignKey(e => e.ShelfTypeId)
					  .HasConstraintName("FK_ShelfTypeLevel_ShelfType")
					  .OnDelete(DeleteBehavior.Cascade);
			});

			// ================== DamageReport ==================
			modelBuilder.Entity<DamageReport>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id).ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()
					  .HasMaxLength(50);

				entity.Property(e => e.Type)
						.IsRequired()
						.HasConversion<string>()
						.HasMaxLength(20); 
				entity.Property(e => e.Source)
						.IsRequired()
						.HasConversion<string>()
						.HasMaxLength(20);
				entity.Property(e => e.Status)
						.IsRequired()
						.HasConversion<string>()
						.HasMaxLength(20);

				entity.Property(e => e.Description);
				entity.Property(e => e.AdminNote);

				entity.Property(e => e.IsWarrantyClaim)
					  .HasDefaultValue(false);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.PartnerAdminApprovedAt);

				entity.Property(e => e.ReviewedAt);
				entity.Property(e => e.WarrantyExpirationDate);

				// ===== Relationships =====

				// Liên kết với Kho/Cửa hàng
				entity.HasOne(e => e.InventoryLocation)
					  .WithMany(l => l.DamageReports)
					  .HasForeignKey(e => e.InventoryLocationId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_DamageReport_InventoryLocation");

				// Nhân sự
				entity.HasOne(e => e.ReportedByUser)
					  .WithMany(u => u.ReportedDamageReports)
					  .HasForeignKey(e => e.ReportedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_DamageReport_ReportedByUser");

				entity.HasOne(e => e.PartnerAdminApprovedByUser)
					  .WithMany(u => u.PartnerAdminDamageReports)
					  .HasForeignKey(e => e.PartnerAdminApprovedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_DamageReport_PartnerAdminApprovedByUser");

				entity.HasOne(e => e.ReviewedByUser)
					  .WithMany(u => u.ReviewedDamageReports)
					  .HasForeignKey(e => e.ReviewedByUserId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_DamageReport_ReviewedByUser");

				// Media đính kèm
				entity.HasMany(e => e.Items)
					  .WithOne(m => m.DamageReport)
					  .HasForeignKey(m => m.DamageReportId)
					  .OnDelete(DeleteBehavior.Cascade);
			});

			// ================== DamageReportItem ==================
			modelBuilder.Entity<DamageReportItem>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id).ValueGeneratedOnAdd();

				entity.Property(e => e.DamageItemType)
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.Quantity);

				// ===== Relationships =====

				entity.HasOne(e => e.DamageReport)
					  .WithMany(r => r.Items)
					  .HasForeignKey(e => e.DamageReportId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_DamageReportItem_DamageReport");

				// Nối với Product
				entity.HasOne(e => e.ProductColor)
					  .WithMany(p => p.DamageReportItems) 
					  .HasForeignKey(e => e.ProductColorId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_DamageReportItem_ProductColor");

				// Nối với Shelf
				entity.HasOne(e => e.Shelf)
					  .WithMany(s => s.DamageReportItems) 
					  .HasForeignKey(e => e.ShelfId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_DamageReportItem_Shelf");

				entity.HasMany(e => e.DamageMedia)
					  .WithOne(i => i.DamageReportItem)
					  .HasForeignKey(i => i.DamageReportItemId);
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

				entity.HasOne(e => e.DamageReportItem)
					  .WithMany(r => r.DamageMedia)
					  .HasForeignKey(e => e.DamageReportItemId)
					  .OnDelete(DeleteBehavior.Cascade)
					  .HasConstraintName("FK_DamageMedia_DamageReportItem");
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

				entity.Property(e => e.Latitude)
					  .HasColumnType("double precision");

				entity.Property(e => e.Longitude)
					  .HasColumnType("double precision");

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

				entity.HasOne(e => e.City)
					  .WithMany(l => l.Warehouses)
					  .HasForeignKey(e => e.CityId)
					  .HasConstraintName("FK_Warehouse_City");

				// UserWarehouse
				entity.HasMany(e => e.UserWarehouses)
					  .WithOne(a => a.Warehouse)
					  .HasForeignKey(a => a.WarehouseId);

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

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

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

				// ===== Index =====

				entity.HasIndex(e => new { e.InventoryLocationId, e.ProductColorId, e.Status })
					  .IsUnique();
			});

			// ================== InventoryTransaction ==================
			modelBuilder.Entity<InventoryTransaction>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.FromStatus)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.ToStatus)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.Quantity)
					  .IsRequired();

				entity.Property(e => e.ReferenceType)
					  .IsRequired()
					  .HasConversion<string>()
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
			});

			// ================== InventoryLocation ==================
			modelBuilder.Entity<InventoryLocation>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Type)
					  .IsRequired()
					  .HasConversion<string>()
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

				entity.HasMany(e => e.StoreOrders)
				      .WithOne(a => a.StoreLocation)
				      .HasForeignKey(a => a.StoreLocationId);

				entity.HasMany(e => e.ShipmentAssignments)
					  .WithOne(a => a.WarehouseLocation)
					  .HasForeignKey(a => a.WarehouseLocationId);

				entity.HasMany(e => e.Shelves)
					  .WithOne(l => l.InventoryLocation)
					  .HasForeignKey(l => l.InventoryLocationId);

				// ===== Unique =====
				entity.HasIndex(e => e.WarehouseId)
					  .IsUnique()
					  .HasFilter("\"WarehouseId\" IS NOT NULL");

				entity.HasIndex(e => e.StoreId)
					  .IsUnique()
					  .HasFilter("\"StoreId\" IS NOT NULL");

				// ===== Check constraint (NEW WAY) =====
				entity.ToTable(t =>
				{
					t.HasCheckConstraint(
						"CK_InventoryLocation_OnlyOneOwner",
						"(\"WarehouseId\" IS NOT NULL AND \"StoreId\" IS NULL) OR (\"WarehouseId\" IS NULL AND \"StoreId\" IS NOT NULL)"
					);
				});
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

				entity.Property(e => e.IsReturn)
					  .IsRequired()
					  .HasDefaultValue(false);

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.PickedUpAt);

				entity.Property(e => e.DeliveredAt);

				entity.Property(e => e.StoreReceivedAt);

				entity.Property(e => e.ReturnPickedUpAt);

				entity.Property(e => e.ArrivedWarehouseAt);

				entity.Property(e => e.WarehouseReceivedAt);



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

				entity.HasOne(e => e.Shipper)
					  .WithMany(u => u.Shipments)
					  .HasForeignKey(e => e.ShipperId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Shipment_Shipper");

				entity.HasOne(s => s.ShipmentAssignment)
					  .WithMany(sa => sa.Shipments)
					  .HasForeignKey(s => s.ShipmentAssignmentId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_Shipment_Assignment");

				// Shipment

				entity.HasMany(e => e.Items)
					  .WithOne(i => i.Shipment)
					  .HasForeignKey(i => i.ShipmentId);

				entity.HasMany(e => e.Media)
			          .WithOne(i => i.Shipment)
			          .HasForeignKey(i => i.ShipmentId);

				entity.HasMany(e => e.ShelfShipmentItems)
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

				entity.HasOne(e => e.Shelf)
					  .WithMany(s => s.ShipmentItems) 
				  	  .HasForeignKey(e => e.ShelfId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShipmentItem_Shelf");

				entity.HasOne(e => e.StoreOrderItem)
					  .WithMany(s => s.ShipmentItems) 
					  .HasForeignKey(e => e.StoreOrderItemId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShipmentItem_StoreOrderItem");

				entity.HasOne(e => e.DamageReportItem)
					  .WithMany(d => d.ShipmentItems) 
					  .HasForeignKey(e => e.DamageReportItemId)
					  .OnDelete(DeleteBehavior.Restrict)
					  .HasConstraintName("FK_ShipmentItem_DamageReportItem");
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
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.Purpose)
					  .IsRequired()
					  .HasConversion<string>()
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

			// ================== ShipmentAssignment ==================
			modelBuilder.Entity<ShipmentAssignment>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Type)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.CreatedAt)
				  .IsRequired()
				  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.RespondedAt);

				entity.Property(e => e.InProgressAt);

				entity.Property(e => e.CompletedAt);

				entity.HasOne(e => e.Shipper)
				  .WithMany(s => s.Shippers)
				  .HasForeignKey(e => e.ShipperId)
				  .HasConstraintName("FK_ShipmentAssignment_Shipper");	

				entity.HasOne(e => e.AssignedByUser)
				  .WithMany(s => s.AssignedShipmentAssignments)
				  .HasForeignKey(e => e.AssignedByUserId)
				  .HasConstraintName("FK_ShipmentAssignment_AssignedByUser");

				entity.HasOne(e => e.CreatedByUser)
				  .WithMany(s => s.CreatedShipmentAssignments)
				  .HasForeignKey(e => e.CreatedByUserId)
				  .HasConstraintName("FK_ShipmentAssignment_CreatedByUser");

				entity.HasOne(e => e.WarehouseLocation)
				  .WithMany(s => s.ShipmentAssignments)
				  .HasForeignKey(e => e.WarehouseLocationId)
				  .HasConstraintName("FK_ShipmentAssignment_InventoryLocation");

				// N
				entity.HasMany(e => e.Shipments)
				  .WithOne(i => i.ShipmentAssignment)
				  .HasForeignKey(i => i.ShipmentAssignmentId);

				entity.HasMany(e => e.AssignmentDamageReports)
				  .WithOne(i => i.ShipmentAssignment)
				  .HasForeignKey(i => i.ShipmentAssignmentId);

				entity.HasMany(e => e.AssignmentStoreOrders)
				  .WithOne(i => i.ShipmentAssignment)
				  .HasForeignKey(i => i.ShipmentAssignmentId);

				entity.HasMany(e => e.AssignmentShelfOrders)
				  .WithOne(i => i.ShipmentAssignment)
				  .HasForeignKey(i => i.ShipmentAssignmentId);
			});

			// ================== AssignmentStoreOrder ==================
			modelBuilder.Entity<AssignmentStoreOrder>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.HasIndex(e => new { e.ShipmentAssignmentId, e.StoreOrderId }).IsUnique();

				// Cấu hình mối quan hệ với ShipmentAssignment
				entity.HasOne(e => e.ShipmentAssignment)
					.WithMany(s => s.AssignmentStoreOrders)
					.HasForeignKey(e => e.ShipmentAssignmentId)
					.OnDelete(DeleteBehavior.Cascade);

				// Cấu hình mối quan hệ với StoreOrder
				entity.HasOne(e => e.StoreOrder)
					.WithMany(s => s.AssignmentStoreOrders)
					.HasForeignKey(e => e.StoreOrderId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			// ================== AssignmentShelfOrder ==================
			modelBuilder.Entity<AssignmentShelfOrder>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.HasIndex(e => new { e.ShipmentAssignmentId, e.ShelfOrderId }).IsUnique();

				// Cấu hình mối quan hệ với ShipmentAssignment
				entity.HasOne(e => e.ShipmentAssignment)
					.WithMany(s => s.AssignmentShelfOrders)
					.HasForeignKey(e => e.ShipmentAssignmentId)
					.OnDelete(DeleteBehavior.Cascade);

				// Cấu hình mối quan hệ với ShelfOrder
				entity.HasOne(e => e.ShelfOrder)
					.WithMany(s => s.AssignmentShelfOrders)
					.HasForeignKey(e => e.ShelfOrderId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			// ================== AssignmentStoreOrderItem ==================
			modelBuilder.Entity<AssignmentStoreOrderItem>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.AllocatedQuantity)
					  .IsRequired();

				// Đảm bảo một món trong một Assignment không bị lặp lại dòng phân bổ
				entity.HasIndex(e => new { e.AssignmentStoreOrderId, e.StoreOrderItemId }).IsUnique();

				// Nối với bảng trung gian StoreOrder (Xóa cha thì xóa con)
				entity.HasOne(e => e.AssignmentStoreOrder)
					.WithMany(s => s.AssignmentStoreOrderItems)
					.HasForeignKey(e => e.AssignmentStoreOrderId)
					.OnDelete(DeleteBehavior.Cascade);

				// Nối với món hàng gốc (Dùng Restrict để tránh lỗi Multiple Cascade Paths)
				entity.HasOne(e => e.StoreOrderItem)
					.WithMany(s => s.AssignmentStoreOrderItems)
					.HasForeignKey(e => e.StoreOrderItemId)
					.OnDelete(DeleteBehavior.Restrict);
			});

			// ================== AssignmentShelfOrderItem ==================
			modelBuilder.Entity<AssignmentShelfOrderItem>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.AllocatedQuantity)
					  .IsRequired();

				entity.HasIndex(e => new { e.AssignmentShelfOrderId, e.ShelfOrderItemId }).IsUnique();

				// Nối với bảng trung gian ShelfOrder (Xóa cha thì xóa con)
				entity.HasOne(e => e.AssignmentShelfOrder)
					.WithMany(s => s.AssignmentShelfOrderItems)
					.HasForeignKey(e => e.AssignmentShelfOrderId)
					.OnDelete(DeleteBehavior.Cascade);

				// Nối với kệ gốc
				entity.HasOne(e => e.ShelfOrderItem)
					.WithMany(s => s.AssignmentShelfOrderItems)
					.HasForeignKey(e => e.ShelfOrderItemId)
					.OnDelete(DeleteBehavior.Restrict);
			});

			// ================== AssignmentDamageReport ==================
			modelBuilder.Entity<AssignmentDamageReport>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.HasIndex(e => new { e.ShipmentAssignmentId, e.DamageReportId }).IsUnique();

				// Cấu hình mối quan hệ với ShipmentAssignment
				entity.HasOne(e => e.ShipmentAssignment)
					.WithMany(s => s.AssignmentDamageReports)
					.HasForeignKey(e => e.ShipmentAssignmentId)
					.OnDelete(DeleteBehavior.Cascade);

				// Cấu hình mối quan hệ với ShelfOrder
				entity.HasOne(e => e.DamageReport)
					.WithMany(s => s.AssignmentDamageReports)
					.HasForeignKey(e => e.DamageReportId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			// ================== Order ==================
			modelBuilder.Entity<Order>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.CustomerName)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(e => e.CustomerEmail)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.HasIndex(e => e.CustomerEmail)
		              .HasDatabaseName("IX_Orders_UserEmail");


				entity.Property(e => e.TotalAmount)
					  .IsRequired()
					  .HasPrecision(18, 2);

				entity.Property(e => e.PaymentMethod)
					  .IsRequired()
					  .HasMaxLength(20);

				entity.Property(e => e.OrderCode)
		              .IsRequired();

				entity.HasIndex(e => e.OrderCode)
		              .IsUnique();

				entity.Property(e => e.Status)
					  .IsRequired()
					  .HasMaxLength(20);

                entity.Property(e => e.BankReference)
					  .IsRequired(false) 
					  .HasMaxLength(50);

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

				entity.HasMany(e => e.CommissionHistories)
					  .WithOne(i => i.OrderItem)
					  .HasForeignKey(i => i.OrderItemId);

				// ===== Index =====

				entity.HasIndex(e => new { e.OrderId, e.ProductColorId })
					  .IsUnique();
			});
			// ================== Commission History ==================
			modelBuilder.Entity<CommissionHistory>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					 .ValueGeneratedOnAdd();

				entity.Property(e => e.SalesAmount)
					.HasColumnType("decimal(18,2)");

				entity.Property(e => e.CommissionAmount)
					.HasColumnType("decimal(18,2)"); // VD: 100.000,50

				entity.Property(e => e.AppliedRate)
					.HasColumnType("decimal(18,4)"); // VD: 0.1500 (15%)

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				// ===== Relationships =====
				entity.HasOne(ch => ch.OrderItem)
					.WithMany(oi => oi.CommissionHistories)
					.HasForeignKey(ch => ch.OrderItemId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("FK_CommissionHistory_OrderItem");

				entity.HasOne(ch => ch.Partner)
					.WithMany(oi => oi.CommissionHistories) 
					.HasForeignKey(ch => ch.PartnerId)
					.OnDelete(DeleteBehavior.Restrict)
				    .HasConstraintName("FK_CommissionHistory_Partner");

				entity.HasOne(ch => ch.MonthlySettlement)
					.WithMany(ms => ms.CommissionHistories)
					.HasForeignKey(ch => ch.MonthlySettlementId)
					.OnDelete(DeleteBehavior.Restrict) 
					.HasConstraintName("FK_CommissionHistory_MonthlySettlement");
			});

			// ================== Monthly Settlement (Phiếu chốt sổ tháng) ==================
			modelBuilder.Entity<MonthlySettlement>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id).ValueGeneratedOnAdd();

				entity.Property(e => e.TotalSalesAmount)
					.HasColumnType("decimal(18,2)");

				entity.Property(e => e.TotalCommissionAmount)
					.HasColumnType("decimal(18,2)");

				entity.Property(e => e.DeductionAmount)
				     .HasColumnType("decimal(18,2)");

				entity.Property(e => e.FinalAmount)
				      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TransferReceiptUrl)
					  .HasMaxLength(500) 
					  .IsRequired(false);

                entity.Property(e => e.Note)
				      .HasMaxLength(500);

				entity.Property(e => e.Status)
					.IsRequired()
					.HasMaxLength(20);

				entity.Property(e => e.CreatedAt)
					.IsRequired()
					.HasDefaultValueSql("CURRENT_TIMESTAMP");

				// ===== Relationships =====
				entity.HasOne(ms => ms.Partner)
					.WithMany(p => p.MonthlySettlements) 
					.HasForeignKey(ms => ms.PartnerId)
					.OnDelete(DeleteBehavior.Restrict) 
					.HasConstraintName("FK_MonthlySettlement_Partner");
			});

			// ================== CITY ==================
			modelBuilder.Entity<City>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Code)
					  .IsRequired()
					  .HasMaxLength(10);

				entity.Property(e => e.Name)
					  .IsRequired()
					  .HasMaxLength(150);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt)
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				// Unique constraints
				entity.HasIndex(e => e.Code)
					  .IsUnique();

				entity.HasIndex(e => e.Name)
					  .IsUnique();

				entity.HasMany(e => e.Warehouses)
					 .WithOne(w => w.City)
					 .HasForeignKey(w => w.CityId)
					 .OnDelete(DeleteBehavior.Restrict); // hoặc Cascade tùy business

				entity.HasMany(e => e.Stores)
					 .WithOne(w => w.City)
					 .HasForeignKey(w => w.CityId)
					 .OnDelete(DeleteBehavior.Restrict); // hoặc Cascade tùy business

				entity.HasMany(e => e.StoreCreationRequests)
					 .WithOne(w => w.City)
					 .HasForeignKey(w => w.CityId)
					 .OnDelete(DeleteBehavior.Restrict); // hoặc Cascade tùy business
			});

			modelBuilder.Entity<Notification>(entity =>
			{
				entity.HasKey(n => n.Id);
				entity.Property(n => n.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(n => n.Title).IsRequired().HasMaxLength(255);
				entity.Property(n => n.Content).IsRequired();

				entity.Property(n => n.RefType)
					  .HasMaxLength(100);

				entity.Property(e => e.CreatedAt)
					  .IsRequired();

				entity.Property(e => e.ReadAt);

				entity.HasIndex(n => new { n.UserId, n.CreatedAt }).IsDescending(false, true);

				// --- THÊM QUAN HỆ VÀO ĐÂY ---
				entity.HasOne(n => n.User)                  
					  .WithMany(u => u.Notifications)        
					  .HasForeignKey(n => n.UserId)          
					  .OnDelete(DeleteBehavior.Cascade);
			});
		}
	}
}
