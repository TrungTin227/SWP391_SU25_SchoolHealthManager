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
        public DbSet<NurseProfile> NurseProfiles { get; set; }
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

        // Quản lý khám sức khỏe
        public DbSet<CheckupCampaign> CheckupCampaigns { get; set; }
        public DbSet<CheckupSchedule> CheckupSchedules { get; set; }
        public DbSet<CheckupRecord> CheckupRecords { get; set; }

        // Quản lý vật tư y tế
        public DbSet<MedicalSupply> MedicalSupplies { get; set; }
        public DbSet<SupplyUsage> SupplyUsages { get; set; }

        // Hẹn tư vấn
        public DbSet<CounselingAppointment> CounselingAppointments { get; set; }
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

            // Precision config for CheckupRecord
            builder.Entity<CheckupRecord>(entity =>
            {
                entity.Property(e => e.HeightCm).HasPrecision(5, 2);
                entity.Property(e => e.WeightKg).HasPrecision(5, 2);
                entity.Property(e => e.BloodPressureDiastolic).HasPrecision(3, 0);
            });

            // Parent ↔ User one-to-one
            builder.Entity<Parent>()
                .HasKey(p => p.UserId);
            builder.Entity<Parent>()
                .HasOne(p => p.User)
                .WithOne(u => u.Parent)
                .HasForeignKey<Parent>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Parent ↔ Student relationship (1 Parent có nhiều Students)
            builder.Entity<Student>()
                .HasOne(s => s.Parent)  // Student có 1 Parent
                .WithMany(p => p.Students)  // Parent có nhiều Students
                .HasForeignKey(s => s.ParentUserId)  
                .OnDelete(DeleteBehavior.NoAction);

            // NurseProfile ↔ User one-to-one
            builder.Entity<NurseProfile>()
                .HasKey(sp => sp.UserId);
            builder.Entity<NurseProfile>()
                .HasOne(sp => sp.User)
                .WithOne(u => u.StaffProfile)
                .HasForeignKey<NurseProfile>(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite key for VaccineDoseInfo
            builder.Entity<VaccineDoseInfo>()
                .HasKey(v => new { v.VaccineTypeId, v.DoseNumber });

            // ParentMedicationDelivery relationships - FIX CASCADE PATHS
            builder.Entity<ParentMedicationDelivery>(entity =>
            {
                entity.HasOne(e => e.ReceivedUser)
                      .WithMany()
                      .HasForeignKey(e => e.ReceivedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Parent)
                      .WithMany()
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.NoAction);  // CHANGED: Cascade -> NoAction
            });

            // MedicalSupply ↔ SupplyUsage
            builder.Entity<MedicalSupply>()
                .HasMany(ms => ms.SupplyUsages)
                .WithOne(su => su.MedicalSupply)
                .HasForeignKey(su => su.MedicalSupplyId)
                .OnDelete(DeleteBehavior.Cascade);

            // SupplyUsage ↔ HealthEvent - FIX CASCADE PATHS
            builder.Entity<SupplyUsage>()
                .HasOne(su => su.HealthEvent)
                .WithMany(he => he.SupplyUsages)  // Specify the correct collection property
                .HasForeignKey(su => su.HealthEventId)  // This should match your entity's FK property
                .OnDelete(DeleteBehavior.NoAction);  

            // SupplyUsage ↔ UsedBy User
            builder.Entity<SupplyUsage>()
                .HasOne(su => su.UsedByNurse)
                .WithMany()
                .HasForeignKey(su => su.NurseProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // CounselingAppointment relationships & indexes
            builder.Entity<CounselingAppointment>(ca =>
            {
                ca.HasOne(x => x.Student)
                  .WithMany(s => s.CounselingAppointments)
                  .HasForeignKey(x => x.StudentId)
                  .OnDelete(DeleteBehavior.NoAction);
                ca.HasIndex(x => x.StudentId);

                ca.HasOne(x => x.Parent)
                  .WithMany(p => p.CounselingAppointments)
                  .HasForeignKey(x => x.ParentId)
                  .OnDelete(DeleteBehavior.Cascade);
                ca.HasIndex(x => x.ParentId);

                ca.HasOne(x => x.StaffUser)
                  .WithMany(u => u.CounselingAppointments)
                  .HasForeignKey(x => x.StaffUserId)
                  .OnDelete(DeleteBehavior.NoAction);
                ca.HasIndex(x => x.StaffUserId);

                ca.HasOne(x => x.CheckupRecord)
                  .WithMany(cr => cr.CounselingAppointments)
                  .HasForeignKey(x => x.CheckupRecordId)
                  .OnDelete(DeleteBehavior.NoAction);
                ca.HasIndex(x => x.CheckupRecordId);

                ca.HasOne(x => x.VaccinationRecord)
                  .WithMany(vr => vr.CounselingAppointments)
                  .HasForeignKey(x => x.VaccinationRecordId)
                  .OnDelete(DeleteBehavior.NoAction);
                ca.HasIndex(x => x.VaccinationRecordId);
            });

            // THÊM: Cấu hình explicit cho các relationships chính
            builder.Entity<HealthEvent>(he =>
            {
                he.HasOne(x => x.Student)
                  .WithMany(s => s.HealthEvents)
                  .HasForeignKey(x => x.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<VaccinationRecord>(vr =>
            {
                vr.HasOne(x => x.Student)
                  .WithMany(s => s.VaccinationRecords)
                  .HasForeignKey(x => x.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CheckupRecord>(cr =>
            {
                cr.HasOne(x => x.Schedule)
                  .WithMany()
                  .HasForeignKey(x => x.ScheduleId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            // Unique and performance indexes
            builder.Entity<Student>()
                   .HasIndex(s => s.StudentCode)
                   .IsUnique();

            builder.Entity<HealthProfile>()
                   .HasIndex(h => h.StudentId);

            builder.Entity<HealthEvent>()
                   .HasIndex(e => e.StudentId);

            builder.Entity<VaccinationRecord>()
                   .HasIndex(vr => vr.StudentId);
            builder.Entity<Student>()
                .HasIndex(s => s.ParentUserId)
                .HasDatabaseName("IX_Students_ParentUserId");
        }
    }
}