using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public static class DBInitializer
    {
        private static readonly Guid SystemGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public static async Task Initialize(
            SchoolHealthManagerDbContext context,
            UserManager<User> userManager)
        {
            // Apply migrations automatically
            await context.Database.MigrateAsync();

            // Seed data in proper order
            await SeedRoles(context);
            await SeedAdminUser(context, userManager);
            await SeedParentsAndStudents(context, userManager);
            await SeedSchoolNurses(context, userManager);
            await SeedVaccineTypes(context);
            await SeedMedicalSupplies(context);
            await SeedMedications(context);
        }

        #region Role Seeding
        private static async Task SeedRoles(SchoolHealthManagerDbContext context)
        {
            if (await context.Roles.AnyAsync()) return;

            var roles = GetRoles();
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        private static List<Role> GetRoles()
        {
            var now = DateTime.UtcNow;
            return new List<Role>
            {
                CreateRole("Admin", "ADMIN", now),
                CreateRole("Parent", "PARENT", now),
                CreateRole("SchoolNurse", "SCHOOLNURSE", now),
                CreateRole("Student", "STUDENT", now),
                CreateRole("Manager", "MANAGER", now)
            };
        }

        private static Role CreateRole(string name, string normalizedName, DateTime now)
        {
            return new Role
            {
                Name = name,
                NormalizedName = normalizedName,
                CreatedAt = now,
                CreatedBy = SystemGuid,
                UpdatedAt = now,
                UpdatedBy = SystemGuid,
                IsDeleted = false
            };
        }
        #endregion

        #region Admin User Seeding
        private static async Task SeedAdminUser(SchoolHealthManagerDbContext context, UserManager<User> userManager)
        {
            const string adminEmail = "tinvtse@gmail.com";
            if (await userManager.FindByEmailAsync(adminEmail) != null) return;

            var adminUser = CreateUser(adminEmail, "System", "Admin");
            const string adminPassword = "Admin@123";

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        #endregion

        #region Parent and Student Seeding
        private static async Task SeedParentsAndStudents(SchoolHealthManagerDbContext context, UserManager<User> userManager)
        {
            if (await context.Parents.AnyAsync()) return;

            var seedData = GetParentStudentSeedData();

            foreach (var (email, parentFirst, parentLast, studentFirst, studentLast) in seedData)
            {
                await CreateParentWithStudent(context, userManager, email, parentFirst, parentLast, studentFirst, studentLast);
            }

            await context.SaveChangesAsync();
        }

        private static List<(string Email, string ParentFirst, string ParentLast, string StudentFirst, string StudentLast)> GetParentStudentSeedData()
        {
            return new List<(string, string, string, string, string)>
            {
                ("tinvtse@gmail.com", "Nguyen", "An", "Le", "Minh"),
                ("parent2@gmail.com", "Tran", "Binh", "Pham", "Hoa")
            };
        }

        private static async Task CreateParentWithStudent(
            SchoolHealthManagerDbContext context,
            UserManager<User> userManager,
            string email,
            string parentFirst,
            string parentLast,
            string studentFirst,
            string studentLast)
        {
            // Create Parent User
            var parentUser = CreateUser(email, parentFirst, parentLast);
            const string parentPassword = "Parent@123";

            var userResult = await userManager.CreateAsync(parentUser, parentPassword);
            if (!userResult.Succeeded)
            {
                Console.WriteLine($"Failed to create parent user {email}: {string.Join(", ", userResult.Errors)}");
                return;
            }

            await userManager.AddToRoleAsync(parentUser, "Parent");

            // Create Parent profile
            var parent = CreateParent(parentUser.Id);
            await context.Parents.AddAsync(parent);
            await context.SaveChangesAsync();

            // Create Student
            var student = CreateStudent(studentFirst, studentLast, parent.UserId);
            await context.Students.AddAsync(student);

            Console.WriteLine($"Seeded Parent+Student for {email}");
        }

        private static Parent CreateParent(Guid userId)
        {
            var now = DateTime.UtcNow;
            return new Parent
            {
                UserId = userId,
                Relationship = "Other",
                CreatedAt = now,
                CreatedBy = SystemGuid,
                UpdatedAt = now,
                UpdatedBy = SystemGuid,
                IsDeleted = false
            };
        }

        private static Student CreateStudent(string firstName, string lastName, Guid parentUserId)
        {
            var now = DateTime.UtcNow;
            return new Student
            {
                Id = Guid.NewGuid(),
                StudentCode = $"HS{now:yyyyMMddHHmmss}",
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = now.AddYears(-10),
                Grade = "1",
                Section = "1A",
                ParentUserId = parentUserId,
                CreatedAt = now,
                CreatedBy = SystemGuid,
                UpdatedAt = now,
                UpdatedBy = SystemGuid,
                IsDeleted = false
            };
        }
        #endregion

        #region School Nurse Seeding
        private static async Task SeedSchoolNurses(SchoolHealthManagerDbContext context, UserManager<User> userManager)
        {
            if (await context.NurseProfiles.AnyAsync()) return;

            var seedNurses = GetNurseSeedData();

            foreach (var (email, first, last, position, department) in seedNurses)
            {
                await CreateNurseWithProfile(context, userManager, email, first, last, position, department);
            }

            await context.SaveChangesAsync();
        }

        private static List<(string Email, string FirstName, string LastName, string Position, string Department)> GetNurseSeedData()
        {
            return new List<(string, string, string, string, string)>
            {
                ("nurse1@example.com", "Le", "Thi", "School Nurse", "Health Dept"),
                ("nurse2@example.com", "Pham", "Hoa", "School Nurse", "Health Dept")
            };
        }

        private static async Task CreateNurseWithProfile(
            SchoolHealthManagerDbContext context,
            UserManager<User> userManager,
            string email,
            string first,
            string last,
            string position,
            string department)
        {
            var nurseUser = CreateUser(email, first, last);
            const string nursePassword = "Nurse@123";

            var nurseResult = await userManager.CreateAsync(nurseUser, nursePassword);
            if (!nurseResult.Succeeded)
            {
                Console.WriteLine($"Failed to create nurse user {email}: {string.Join(", ", nurseResult.Errors)}");
                return;
            }

            await userManager.AddToRoleAsync(nurseUser, "SchoolNurse");

            var profile = CreateNurseProfile(nurseUser.Id, position, department);
            await context.NurseProfiles.AddAsync(profile);

            Console.WriteLine($"Seeded School Nurse and StaffProfile for {email}");
        }

        private static NurseProfile CreateNurseProfile(Guid userId, string position, string department)
        {
            var now = DateTime.UtcNow;
            return new NurseProfile
            {
                UserId = userId,
                Position = position,
                Department = department,
                CreatedAt = now,
                CreatedBy = SystemGuid,
                UpdatedAt = now,
                UpdatedBy = SystemGuid,
                IsDeleted = false
            };
        }
        #endregion

        #region Vaccine Types Seeding
        private static async Task SeedVaccineTypes(SchoolHealthManagerDbContext context)
        {
            if (await context.VaccinationTypes.AnyAsync()) return;

            var vaccines = GetVaccineTypes();
            await context.VaccinationTypes.AddRangeAsync(vaccines);
            await context.SaveChangesAsync();
        }

        private static List<VaccinationType> GetVaccineTypes()
        {
            var now = DateTime.UtcNow;
            var vaccines = new List<VaccinationType>();

            // EPI vaccines
            vaccines.AddRange(CreateEpiVaccines(now));

            // Supplemental vaccines
            vaccines.AddRange(CreateSupplementalVaccines(now));

            // Booster vaccines
            vaccines.AddRange(CreateBoosterVaccines(now));

            return vaccines;
        }

        private static List<VaccinationType> CreateEpiVaccines(DateTime now)
        {
            return new List<VaccinationType>
            {
                CreateVaccineType("BCG", "Bacillus Calmette–Guérin (BCG)", "EPI", now),
                CreateVaccineType("HepB", "Hepatitis B", "EPI", now),
                CreateVaccineType("DPT-HBV-Hib", "Pentavalent (DPT‑HepB‑Hib)", "EPI", now),
                CreateVaccineType("OPV", "Oral Polio Vaccine (OPV)", "EPI", now),
                CreateVaccineType("IPV", "Inactivated Polio Vaccine (IPV)", "EPI", now),
                CreateVaccineType("MMR", "Measles–Mumps–Rubella (MMR)", "EPI", now),
                CreateVaccineType("JE", "Japanese Encephalitis", "EPI", now)
            };
        }

        private static List<VaccinationType> CreateSupplementalVaccines(DateTime now)
        {
            return new List<VaccinationType>
            {
                CreateVaccineType("PCV", "Pneumococcal Conjugate Vaccine (PCV)", "Supplemental", now),
                CreateVaccineType("MenACWY", "Meningococcal ACWY Conjugate", "Supplemental", now),
                CreateVaccineType("MenB", "Meningococcal B", "Supplemental", now),
                CreateVaccineType("Influenza", "Seasonal Influenza", "Supplemental", now),
                CreateVaccineType("Varicella", "Chickenpox (Varicella)", "Supplemental", now),
                CreateVaccineType("HepA", "Hepatitis A", "Supplemental", now),
                CreateVaccineType("Typhoid", "Typhoid", "Supplemental", now),
                CreateVaccineType("COVID19", "COVID-19", "Supplemental", now)
            };
        }

        private static List<VaccinationType> CreateBoosterVaccines(DateTime now)
        {
            return new List<VaccinationType>
            {
                CreateVaccineType("Tdap", "Td/Tdap (Booster Diphtheria–Tetanus–Pertussis)", "Booster", now)
            };
        }

        private static VaccinationType CreateVaccineType(string code, string name, string group, DateTime now)
        {
            return new VaccinationType
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                Group = group,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = SystemGuid,
                UpdatedAt = now,
                UpdatedBy = SystemGuid,
                IsDeleted = false
            };
        }
        #endregion

        #region Medical Supplies Seeding
        private static async Task SeedMedicalSupplies(SchoolHealthManagerDbContext context)
        {
            if (await context.MedicalSupplies.AnyAsync()) return;

            var medicalSupplies = GetMedicalSupplies();
            await context.MedicalSupplies.AddRangeAsync(medicalSupplies);
            await context.SaveChangesAsync();
        }

        private static List<MedicalSupply> GetMedicalSupplies()
        {
            var supplies = new List<MedicalSupply>();

            // Basic equipment
            supplies.AddRange(CreateBasicEquipment());

            // Consumable medical supplies
            supplies.AddRange(CreateConsumableSupplies());

            // Additional school-specific supplies
            supplies.AddRange(CreateSchoolSpecificSupplies());

            return supplies;
        }

        private static List<MedicalSupply> CreateBasicEquipment()
        {
            return new List<MedicalSupply>
            {
                CreateMedicalSupply("Giường bệnh nhân", "cái", 3, 1),
                CreateMedicalSupply("Tủ đầu giường", "cái", 3, 1),
                CreateMedicalSupply("Bàn khám bệnh", "cái", 1, 1),
                CreateMedicalSupply("Đèn khám bệnh", "cái", 2, 1),
                CreateMedicalSupply("Huyết áp kế người lớn và trẻ em", "cái", 2, 1),
                CreateMedicalSupply("Ống nghe bệnh", "cái", 2, 1),
                CreateMedicalSupply("Nhiệt kế y học 42ºC", "cái", 5, 2),
                CreateMedicalSupply("Cân trọng lượng 120kg có thước đo chiều cao", "cái", 1, 1),
                CreateMedicalSupply("Thước dây 1,5 mét", "cái", 1, 1),
                CreateMedicalSupply("Bàn để dụng cụ", "cái", 2, 1)
            };
        }

        private static List<MedicalSupply> CreateConsumableSupplies()
        {
            return new List<MedicalSupply>
            {
                CreateMedicalSupply("Bông gòn", "gói", 20, 5),
                CreateMedicalSupply("Gạc cá nhân", "gói", 20, 5),
                CreateMedicalSupply("Băng gạc vô trùng", "gói", 10, 3),
                CreateMedicalSupply("Băng keo y tế", "cuộn", 10, 3),
                CreateMedicalSupply("Găng tay y tế (hộp)", "hộp", 2, 1),
                CreateMedicalSupply("Khẩu trang y tế", "hộp", 2, 1),
                CreateMedicalSupply("Cồn 70% sát khuẩn", "chai", 5, 2),
                CreateMedicalSupply("Betadine (Povidone Iodine)", "chai", 15, 3),
                CreateMedicalSupply("Miếng dán cá nhân (Band-Aid)", "hộp", 25, 5)
            };
        }

        private static List<MedicalSupply> CreateSchoolSpecificSupplies()
        {
            return new List<MedicalSupply>
            {
                CreateMedicalSupply("Paracetamol syrup trẻ em", "chai", 12, 3),
                CreateMedicalSupply("Dung dịch ORS (Oresol)", "gói", 20, 5),
                CreateMedicalSupply("Gel rửa tay khô", "chai", 40, 10),
                CreateMedicalSupply("Túi đá lạnh tức thì", "cái", 20, 5),
                CreateMedicalSupply("Bình xịt nước muối sinh lý", "chai", 18, 4),
                CreateMedicalSupply("Miếng dán hạ sốt trẻ em", "hộp", 10, 2),
                CreateMedicalSupply("Kem chống muỗi trẻ em", "tuýp", 15, 3),
                CreateMedicalSupply("Nhiệt kế điện tử", "cái", 10, 2),
                CreateMedicalSupply("Dung dịch sát khuẩn tay", "chai", 25, 6),
                CreateMedicalSupply("Túi rác y tế", "cuộn", 20, 4)
            };
        }

        private static MedicalSupply CreateMedicalSupply(string name, string unit, int currentStock, int minimumStock)
        {
            var now = DateTime.UtcNow;
            return new MedicalSupply
            {
                Id = Guid.NewGuid(),
                Name = name,
                Unit = unit,
                CurrentStock = currentStock,
                MinimumStock = minimumStock,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = SystemGuid,
                UpdatedAt = now,
                UpdatedBy = SystemGuid,
                IsDeleted = false
            };
        }
        #endregion

        #region Medications Seeding
        private static async Task SeedMedications(SchoolHealthManagerDbContext context)
        {
            if (await context.Medications.AnyAsync()) return;

            var medications = GetMedications();
            await context.Medications.AddRangeAsync(medications);
            await context.SaveChangesAsync();
        }

        private static List<Medication> GetMedications()
        {
            var medications = new List<Medication>();

            // Emergency medications
            medications.AddRange(CreateEmergencyMedications());

            // Pain relief and fever medications
            medications.AddRange(CreatePainReliefMedications());

            // Anti-allergy medications
            medications.AddRange(CreateAntiAllergyMedications());

            // Antibiotics
            medications.AddRange(CreateAntibiotics());

            // Topical treatments
            medications.AddRange(CreateTopicalTreatments());

            // Disinfectants
            medications.AddRange(CreateDisinfectants());

            // Solutions and vitamins
            medications.AddRange(CreateSolutionsAndVitamins());

            // Digestive medications
            medications.AddRange(CreateDigestiveMedications());

            // ENT medications
            medications.AddRange(CreateENTMedications());

            // Respiratory medications
            medications.AddRange(CreateRespiratoryMedications());

            return medications;
        }

        private static List<Medication> CreateEmergencyMedications()
        {
            return new List<Medication>
            {
                CreateMedication("Morphin (Chlohydrat)", "Ống tiêm", "Tiêm – ống 10 mg/ml"),
                CreateMedication("Adrenalin", "Ống tiêm", "Tiêm – ống 1 mg/ml"),
                CreateMedication("Diazepam", "Ống tiêm", "Tiêm – ống 10 mg/ml"),
                CreateMedication("Atropin", "Ống tiêm", "Tiêm – ống 0,5 mg/ml hoặc 1 mg/ml")
            };
        }

        private static List<Medication> CreatePainReliefMedications()
        {
            return new List<Medication>
            {
                CreateMedication("Paracetamol", "Viên nén", "Uống – viên 500 mg"),
                CreateMedication("Ibuprofen", "Viên nén", "Uống – viên 200 mg hoặc 400 mg"),
                CreateMedication("Diclofenac (Voltaren)", "Ống tiêm", "Tiêm – ống 75 mg bột pha tiêm"),
                CreateMedication("Piroxicam", "Viên nén", "Uống – viên 10 mg, 20 mg"),
                CreateMedication("Naproxen", "Viên nén", "Uống – viên 250 mg")
            };
        }

        private static List<Medication> CreateAntiAllergyMedications()
        {
            return new List<Medication>
            {
                CreateMedication("Chlorpheniramin", "Viên nén", "Uống – viên 4 mg (dạng chlorpheniramine maleate)"),
                CreateMedication("Diphenhydramin", "Viên nén", "Uống – viên 25 mg (hay 50 mg)")
            };
        }

        private static List<Medication> CreateAntibiotics()
        {
            return new List<Medication>
            {
                CreateMedication("Amoxicillin", "Viên nén", "Uống – viên 250 mg, 500 mg"),
                CreateMedication("Cefaclor", "Viên nén", "Uống – viên 250 mg, 500 mg"),
                CreateMedication("Cloxacillin", "Viên nén", "Uống – viên 250 mg"),
                CreateMedication("Benzathin benzylpenicilin", "Lọ bột pha tiêm", "Tiêm – lọ 600.000 IU, 1.200.000 IU, 2.400.000 IU")
            };
        }

        private static List<Medication> CreateTopicalTreatments()
        {
            return new List<Medication>
            {
                CreateMedication("Benzyl benzoat", "Chai/Nắp", "Dùng ngoài – dung dịch"),
                CreateMedication("Fluocinolol", "Tuýp 5 g, 15 g", "Dùng ngoài – mỡ 0,025 %"),
                CreateMedication("Miconazol", "Tuýp 10 g", "Dùng ngoài – kem 2 %, tuýp 10 g"),
                CreateMedication("Panthenol (Dexpanthenol)", "Bình xịt", "Dạng xịt bọt")
            };
        }

        private static List<Medication> CreateDisinfectants()
        {
            return new List<Medication>
            {
                CreateMedication("Cồn 70° (Alcohol 70°)", "Chai 60 ml", "Dùng ngoài – lọ 60 ml"),
                CreateMedication("Cồn iod", "Chai 15 ml", "Dùng ngoài – dung dịch 2,5 % – lọ 15 ml"),
                CreateMedication("Nước oxy già (H₂O₂ 3%)", "Chai 15 ml, 60 ml", "Dùng ngoài – dung dịch 3 % – lọ 15 ml, 60 ml"),
                CreateMedication("Povidon–iod (Betadine)", "Chai 10 ml", "Dùng ngoài – dung dịch 10 % – chai 10 ml")
            };
        }

        private static List<Medication> CreateSolutionsAndVitamins()
        {
            return new List<Medication>
            {
                CreateMedication("Dung dịch Glucose (5% / 30%)", "Ống tiêm", "Tiêm – ống 20 ml (5 % và 30 %)"),
                CreateMedication("Ringer lactat", "Chai 250 ml, 500 ml", "Tiêm truyền – chai 250 ml, 500 ml"),
                CreateMedication("Natri clorid (NaCl 0,9%)", "Chai 500 ml", "Tiêm truyền – chai 500 ml"),
                CreateMedication("Vitamin C", "Viên nén", "Uống – viên 100 mg"),
                CreateMedication("Vitamin PP (Niacinamide)", "Viên nén", "Uống – viên 50 mg"),
                CreateMedication("Acid folic", "Viên nén", "Uống – viên 1 mg, 5 mg"),
                CreateMedication("Cyanocobalamin (Vitamin B₁₂)", "Ống tiêm", "Tiêm – ống 1 mg")
            };
        }

        private static List<Medication> CreateDigestiveMedications()
        {
            return new List<Medication>
            {
                CreateMedication("ORS (Dung dịch bù điện giải)", "Gói pha ORS", "Uống – gói pha với 200 ml nước đun sôi để nguội"),
                CreateMedication("Smecta (Diosmectite)", "Gói 3 g", "Uống – gói bột 3 g"),
                CreateMedication("Loperamide", "Viên nén", "Uống – viên 2 mg")
            };
        }

        private static List<Medication> CreateENTMedications()
        {
            return new List<Medication>
            {
                CreateMedication("Chloramphenicol", "Lọ nhỏ 5 ml", "Nhỏ mắt – dung dịch 1 %"),
                CreateMedication("Neomycin – dexamethasone", "Lọ nhỏ 5 ml", "Nhỏ mắt – dung dịch"),
                CreateMedication("Xylometazolin", "Lọ nhỏ 10 ml", "Nhỏ mũi – dung dịch 0,05 %"),
                CreateMedication("Neo–Penotran (Neomycin + Nystatin + Polymyxin B)", "Lọ nhỏ 5 ml", "Nhỏ mắt – dung dịch")
            };
        }

        private static List<Medication> CreateRespiratoryMedications()
        {
            return new List<Medication>
            {
                CreateMedication("Salbutamol (Ventolin)", "Bình hít", "Hít – bình xịt 100 mcg"),
                CreateMedication("Ambroxol (Lazolvan)", "Viên nén hoặc chai siro", "Uống – viên 30 mg, siro 15 mg/5 ml"),
                CreateMedication("Loratadin", "Viên nén", "Uống – viên 10 mg")
            };
        }

        private static Medication CreateMedication(string name, string unit, string dosageForm)
        {
            return new Medication
            {
                Id = Guid.NewGuid(),
                Name = name,
                Unit = unit,
                DosageForm = dosageForm
            };
        }
        #endregion

        #region Helper Methods
        private static User CreateUser(string email, string firstName, string lastName)
        {
            var now = DateTime.UtcNow;
            return new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                CreatedAt = now,
                CreatedBy = SystemGuid,
                UpdatedAt = now,
                UpdatedBy = SystemGuid,
                IsDeleted = false
            };
        }
        #endregion
    }
}