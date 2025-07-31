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
        public DbSet<ParentMedicationDeliveryDetail> ParentMedicationDeliveryDetails { get; set; }
        public DbSet<MedicationSchedule> MedicationSchedules { get; set; }
        public DbSet<MedicationUsageRecord> MedicationUsageRecords { get; set; }
        #endregion

        #region Vaccination Management
        public DbSet<VaccinationType> VaccinationTypes { get; set; }
        public DbSet<VaccinationCampaign> VaccinationCampaigns { get; set; }
        public DbSet<VaccinationSchedule> VaccinationSchedules { get; set; }
        public DbSet<VaccinationRecord> VaccinationRecords { get; set; }
        public DbSet<VaccineDoseInfo> VaccineDoseInfos { get; set; }
        public DbSet<SessionStudent> SessionStudents { get; set; }
        #endregion

        #region Medical Supply Management
        public DbSet<MedicalSupply> MedicalSupplies { get; set; }
        public DbSet<SupplyUsage> SupplyUsages { get; set; }
        public DbSet<MedicalSupplyLot> MedicalSupplyLots { get; set; }
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
            ConfigureEnumConversions(builder);
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

                // Relationship với User (ReportedUser)
                entity.HasOne(he => he.ReportedUser)
                       .WithMany()
                       .HasForeignKey(he => he.ReportedUserId)
                       .OnDelete(DeleteBehavior.NoAction);

                // Relationship với VaccinationRecord (nếu có)
                entity.HasOne(he => he.VaccinationRecord)
                      .WithMany(vr => vr.HealthEvents)
                      .HasForeignKey(he => he.VaccinationRecordId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(he => he.CheckupRecord)
                      .WithMany(cr => cr.HealthEvents)    
                      .HasForeignKey(he => he.CheckupRecordId)
                      .OnDelete(DeleteBehavior.NoAction);
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

            builder.Entity<MedicationLot>(entity =>
            {
                // Relationship với Medication (nullable)
                entity.HasOne(ml => ml.Medication)
                      .WithMany(m => m.Lots)
                      .HasForeignKey(ml => ml.MedicationId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Relationship với VaccinationType (nullable)
                entity.HasOne(ml => ml.VaccineType)
                      .WithMany(vt => vt.MedicationLots)
                      .HasForeignKey(ml => ml.VaccineTypeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ParentMedicationDeliveryDetail configuration
            builder.Entity<ParentMedicationDeliveryDetail>(entity =>
            {
                entity.HasOne(d => d.ParentMedicationDelivery)
                      .WithMany(p => p.Details)
                      .HasForeignKey(d => d.ParentMedicationDeliveryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // MedicationSchedule configuration
            builder.Entity<MedicationSchedule>(entity =>
            {
                entity.HasOne(ms => ms.ParentMedicationDeliveryDetail)
                      .WithMany(d => d.MedicationSchedules)
                      .HasForeignKey(ms => ms.ParentMedicationDeliveryDetailId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // MedicationUsageRecord configuration
            builder.Entity<MedicationUsageRecord>(entity =>
            {
                entity.HasOne(mur => mur.DeliveryDetail)
                      .WithMany(d => d.UsageRecords)
                      .HasForeignKey(mur => mur.DeliveryDetailId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(mur => mur.MedicationSchedule)
                      .WithMany(ms => ms.UsageRecords)
                      .HasForeignKey(mur => mur.MedicationScheduleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(mur => mur.Nurse)
                      .WithMany()
                      .HasForeignKey(mur => mur.CheckedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
        #endregion

        #region Vaccination Management Configuration
        private static void ConfigureVaccinationManagement(ModelBuilder builder)
        {
            // VaccineDoseInfo configuration
            builder.Entity<VaccineDoseInfo>(entity =>
            {
                entity.HasKey(v => v.Id);

                // Relationship với VaccinationType (Many-to-One)
                entity.HasOne(v => v.VaccineType)
                      .WithMany(vt => vt.VaccineDoseInfos)
                      .HasForeignKey(v => v.VaccineTypeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Self-reference relationship (Many-to-One)
                entity.HasOne(v => v.PreviousDose)
                      .WithMany(v => v.NextDoses)
                      .HasForeignKey(v => v.PreviousDoseId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint
                entity.HasIndex(v => new { v.VaccineTypeId, v.DoseNumber })
                      .IsUnique()
                      .HasDatabaseName("IX_VaccineDoseInfos_VaccineTypeId_DoseNumber_Unique");
            });

            // SessionStudent configuration
            builder.Entity<SessionStudent>(entity =>
            {
                entity.HasKey(ss => ss.Id);

                // Relationship với VaccinationSchedule
                entity.HasOne(ss => ss.VaccinationSchedule)
                      .WithMany(vs => vs.SessionStudents)
                      .HasForeignKey(ss => ss.VaccinationScheduleId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationship với Student
                entity.HasOne(ss => ss.Student)
                      .WithMany(s => s.SessionStudents)
                      .HasForeignKey(ss => ss.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint
                entity.HasIndex(ss => new { ss.VaccinationScheduleId, ss.StudentId })
                      .IsUnique()
                      .HasDatabaseName("IX_SessionStudents_ScheduleId_StudentId_Unique");
            });

            builder.Entity<VaccinationRecord>(entity =>
            {
                // Chỉ giữ relationship qua SessionStudent
                entity.HasOne(vr => vr.SessionStudent)
                      .WithMany(ss => ss.VaccinationRecords)
                      .HasForeignKey(vr => vr.SessionStudentId)
                      .OnDelete(DeleteBehavior.Restrict); //  Thay đổi thành Restrict để tránh cascade conflicts
                //entity.HasOne(vr => vr.VaccineLot)
                //      .WithMany(ml => ml.VaccinationRecords)
                //      .HasForeignKey(vr => vr.VaccineLotId)
                //      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(vr => vr.VaccinatedBy)
                      .WithMany()
                      .HasForeignKey(vr => vr.VaccinatedById)
                      .OnDelete(DeleteBehavior.Restrict);             
            });

            builder.Entity<VaccinationSchedule>(entity =>
            {
                // Relationships với Campaign và VaccinationType
                entity.HasOne(vs => vs.Campaign)
                      .WithMany(c => c.Schedules)
                      .HasForeignKey(vs => vs.CampaignId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(vs => vs.VaccinationType)
                      .WithMany(vt => vt.Schedules)   // <-- chỉ rõ nav collection
                      .HasForeignKey(vs => vs.VaccinationTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relationship với SessionStudents
                entity.HasMany(vs => vs.SessionStudents)
                      .WithOne(ss => ss.VaccinationSchedule)
                      .HasForeignKey(ss => ss.VaccinationScheduleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
        #endregion

        #region Medical Supply Management Configuration
        private static void ConfigureMedicalSupplyManagement(ModelBuilder builder)
        {
            // MedicalSupply ↔ MedicalSupplyLot (One-to-Many)
            builder.Entity<MedicalSupply>()
                   .HasMany(ms => ms.Lots)
                   .WithOne(msl => msl.MedicalSupply)
                   .OnDelete(DeleteBehavior.Cascade);

            // MedicalSupplyLot ↔ SupplyUsage (One-to-Many)
            builder.Entity<MedicalSupplyLot>()
                   .HasMany(msl => msl.SupplyUsages)
                   .WithOne(su => su.MedicalSupplyLot)
                   .OnDelete(DeleteBehavior.Restrict);

            // SupplyUsage relationships
            builder.Entity<SupplyUsage>(entity =>
            {
                entity.HasOne(su => su.HealthEvent)
                      .WithMany(he => he.SupplyUsages)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(su => su.UsedByNurse)
                      .WithMany()
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
            // Student indexes
            builder.Entity<Student>()
                   .HasIndex(s => s.StudentCode)
                   .IsUnique()
                   .HasDatabaseName("IX_Students_StudentCode_Unique");

            builder.Entity<Student>()
                   .HasIndex(s => s.ParentUserId)
                   .HasDatabaseName("IX_Students_ParentUserId");

            // Health indexes
            builder.Entity<HealthProfile>()
                   .HasIndex(hp => hp.StudentId)
                   .HasDatabaseName("IX_HealthProfiles_StudentId");

            builder.Entity<HealthEvent>()
                   .HasIndex(he => he.StudentId)
                   .HasDatabaseName("IX_HealthEvents_StudentId");

            // SessionStudent indexes
            builder.Entity<SessionStudent>(entity =>
            {
                entity.HasIndex(ss => ss.VaccinationScheduleId)
                      .HasDatabaseName("IX_SessionStudents_VaccinationScheduleId");

                entity.HasIndex(ss => ss.StudentId)
                      .HasDatabaseName("IX_SessionStudents_StudentId");
            });

            // VaccinationSchedule indexes
            builder.Entity<VaccinationSchedule>(entity =>
            {
                entity.HasIndex(vs => vs.CampaignId)
                      .HasDatabaseName("IX_VaccinationSchedules_CampaignId");

                entity.HasIndex(vs => vs.VaccinationTypeId)
                      .HasDatabaseName("IX_VaccinationSchedules_VaccinationTypeId");
            });

            // VaccinationRecord indexes
            builder.Entity<VaccinationRecord>(entity =>
            {
                entity.HasIndex(vr => vr.SessionStudentId)
                      .HasDatabaseName("IX_VaccinationRecords_SessionStudentId");

                //entity.HasIndex(vr => vr.VaccineLotId)
                //      .HasDatabaseName("IX_VaccinationRecords_VaccineLotId");

                entity.HasIndex(vr => vr.VaccinatedById)
                      .HasDatabaseName("IX_VaccinationRecords_VaccinatedById");
                entity.HasIndex(vr => new { vr.SessionStudentId, vr.AdministeredDate })
                      .HasDatabaseName("IX_VaccinationRecords_SessionStudent_Date");

                entity.HasIndex(vr => vr.AdministeredDate)
                      .HasDatabaseName("IX_VaccinationRecords_AdministeredDate");

                entity.HasIndex(vr => vr.VaccinatedAt)
                      .HasDatabaseName("IX_VaccinationRecords_VaccinatedAt");
            });

            // MedicationLot indexes
            builder.Entity<MedicationLot>(entity =>
            {
                entity.HasIndex(ml => ml.MedicationId)
                      .HasDatabaseName("IX_MedicationLots_MedicationId");

                entity.HasIndex(ml => ml.VaccineTypeId)
                      .HasDatabaseName("IX_MedicationLots_VaccineTypeId");

                entity.HasIndex(ml => ml.LotNumber)
                      .HasDatabaseName("IX_MedicationLots_LotNumber");
            });

            // VaccineDoseInfo indexes
            builder.Entity<VaccineDoseInfo>(entity =>
            {
                entity.HasIndex(v => v.VaccineTypeId)
                      .HasDatabaseName("IX_VaccineDoseInfos_VaccineTypeId");

                entity.HasIndex(v => v.PreviousDoseId)
                      .HasDatabaseName("IX_VaccineDoseInfos_PreviousDoseId");
            });

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
        }
        #endregion

        #region Enum Conversions Configuration
        private static void ConfigureEnumConversions(ModelBuilder builder)
        {
            // Medication enums
            builder.Entity<Medication>()
                .Property(m => m.Category)
                .HasConversion(new EnumToStringConverter<MedicationCategory>())
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Entity<Medication>()
                .Property(m => m.Status)
                .HasConversion(new EnumToStringConverter<MedicationStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // MedicationLot enum
            builder.Entity<MedicationLot>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<LotType>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // CheckupSchedule enum
            builder.Entity<CheckupSchedule>()
                .Property(e => e.ParentConsentStatus)
                .HasConversion(new EnumToStringConverter<CheckupScheduleStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // HealthEvent enums
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
            builder.Entity<HealthEvent>()
                .Property(e => e.Severity)
                .HasConversion(new EnumToStringConverter<SeverityLevel>()) // Chuyển đổi enum SeverityLevel sang chuỗi
                .HasMaxLength(50)
                .IsUnicode(true);

            // HealthProfile enums
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

            // CheckupRecord enums       
            builder.Entity<CheckupRecord>()
                .Property(e => e.Hearing)
                .HasConversion(new EnumToStringConverter<HearingLevel>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // CounselingAppointment enum
            builder.Entity<CounselingAppointment>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<ScheduleStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // FileAttachment enums
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

            // Notification enums
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

            // Parent enum
            builder.Entity<Parent>()
                .Property(e => e.Relationship)
                .HasConversion(new EnumToStringConverter<Relationship>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // ParentMedicationDelivery enum
            builder.Entity<ParentMedicationDelivery>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<StatusMedicationDelivery>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // Report enum
            builder.Entity<Report>()
                .Property(e => e.ReportType)
                .HasConversion(new EnumToStringConverter<ReportType>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // User enum
            builder.Entity<User>()
                .Property(e => e.Gender)
                .HasConversion(new EnumToStringConverter<Gender>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // VaccinationSchedule enum
            builder.Entity<VaccinationSchedule>()
                .Property(e => e.ScheduleStatus)
                .HasConversion(new EnumToStringConverter<ScheduleStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // SessionStudent enums
            builder.Entity<SessionStudent>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<SessionStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Entity<SessionStudent>()
                .Property(e => e.ConsentStatus)
                .HasConversion(new EnumToStringConverter<ParentConsentStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);

            // VaccinationCampaign enum
            builder.Entity<VaccinationCampaign>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<VaccinationCampaignStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);
            builder.Entity<VaccinationRecord>()
                .Property(vr => vr.ReactionSeverity)
                .HasConversion(new EnumToStringConverter<VaccinationReactionSeverity>())
                .HasMaxLength(50)
                .IsUnicode(true);
            builder.Entity<CheckupCampaign>()
                .Property(vr => vr.Status)
                .HasConversion(new EnumToStringConverter<CheckupCampaignStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);
            builder.Entity<CheckupRecord>()
                .Property(vr => vr.Status)
                .HasConversion(new EnumToStringConverter<CheckupRecordStatus>())
                .HasMaxLength(50)
                .IsUnicode(true);
        }
        #endregion
    }
}