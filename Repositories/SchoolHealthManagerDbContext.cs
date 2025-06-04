using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Repositories
{
    public class SchoolHealthManagerDbContext : IdentityDbContext<User, Role, Guid>
    {
        public SchoolHealthManagerDbContext(DbContextOptions<SchoolHealthManagerDbContext> options)
            : base(options)
        {
        }

        #region Core User Management
        public DbSet<Student> Students { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<NurseProfile> NurseProfiles { get; set; }
        #endregion

        #region Health Management
        public DbSet<HealthProfile> HealthProfiles { get; set; }
        public DbSet<HealthEvent> HealthEvents { get; set; }
        public DbSet<CheckupCampaign> CheckupCampaigns { get; set; }
        public DbSet<CheckupSchedule> CheckupSchedules { get; set; }
        public DbSet<CheckupRecord> CheckupRecords { get; set; }
        #endregion

        #region Medication Management
        public DbSet<Medication> Medications { get; set; }
        public DbSet<MedicationLot> MedicationLots { get; set; }
        public DbSet<Dispense> Dispenses { get; set; }
        public DbSet<EventMedication> EventMedications { get; set; }
        public DbSet<ParentMedicationDelivery> ParentMedicationDeliveries { get; set; }
        #endregion

        #region Vaccination Management
        public DbSet<VaccinationType> VaccinationTypes { get; set; }
        public DbSet<VaccinationCampaign> VaccinationCampaigns { get; set; }
        public DbSet<VaccinationSchedule> VaccinationSchedules { get; set; }
        public DbSet<VaccinationRecord> VaccinationRecords { get; set; }
        public DbSet<VaccineDoseInfo> VaccineDoseInfos { get; set; }
        #endregion

        #region Medical Supply Management
        public DbSet<MedicalSupply> MedicalSupplies { get; set; }
        public DbSet<SupplyUsage> SupplyUsages { get; set; }
        #endregion

        #region Support Services
        public DbSet<CounselingAppointment> CounselingAppointments { get; set; }
        public DbSet<FileAttachment> FileAttachments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Report> Reports { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure database schema in logical order
            ConfigureIdentityTables(builder);
            ConfigureUserRelationships(builder);
            ConfigureHealthManagement(builder);
            ConfigureMedicationManagement(builder);
            ConfigureVaccinationManagement(builder);
            ConfigureMedicalSupplyManagement(builder);
            ConfigureSupportServices(builder);
            ConfigureIndexes(builder);
            ConfigurePrecisionAndConstraints(builder);
            // --- Bắt đầu: Thêm cấu hình convert enum -> string cho Medication.Category ---
            builder.Entity<Medication>()
                .Property(m => m.Category)
                .HasConversion(new EnumToStringConverter<MedicationCategory>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // (Tuỳ chọn) Nếu bạn cũng muốn chuyển enum MedicationStatus thành string:
            builder.Entity<Medication>()
                .Property(m => m.Status)
                .HasConversion(new EnumToStringConverter<MedicationStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);
            // 1. CheckupSchedule
            builder.Entity<CheckupSchedule>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<CheckupScheduleStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // 2. HealthEvent
            builder.Entity<HealthEvent>()
                .Property(e => e.EventCategory)
                .HasConversion(new EnumToStringConverter<EventCategory>())
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Entity<HealthEvent>()
                .Property(e => e.EventType)
                .HasConversion(new EnumToStringConverter<EventType>())
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Entity<HealthEvent>()
                .Property(e => e.EventStatus)
                .HasConversion(new EnumToStringConverter<EventStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // 3. HealthProfile
            builder.Entity<HealthProfile>()
                .Property(e => e.Vision)
                .HasConversion(new EnumToStringConverter<VisionLevel>())
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Entity<HealthProfile>()
                .Property(e => e.Hearing)
                .HasConversion(new EnumToStringConverter<HearingLevel>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // 4. CheckupRecord
            builder.Entity<CheckupRecord>()
                .Property(e => e.VisionLeft)
                .HasConversion(new EnumToStringConverter<VisionLevel>())
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Entity<CheckupRecord>()
                .Property(e => e.VisionRight)
                .HasConversion(new EnumToStringConverter<VisionLevel>())
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Entity<CheckupRecord>()
                .Property(e => e.Hearing)
                .HasConversion(new EnumToStringConverter<HearingLevel>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // 5. CounselingAppointment
            builder.Entity<CounselingAppointment>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<ScheduleStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // 6. FileAttachment
            builder.Entity<FileAttachment>()
                .Property(e => e.ReferenceType)
                .HasConversion(new EnumToStringConverter<ReferenceType>())
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Entity<FileAttachment>()
                .Property(e => e.FileType)
                .HasConversion(new EnumToStringConverter<FileType>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // 7. Notification
            builder.Entity<Notification>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<NotificationType>())
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Entity<Notification>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<NotificationStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // 8. Parent
            builder.Entity<Parent>()
                .Property(e => e.Relationship)
                .HasConversion(new EnumToStringConverter<Relationship>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // 9. ParentMedicationDelivery
            builder.Entity<ParentMedicationDelivery>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<StatusMedicationDelivery>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // 10. Report
            builder.Entity<Report>()
                .Property(e => e.ReportType)
                .HasConversion(new EnumToStringConverter<ReportType>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // 11. User
            builder.Entity<User>()
                .Property(e => e.Gender)
                .HasConversion(new EnumToStringConverter<Gender>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // 12. VaccinationSchedule
            builder.Entity<VaccinationSchedule>()
                .Property(e => e.ScheduleType)
                .HasConversion(new EnumToStringConverter<ScheduleType>())
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Entity<VaccinationSchedule>()
                .Property(e => e.ScheduleStatus)
                .HasConversion(new EnumToStringConverter<ScheduleStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);
            // --- Kết thúc convert enum -> string ---
        }

        #region Identity Configuration
        private static void ConfigureIdentityTables(ModelBuilder builder)
        {
            // Map Identity tables to custom names
            builder.Entity<User>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        }
        #endregion

        #region User Relationships Configuration
        private static void ConfigureUserRelationships(ModelBuilder builder)
        {
            // Parent ↔ User (One-to-One)
            builder.Entity<Parent>(entity =>
            {
                entity.HasKey(p => p.UserId);
                entity.HasOne(p => p.User)
                      .WithOne(u => u.Parent)
                      .HasForeignKey<Parent>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // NurseProfile ↔ User (One-to-One)
            builder.Entity<NurseProfile>(entity =>
            {
                entity.HasKey(sp => sp.UserId);
                entity.HasOne(sp => sp.User)
                      .WithOne(u => u.StaffProfile)
                      .HasForeignKey<NurseProfile>(sp => sp.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Parent ↔ Student (One-to-Many)
            builder.Entity<Student>(entity =>
            {
                entity.HasOne(s => s.Parent)
                      .WithMany(p => p.Students)
                      .HasForeignKey(s => s.ParentUserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
        #endregion

        #region Health Management Configuration
        private static void ConfigureHealthManagement(ModelBuilder builder)
        {
            // HealthEvent relationships
            builder.Entity<HealthEvent>(entity =>
            {
                entity.HasOne(he => he.Student)
                      .WithMany(s => s.HealthEvents)
                      .HasForeignKey(he => he.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CheckupRecord relationships
            builder.Entity<CheckupRecord>(entity =>
            {
                entity.HasOne(cr => cr.Schedule)
                      .WithMany()
                      .HasForeignKey(cr => cr.ScheduleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
        #endregion

        #region Medication Management Configuration
        private static void ConfigureMedicationManagement(ModelBuilder builder)
        {
            // ParentMedicationDelivery relationships
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
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
        #endregion

        #region Vaccination Management Configuration
        private static void ConfigureVaccinationManagement(ModelBuilder builder)
        {
            // VaccineDoseInfo composite key
            builder.Entity<VaccineDoseInfo>(entity =>
            {
                entity.HasKey(v => new { v.VaccineTypeId, v.DoseNumber });
            });

            // VaccinationRecord relationships
            builder.Entity<VaccinationRecord>(entity =>
            {
                entity.HasOne(vr => vr.Student)
                      .WithMany(s => s.VaccinationRecords)
                      .HasForeignKey(vr => vr.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
        #endregion

        #region Medical Supply Management Configuration
        private static void ConfigureMedicalSupplyManagement(ModelBuilder builder)
        {
            // MedicalSupply ↔ SupplyUsage (One-to-Many)
            builder.Entity<MedicalSupply>(entity =>
            {
                entity.HasMany(ms => ms.SupplyUsages)
                      .WithOne(su => su.MedicalSupply)
                      .HasForeignKey(su => su.MedicalSupplyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SupplyUsage relationships
            builder.Entity<SupplyUsage>(entity =>
            {
                entity.HasOne(su => su.HealthEvent)
                      .WithMany(he => he.SupplyUsages)
                      .HasForeignKey(su => su.HealthEventId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(su => su.UsedByNurse)
                      .WithMany()
                      .HasForeignKey(su => su.NurseProfileId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
        #endregion

        #region Support Services Configuration
        private static void ConfigureSupportServices(ModelBuilder builder)
        {
            // CounselingAppointment relationships
            builder.Entity<CounselingAppointment>(entity =>
            {
                entity.HasOne(ca => ca.Student)
                      .WithMany(s => s.CounselingAppointments)
                      .HasForeignKey(ca => ca.StudentId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ca => ca.Parent)
                      .WithMany(p => p.CounselingAppointments)
                      .HasForeignKey(ca => ca.ParentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ca => ca.StaffUser)
                      .WithMany(u => u.CounselingAppointments)
                      .HasForeignKey(ca => ca.StaffUserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ca => ca.CheckupRecord)
                      .WithMany(cr => cr.CounselingAppointments)
                      .HasForeignKey(ca => ca.CheckupRecordId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ca => ca.VaccinationRecord)
                      .WithMany(vr => vr.CounselingAppointments)
                      .HasForeignKey(ca => ca.VaccinationRecordId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
        #endregion

        #region Index Configuration
        private static void ConfigureIndexes(ModelBuilder builder)
        {
            // Unique indexes
            builder.Entity<Student>()
                   .HasIndex(s => s.StudentCode)
                   .IsUnique()
                   .HasDatabaseName("IX_Students_StudentCode_Unique");

            // Performance indexes
            builder.Entity<Student>()
                   .HasIndex(s => s.ParentUserId)
                   .HasDatabaseName("IX_Students_ParentUserId");

            builder.Entity<HealthProfile>()
                   .HasIndex(hp => hp.StudentId)
                   .HasDatabaseName("IX_HealthProfiles_StudentId");

            builder.Entity<HealthEvent>()
                   .HasIndex(he => he.StudentId)
                   .HasDatabaseName("IX_HealthEvents_StudentId");

            builder.Entity<VaccinationRecord>()
                   .HasIndex(vr => vr.StudentId)
                   .HasDatabaseName("IX_VaccinationRecords_StudentId");

            // CounselingAppointment indexes
            builder.Entity<CounselingAppointment>(entity =>
            {
                entity.HasIndex(ca => ca.StudentId)
                      .HasDatabaseName("IX_CounselingAppointments_StudentId");

                entity.HasIndex(ca => ca.ParentId)
                      .HasDatabaseName("IX_CounselingAppointments_ParentId");

                entity.HasIndex(ca => ca.StaffUserId)
                      .HasDatabaseName("IX_CounselingAppointments_StaffUserId");

                entity.HasIndex(ca => ca.CheckupRecordId)
                      .HasDatabaseName("IX_CounselingAppointments_CheckupRecordId");

                entity.HasIndex(ca => ca.VaccinationRecordId)
                      .HasDatabaseName("IX_CounselingAppointments_VaccinationRecordId");
            });
        }
        #endregion

        #region Precision and Constraints Configuration
        private static void ConfigurePrecisionAndConstraints(ModelBuilder builder)
        {
            // CheckupRecord precision settings
            builder.Entity<CheckupRecord>(entity =>
            {
                entity.Property(e => e.HeightCm)
                      .HasPrecision(5, 2);

                entity.Property(e => e.WeightKg)
                      .HasPrecision(5, 2);

                entity.Property(e => e.BloodPressureDiastolic)
                      .HasPrecision(3, 0);
            });

            // Add other precision and constraint configurations as needed
            // Example: String length constraints, decimal precision, etc.
        }
        #endregion
    }
}