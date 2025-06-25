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

            foreach (var (email, parentFirst, parentLast, students) in seedData)
            {
                await CreateParentWithStudents(context, userManager, email, parentFirst, parentLast, students);
            }

            await context.SaveChangesAsync();
        }

        private static List<(string Email, string ParentFirst, string ParentLast, List<(string StudentFirst, string StudentLast, string Grade, string Section)> Students)> GetParentStudentSeedData()
        {
            return new List<(string, string, string, List<(string, string, string, string)>)>
    {
        // Grade 1 - 3 students with individual parents
        ("parent.nguyen.anh@gmail.com", "Nguyen", "Van Anh",
            new List<(string, string, string, string)>
            {
                ("Nguyen", "Minh An", "1", "1A")
            }),

        ("parent.tran.hoa@gmail.com", "Tran", "Thi Hoa",
            new List<(string, string, string, string)>
            {
                ("Tran", "Minh Hoa", "1", "1B")
            }),

        ("parent.le.duc@gmail.com", "Le", "Van Duc",
            new List<(string, string, string, string)>
            {
                ("Le", "Thanh Duc", "1", "1A")
            }),

        // Grade 2 - 3 students with individual parents
        ("parent.pham.lan@gmail.com", "Pham", "Thi Lan",
            new List<(string, string, string, string)>
            {
                ("Pham", "Minh Lan", "2", "2A")
            }),

        ("parent.vo.hung@gmail.com", "Vo", "Van Hung",
            new List<(string, string, string, string)>
            {
                ("Vo", "Minh Hung", "2", "2B")
            }),

        ("parent.bui.mai@gmail.com", "Bui", "Thi Mai",
            new List<(string, string, string, string)>
            {
                ("Bui", "Thanh Mai", "2", "2A")
            }),

        // Grade 3 - 3 students with individual parents
        ("parent.hoang.nam@gmail.com", "Hoang", "Van Nam",
            new List<(string, string, string, string)>
            {
                ("Hoang", "Minh Nam", "3", "3A")
            }),

        ("parent.dang.thu@gmail.com", "Dang", "Thi Thu",
            new List<(string, string, string, string)>
            {
                ("Dang", "Thanh Thu", "3", "3B")
            }),

        ("parent.vu.long@gmail.com", "Vu", "Van Long",
            new List<(string, string, string, string)>
            {
                ("Vu", "Minh Long", "3", "3A")
            }),

        // Grade 4 - 3 students with individual parents
        ("parent.do.linh@gmail.com", "Do", "Thi Linh",
            new List<(string, string, string, string)>
            {
                ("Do", "Thanh Linh", "4", "4A")
            }),

        ("parent.ngo.son@gmail.com", "Ngo", "Van Son",
            new List<(string, string, string, string)>
            {
                ("Ngo", "Minh Son", "4", "4B")
            }),

        ("parent.ta.yen@gmail.com", "Ta", "Thi Yen",
            new List<(string, string, string, string)>
            {
                ("Ta", "Thanh Yen", "4", "4A")
            }),

        // Grade 5 - 2 students with individual parents
        ("parent.ly.khoa@gmail.com", "Ly", "Van Khoa",
            new List<(string, string, string, string)>
            {
                ("Ly", "Minh Khoa", "5", "5A")
            }),

        ("parent.cao.nga@gmail.com", "Cao", "Thi Nga",
            new List<(string, string, string, string)>
            {
                ("Cao", "Thanh Nga", "5", "5B")
            }),

        // 1 Parent with 2 children in different grades (Grade 1 and Grade 5)
        ("parent.multi.children@gmail.com", "Truong", "Van Hai",
            new List<(string, string, string, string)>
            {
                ("Truong", "Minh Tuan", "1", "1B"),
                ("Truong", "Minh Chi", "5", "5A")
            })
    };
        }

        private static async Task CreateParentWithStudents(
            SchoolHealthManagerDbContext context,
            UserManager<User> userManager,
            string email,
            string parentFirst,
            string parentLast,
            List<(string StudentFirst, string StudentLast, string Grade, string Section)> students)
        {
            // Create Parent User
            var parentUser = CreateParentUser(email, parentFirst, parentLast);
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

            // Create Students for this parent
            foreach (var (studentFirst, studentLast, grade, section) in students)
            {
                var student = CreateStudent(studentFirst, studentLast, parent.UserId, grade, section);
                await context.Students.AddAsync(student);
            }

            var childrenInfo = string.Join(", ", students.Select(s => $"{s.StudentFirst} {s.StudentLast} (Grade {s.Grade})"));
            Console.WriteLine($"Seeded Parent {email} with children: {childrenInfo}");
        }

        private static Parent CreateParent(Guid userId)
        {
            var now = new DateTime(2025, 6, 24, 12, 53, 29, DateTimeKind.Utc);
            return new Parent
            {
                UserId = userId,
                Relationship = Relationship.Other,
                CreatedAt = now,
                CreatedBy = SystemGuid,
                UpdatedAt = now,
                UpdatedBy = SystemGuid,
                IsDeleted = false
            };
        }

        private static Student CreateStudent(string firstName, string lastName, Guid parentUserId, string grade, string section)
        {
            var now = new DateTime(2025, 6, 24, 12, 53, 29, DateTimeKind.Utc);
            var random = new Random();

            // Generate age based on grade (Grade 1 = ~6-7 years, Grade 5 = ~10-11 years)
            var baseAge = int.Parse(grade) + 5; // Grade 1 = 6 years, Grade 5 = 10 years
            var ageVariation = random.Next(0, 2); // Add 0-1 years variation
            var studentAge = baseAge + ageVariation;

            return new Student
            {
                Id = Guid.NewGuid(),
                StudentCode = GenerateStudentCode(grade, section),
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = now.AddYears(-studentAge).AddDays(random.Next(-180, 180)), // Random within the year
                Grade = grade,
                Section = section,
                ParentUserId = parentUserId,
                CreatedAt = now,
                CreatedBy = SystemGuid,
                UpdatedAt = now,
                UpdatedBy = SystemGuid,
                IsDeleted = false
            };
        }

        private static string GenerateStudentCode(string grade, string section)
        {
            var now = new DateTime(2025, 6, 24, 12, 53, 29, DateTimeKind.Utc);
            var random = new Random();
            var randomNumber = random.Next(100, 999);

            // Format: HS + Grade + Section + Year + RandomNumber
            // Example: HS1A2025001, HS2B2025002
            return $"HS{grade}{section.Replace(" ", "")}{now.Year}{randomNumber:D3}";
        }

        private static User CreateParentUser(string email, string firstName, string lastName)
        {
            var now = new DateTime(2025, 6, 24, 12, 53, 29, DateTimeKind.Utc);

            return new User
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                Gender = Gender.Other, // Default value
                IsFirstLogin = true,
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
        ("nurse1@example.com", "Le", "Thi Linh", "School Nurse", "Health Department"),
        ("nurse2@example.com", "Pham", "Van Hoa", "Senior School Nurse", "Health Department"),
        ("nurse3@example.com", "Nguyen", "Thi Mai", "Head Nurse", "Health Department")
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
            var nurseUser = CreateNurseUser(email, first, last);
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

            Console.WriteLine($"Seeded School Nurse and Profile for {email} - {first} {last}");
        }

        private static NurseProfile CreateNurseProfile(Guid userId, string position, string department)
        {
            var now = new DateTime(2025, 6, 24, 12, 53, 29, DateTimeKind.Utc);
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

        private static User CreateNurseUser(string email, string firstName, string lastName)
        {
            var now = new DateTime(2025, 6, 24, 12, 53, 29, DateTimeKind.Utc);

            return new User
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                Gender = Gender.Other, // Default value
                IsFirstLogin = true,
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
            medications.AddRange(CreateSolutionAndVitaminMedications());

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
        CreateMedication(
            name: "Morphin (Chlohydrat)",
            unit: "Ống tiêm",
            dosageForm: "Tiêm – ống 10 mg/ml",
            category: MedicationCategory.Emergency
        ),
        CreateMedication(
            name: "Adrenalin",
            unit: "Ống tiêm",
            dosageForm: "Tiêm – ống 1 mg/ml",
            category: MedicationCategory.Emergency
        ),
        CreateMedication(
            name: "Diazepam",
            unit: "Ống tiêm",
            dosageForm: "Tiêm – ống 10 mg/ml",
            category: MedicationCategory.Emergency
        ),
        CreateMedication(
            name: "Atropin",
            unit: "Ống tiêm",
            dosageForm: "Tiêm – ống 0,5 mg/ml hoặc 1 mg/ml",
            category: MedicationCategory.Emergency
        )
    };
        }

        private static List<Medication> CreatePainReliefMedications()
        {
            return new List<Medication>
    {
        CreateMedication(
            name: "Paracetamol",
            unit: "Viên nén",
            dosageForm: "Uống – viên 500 mg",
            category: MedicationCategory.PainRelief
        ),
        CreateMedication(
            name: "Ibuprofen",
            unit: "Viên nén",
            dosageForm: "Uống – viên 200 mg hoặc 400 mg",
            category: MedicationCategory.PainRelief
        ),
        CreateMedication(
            name: "Diclofenac (Voltaren)",
            unit: "Ống tiêm",
            dosageForm: "Tiêm – ống 75 mg bột pha tiêm",
            category: MedicationCategory.PainRelief
        ),
        CreateMedication(
            name: "Piroxicam",
            unit: "Viên nén",
            dosageForm: "Uống – viên 10 mg, 20 mg",
            category: MedicationCategory.PainRelief
        ),
        CreateMedication(
            name: "Naproxen",
            unit: "Viên nén",
            dosageForm: "Uống – viên 250 mg",
            category: MedicationCategory.PainRelief
        )
    };
        }

        private static List<Medication> CreateAntiAllergyMedications()
        {
            return new List<Medication>
    {
        CreateMedication(
            name: "Chlorpheniramin",
            unit: "Viên nén",
            dosageForm: "Uống – viên 4 mg (dạng chlorpheniramine maleate)",
            category: MedicationCategory.AntiAllergy
        ),
        CreateMedication(
            name: "Diphenhydramin",
            unit: "Viên nén",
            dosageForm: "Uống – viên 25 mg (hay 50 mg)",
            category: MedicationCategory.AntiAllergy
        )
    };
        }

        private static List<Medication> CreateAntibiotics()
        {
            return new List<Medication>
    {
        CreateMedication(
            name: "Amoxicillin",
            unit: "Viên nén",
            dosageForm: "Uống – viên 250 mg, 500 mg",
            category: MedicationCategory.Antibiotic
        ),
        CreateMedication(
            name: "Cefaclor",
            unit: "Viên nén",
            dosageForm: "Uống – viên 250 mg, 500 mg",
            category: MedicationCategory.Antibiotic
        ),
        CreateMedication(
            name: "Cloxacillin",
            unit: "Viên nén",
            dosageForm: "Uống – viên 250 mg",
            category: MedicationCategory.Antibiotic
        ),
        CreateMedication(
            name: "Benzathin benzylpenicilin",
            unit: "Lọ bột pha tiêm",
            dosageForm: "Tiêm – lọ 600.000 IU, 1.200.000 IU, 2.400.000 IU",
            category: MedicationCategory.Antibiotic
        )
    };
        }

        private static List<Medication> CreateTopicalTreatments()
        {
            return new List<Medication>
    {
        CreateMedication(
            name: "Benzyl benzoat",
            unit: "Chai/Nắp",
            dosageForm: "Dùng ngoài – dung dịch",
            category: MedicationCategory.TopicalTreatment
        ),
        CreateMedication(
            name: "Fluocinolol",
            unit: "Tuýp 5 g, 15 g",
            dosageForm: "Dùng ngoài – mỡ 0,025 %",
            category: MedicationCategory.TopicalTreatment
        ),
        CreateMedication(
            name: "Miconazol",
            unit: "Tuýp 10 g",
            dosageForm: "Dùng ngoài – kem 2 %, tuýp 10 g",
            category: MedicationCategory.TopicalTreatment
        ),
        CreateMedication(
            name: "Panthenol (Dexpanthenol)",
            unit: "Bình xịt",
            dosageForm: "Dạng xịt bọt",
            category: MedicationCategory.TopicalTreatment
        )
    };
        }

        private static List<Medication> CreateDisinfectants()
        {
            return new List<Medication>
    {
        CreateMedication(
            name: "Cồn 70° (Alcohol 70°)",
            unit: "Chai 60 ml",
            dosageForm: "Dùng ngoài – lọ 60 ml",
            category: MedicationCategory.Disinfectant
        ),
        CreateMedication(
            name: "Cồn iod",
            unit: "Chai 15 ml",
            dosageForm: "Dùng ngoài – dung dịch 2,5 % – lọ 15 ml",
            category: MedicationCategory.Disinfectant
        ),
        CreateMedication(
            name: "Nước oxy già (H₂O₂ 3%)",
            unit: "Chai 15 ml, 60 ml",
            dosageForm: "Dùng ngoài – dung dịch 3 % – lọ 15 ml, 60 ml",
            category: MedicationCategory.Disinfectant
        ),
        CreateMedication(
            name: "Povidon–iod (Betadine)",
            unit: "Chai 10 ml",
            dosageForm: "Dùng ngoài – dung dịch 10 % – chai 10 ml",
            category: MedicationCategory.Disinfectant
        )
    };
        }

        private static List<Medication> CreateSolutionAndVitaminMedications()
        {
            return new List<Medication>
    {
        CreateMedication(
            name: "Dung dịch Glucose (5% / 30%)",
            unit: "Ống tiêm",
            dosageForm: "Tiêm – ống 20 ml (5 % và 30 %)",
            category: MedicationCategory.SolutionAndVitamin
        ),
        CreateMedication(
            name: "Ringer lactat",
            unit: "Chai 250 ml, 500 ml",
            dosageForm: "Tiêm truyền – chai 250 ml, 500 ml",
            category: MedicationCategory.SolutionAndVitamin
        ),
        CreateMedication(
            name: "Natri clorid (NaCl 0,9%)",
            unit: "Chai 500 ml",
            dosageForm: "Tiêm truyền – chai 500 ml",
            category: MedicationCategory.SolutionAndVitamin
        ),
        CreateMedication(
            name: "Vitamin C",
            unit: "Viên nén",
            dosageForm: "Uống – viên 100 mg",
            category: MedicationCategory.SolutionAndVitamin
        ),
        CreateMedication(
            name: "Vitamin PP (Niacinamide)",
            unit: "Viên nén",
            dosageForm: "Uống – viên 50 mg",
            category: MedicationCategory.SolutionAndVitamin
        ),
        CreateMedication(
            name: "Acid folic",
            unit: "Viên nén",
            dosageForm: "Uống – viên 1 mg, 5 mg",
            category: MedicationCategory.SolutionAndVitamin
        ),
        CreateMedication(
            name: "Cyanocobalamin (Vitamin B₁₂)",
            unit: "Ống tiêm",
            dosageForm: "Tiêm – ống 1 mg",
            category: MedicationCategory.SolutionAndVitamin
        )
    };
        }

        private static List<Medication> CreateDigestiveMedications()
        {
            return new List<Medication>
    {
        CreateMedication(
            name: "ORS (Dung dịch bù điện giải)",
            unit: "Gói pha ORS",
            dosageForm: "Uống – gói pha với 200 ml nước đun sôi để nguội",
            category: MedicationCategory.Digestive
        ),
        CreateMedication(
            name: "Smecta (Diosmectite)",
            unit: "Gói 3 g",
            dosageForm: "Uống – gói bột 3 g",
            category: MedicationCategory.Digestive
        ),
        CreateMedication(
            name: "Loperamide",
            unit: "Viên nén",
            dosageForm: "Uống – viên 2 mg",
            category: MedicationCategory.Digestive
        )
    };
        }

        private static List<Medication> CreateENTMedications()
        {
            return new List<Medication>
    {
        CreateMedication(
            name: "Chloramphenicol",
            unit: "Lọ nhỏ 5 ml",
            dosageForm: "Nhỏ mắt – dung dịch 1 %",
            category: MedicationCategory.ENT
        ),
        CreateMedication(
            name: "Neomycin – dexamethasone",
            unit: "Lọ nhỏ 5 ml",
            dosageForm: "Nhỏ mắt – dung dịch",
            category: MedicationCategory.ENT
        ),
        CreateMedication(
            name: "Xylometazolin",
            unit: "Lọ nhỏ 10 ml",
            dosageForm: "Nhỏ mũi – dung dịch 0,05 %",
            category: MedicationCategory.ENT
        ),
        CreateMedication(
            name: "Neo–Penotran (Neomycin + Nystatin + Polymyxin B)",
            unit: "Lọ nhỏ 5 ml",
            dosageForm: "Nhỏ mắt – dung dịch",
            category: MedicationCategory.ENT
        )
    };
        }

        private static List<Medication> CreateRespiratoryMedications()
        {
            return new List<Medication>
    {
        CreateMedication(
            name: "Salbutamol (Ventolin)",
            unit: "Bình hít",
            dosageForm: "Hít – bình xịt 100 mcg",
            category: MedicationCategory.Respiratory
        ),
        CreateMedication(
            name: "Ambroxol (Lazolvan)",
            unit: "Viên nén hoặc chai siro",
            dosageForm: "Uống – viên 30 mg, siro 15 mg/5 ml",
            category: MedicationCategory.Respiratory
        ),
        CreateMedication(
            name: "Loratadin",
            unit: "Viên nén",
            dosageForm: "Uống – viên 10 mg",
            category: MedicationCategory.Respiratory
        )
    };
        }

        private static Medication CreateMedication(
            string name,
            string unit,
            string dosageForm,
            MedicationCategory category
        )
        {
            var now = DateTime.UtcNow;
            return new Medication
            {
                Id = Guid.NewGuid(),
                Name = name,
                Unit = unit,
                DosageForm = dosageForm,

                // Gán enum Category và mặc định Status = Active
                Category = category,
                Status = MedicationStatus.Active,

                CreatedAt = now,
                CreatedBy = SystemGuid,
                UpdatedAt = now,
                UpdatedBy = SystemGuid,
                IsDeleted = false
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