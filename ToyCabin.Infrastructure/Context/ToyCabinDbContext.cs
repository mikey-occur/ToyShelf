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
		public DbSet<Role> Roles { get; set; }
		public DbSet<AccountRole> AccountRoles { get; set; }
		public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// ================== ACCOUNT ==================
			modelBuilder.Entity<Account>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Id)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.Username)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(e => e.Email)
					  .IsRequired()
					  .HasMaxLength(255);

				entity.Property(e => e.PasswordHash)
					  .IsRequired()
					  .HasMaxLength(255);

				entity.Property(e => e.Salt)
					  .IsRequired()
					  .HasMaxLength(255);

				entity.Property(e => e.AvatarUrl)
					  .HasMaxLength(500);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt)
					  .HasDefaultValueSql("CURRENT_TIMESTAMP");
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
				entity.HasKey(e => e.OtpId);

				entity.Property(e => e.OtpId)
					  .ValueGeneratedOnAdd();

				entity.Property(e => e.OtpCode)
					  .IsRequired()
					  .HasMaxLength(20);

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
		}
	}
}
