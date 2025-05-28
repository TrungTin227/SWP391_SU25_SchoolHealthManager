using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class SchoolHealthManagerDbContext : IdentityDbContext<User, Role, Guid>
    {
        public SchoolHealthManagerDbContext(DbContextOptions<SchoolHealthManagerDbContext> options)
            : base(options)
        {
        }

        #region Core Domain DbSets
        public DbSet<Student> Students { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<StaffProfile> StaffProfiles { get; set; }
        public DbSet<HealthProfile> HealthProfiles { get; set; }
        public DbSet<ParentMedicationDelivery> ParentMedicationDeliveries { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<MedicationLot> MedicationLots { get; set; }
        public DbSet<Dispense> Dispenses { get; set; }
        public DbSet<HealthEvent> HealthEvents { get; set; }
        public DbSet<EventMedication> EventMedications { get; set; }
        public DbSet<VaccinationCampaign> VaccinationCampaigns { get; set; }
        public DbSet<VaccinationSchedule> VaccinationSchedules { get; set; }
        public DbSet<VaccineType> VaccineTypes { get; set; }
        public DbSet<VaccinationRecord> VaccinationRecords { get; set; }
        public DbSet<VaccineDoseInfo> VaccineDoseInfos { get; set; }
        public DbSet<FileAttachment> FileAttachments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<CheckupCampaign> CheckupCampaigns { get; set; }
        public DbSet<CheckupSchedule> CheckupSchedules { get; set; }
        public DbSet<CheckupRecord> CheckupRecords { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Identity table mapping
            builder.Entity<User>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            builder.Entity<CheckupRecord>(entity =>
            {
                entity.Property(e => e.HeightCm)
                      .HasPrecision(5, 2);      // tối đa 5 chữ số, 2 số sau dấu phẩy

                entity.Property(e => e.WeightKg)
                      .HasPrecision(5, 2);

                entity.Property(e => e.BloodPressureDiastolic)
                      .HasPrecision(3, 0);      // huyết áp là số nguyên, tối đa 3 chữ số
            });

            // Parent - User one-to-one (shared PK)
            builder.Entity<Parent>()
                .HasKey(p => p.UserId);
            builder.Entity<Parent>()
                .HasOne(p => p.User)
                .WithOne(u => u.Parent)
                .HasForeignKey<Parent>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Parent - Student optional one-to-many
            builder.Entity<Parent>()
                .HasOne(p => p.Student)
                .WithMany(s => s.Parents)
                .HasForeignKey(p => p.StudentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // StaffProfile - User one-to-one (shared PK)
            builder.Entity<StaffProfile>()
                .HasKey(sp => sp.UserId);
            builder.Entity<StaffProfile>()
                .HasOne(sp => sp.User)
                .WithOne(u => u.StaffProfile)
                .HasForeignKey<StaffProfile>(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VaccineDoseInfo>()
                .HasKey(v => new { v.VaccineTypeId, v.DoseNumber });

            builder.Entity<ParentMedicationDelivery>(entity =>
            {
                // Quan hệ tới User nhận thuốc
                entity.HasOne(e => e.ReceivedUser)
                      .WithMany()
                      .HasForeignKey(e => e.ReceivedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ tới Parent (người giao thuốc)
                entity.HasOne(e => e.Parent)
                      .WithMany()  // Removed the reference to ParentMedicationDeliveries
                      .HasForeignKey(e => e.DeliveredBy)
                      .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ tới Student
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Unique index on StudentCode
            builder.Entity<Student>()
                .HasIndex(s => s.StudentCode)
                .IsUnique();

            // Optional: add indexes for performance-critical lookups
            builder.Entity<HealthProfile>()
                .HasIndex(h => h.StudentId);
            builder.Entity<HealthEvent>()
                .HasIndex(e => e.StudentId);
            builder.Entity<VaccinationRecord>()
                .HasIndex(vr => vr.StudentId);
        }
    }
}
