using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public static class DBInitializer
    {
        public static async Task Initialize(
            SchoolHealthManagerDbContext context,
            UserManager<User> userManager)
        {
            // A system-wide default Guid for system-initiated operations
            var systemGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

            // Áp dụng migrations tự động
            await context.Database.MigrateAsync();

            #region Seed Roles

            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                    {
                        new Role { Name = "Admin", NormalizedName = "ADMIN", CreatedAt = DateTime.UtcNow, CreatedBy = systemGuid, UpdatedAt = DateTime.UtcNow, UpdatedBy = systemGuid, IsDeleted = false },
                        new Role { Name = "Parent", NormalizedName = "PARENT", CreatedAt = DateTime.UtcNow, CreatedBy = systemGuid, UpdatedAt = DateTime.UtcNow, UpdatedBy = systemGuid, IsDeleted = false },
                        new Role { Name = "SchoolNurse", NormalizedName = "SCHOOLNURSE", CreatedAt = DateTime.UtcNow, CreatedBy = systemGuid, UpdatedAt = DateTime.UtcNow, UpdatedBy = systemGuid, IsDeleted = false },
                        new Role { Name = "Student", NormalizedName = "STUDENT", CreatedAt = DateTime.UtcNow, CreatedBy = systemGuid, UpdatedAt = DateTime.UtcNow, UpdatedBy = systemGuid, IsDeleted = false },
                        new Role { Name = "Manager", NormalizedName = "MANAGER", CreatedAt = DateTime.UtcNow, CreatedBy = systemGuid, UpdatedAt = DateTime.UtcNow, UpdatedBy = systemGuid, IsDeleted = false }
                    };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            const string adminEmail = "tinvtse@gmail.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = systemGuid,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = systemGuid,
                    IsDeleted = false
                };
                const string adminPassword = "Admin@123";
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            #endregion

            #region Seed Parent Users cùng Student tương ứng
            #region Seed Parent Users cùng Student tương ứng
            if (!await context.Parents.AnyAsync())
            {
                // Danh sách phụ huynh và học sinh mẫu
                var seedParents = new List<(string ParentEmail, string ParentFirst, string ParentLast, string StudentFirst, string StudentLast)>
                {
                    ("tinvtse@gmail.com", "Nguyen", "An", "Le", "Minh"),
                    ("parent2@gmail.com", "Tran", "Binh", "Pham", "Hoa")
                };
                    
                foreach (var (email, parentFirst, parentLast, studentFirst, studentLast) in seedParents)
                {
                    // Tạo User cho Parent
                    var parentUser = new User
                    {
                        UserName = email,
                        Email = email,
                        FirstName = parentFirst,
                        LastName = parentLast,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = systemGuid,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = systemGuid,
                        IsDeleted = false
                    };
                    const string parentPassword = "Parent@123";

                    var userResult = await userManager.CreateAsync(parentUser, parentPassword);
                    if (!userResult.Succeeded)
                    {
                        Console.WriteLine($"Failed to create parent user {email}: {string.Join(", ", userResult.Errors)}");
                        continue;
                    }

                    // Gán role Parent
                    await userManager.AddToRoleAsync(parentUser, "Parent");

                    // Tạo Parent trước
                    var parent = new Parent
                    {
                        UserId = parentUser.Id,
                        Relationship = "Other",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = systemGuid,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = systemGuid,
                        IsDeleted = false
                    };
                    await context.Parents.AddAsync(parent);

                    // Lưu trước để có Id của Parent
                    await context.SaveChangesAsync();

                    // Tạo Student tương ứng cho Parent
                    var student = new Student
                    {
                        Id = Guid.NewGuid(),
                        StudentCode = $"HS{DateTime.UtcNow:yyyyMMddHHmmss}",  // Removed 'xx' that might cause formatting issues
                        FirstName = studentFirst,
                        LastName = studentLast,
                        DateOfBirth = DateTime.UtcNow.AddYears(-10),
                        Grade = "1",
                        Section = "1A",
                        ParentUserId = parent.UserId, // Set the required ParentUserId
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = systemGuid,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = systemGuid,
                        IsDeleted = false
                    };
                    await context.Students.AddAsync(student);

                    // Remove the reflection part - it's not needed as StudentId is no longer part of Parent class

                    Console.WriteLine($"Seeded Parent+Student for {email}");
                }

                await context.SaveChangesAsync();
            }
            #endregion
            #endregion

            #region Seed School Nurse Users cùng StaffProfile
            if (!await context.NurseProfiles.AnyAsync())
            {
                var seedNurses = new List<(string Email, string FirstName, string LastName, string Position, string Department)>
                    {
                        ("nurse1@example.com", "Le", "Thi", "School Nurse", "Health Dept"),
                        ("nurse2@example.com", "Pham", "Hoa", "School Nurse", "Health Dept")
                    };

                foreach (var (email, first, last, position, department) in seedNurses)
                {
                    // Tạo User cho School Nurse
                    var nurseUser = new User
                    {
                        UserName = email,
                        Email = email,
                        FirstName = first,
                        LastName = last,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = systemGuid,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = systemGuid,
                        IsDeleted = false
                    };
                    const string nursePassword = "Nurse@123";

                    var nurseResult = await userManager.CreateAsync(nurseUser, nursePassword);
                    if (!nurseResult.Succeeded)
                    {
                        Console.WriteLine($"Failed to create nurse user {email}: {string.Join(", ", nurseResult.Errors)}");
                        continue;
                    }
                    await userManager.AddToRoleAsync(nurseUser, "SchoolNurse");

                    // Tạo StaffProfile tương ứng
                    var profile = new NurseProfile
                    {
                        UserId = nurseUser.Id,
                        Position = position,
                        Department = department,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = systemGuid,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = systemGuid,
                        IsDeleted = false
                    };
                    await context.NurseProfiles.AddAsync(profile);

                    Console.WriteLine($"Seeded School Nurse and StaffProfile for {email}");
                }
                await context.SaveChangesAsync();
            }
            #endregion

            #region Seed Vaccine Types
            if (!await context.VaccineTypes.AnyAsync())
            {
                var now = DateTime.UtcNow;
                var vaccines = new List<VaccineType>
                    {
                        // EPI vaccines
                        new VaccineType { Id = Guid.NewGuid(), Code = "BCG", Name = "Bacillus Calmette–Guérin (BCG)", Group = "EPI", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "HepB", Name = "Hepatitis B", Group = "EPI", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "DPT-HBV-Hib", Name = "Pentavalent (DPT‑HepB‑Hib)", Group = "EPI", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "OPV", Name = "Oral Polio Vaccine (OPV)", Group = "EPI", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "IPV", Name = "Inactivated Polio Vaccine (IPV)", Group = "EPI", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "MMR", Name = "Measles–Mumps–Rubella (MMR)", Group = "EPI", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "JE", Name = "Japanese Encephalitis", Group = "EPI", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        // Supplemental vaccines
                        new VaccineType { Id = Guid.NewGuid(), Code = "PCV", Name = "Pneumococcal Conjugate Vaccine (PCV)", Group = "Supplemental", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "MenACWY", Name = "Meningococcal ACWY Conjugate", Group = "Supplemental", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "MenB", Name = "Meningococcal B", Group = "Supplemental", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "Influenza", Name = "Seasonal Influenza", Group = "Supplemental", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "Varicella", Name = "Chickenpox (Varicella)", Group = "Supplemental", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "HepA", Name = "Hepatitis A", Group = "Supplemental", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "Typhoid", Name = "Typhoid", Group = "Supplemental", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        new VaccineType { Id = Guid.NewGuid(), Code = "COVID19", Name = "COVID-19", Group = "Supplemental", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false },
                        // Booster
                        new VaccineType { Id = Guid.NewGuid(), Code = "Tdap", Name = "Td/Tdap (Booster Diphtheria–Tetanus–Pertussis)", Group = "Booster", IsActive = true, CreatedAt = now, CreatedBy = systemGuid, UpdatedAt = now, UpdatedBy = systemGuid, IsDeleted = false }
                    };

                await context.VaccineTypes.AddRangeAsync(vaccines);
                await context.SaveChangesAsync();
            }
            #endregion
            #region Seed Medical Supplies for Elementary School Health Office
            if (!await context.MedicalSupplies.AnyAsync())
            {
                var medicalSupplies = new List<MedicalSupply>
    {
        // Dụng cụ sơ cứu cơ bản
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Băng gạc vô trùng",
            Unit = "cuộn",
            CurrentStock = 50,
            MinimumStock = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Bông y tế",
            Unit = "gói",
            CurrentStock = 30,
            MinimumStock = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Miếng dán cá nhân (Band-Aid)",
            Unit = "hộp",
            CurrentStock = 25,
            MinimumStock = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Cồn 70%",
            Unit = "chai",
            CurrentStock = 20,
            MinimumStock = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Betadine (Povidone Iodine)",
            Unit = "chai",
            CurrentStock = 15,
            MinimumStock = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },

        // Dụng cụ đo và kiểm tra
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Nhiệt kế điện tử",
            Unit = "cái",
            CurrentStock = 10,
            MinimumStock = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Máy đo huyết áp trẻ em",
            Unit = "cái",
            CurrentStock = 3,
            MinimumStock = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Đèn pin y tế",
            Unit = "cái",
            CurrentStock = 5,
            MinimumStock = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },

        // Thuốc cơ bản (không kê đơn)
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Paracetamol syrup trẻ em",
            Unit = "chai",
            CurrentStock = 12,
            MinimumStock = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Dung dịch ORS (Oresol)",
            Unit = "gói",
            CurrentStock = 20,
            MinimumStock = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Gel rửa tay khô",
            Unit = "chai",
            CurrentStock = 40,
            MinimumStock = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },

        // Dụng cụ bảo hộ
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Găng tay y tế",
            Unit = "hộp",
            CurrentStock = 15,
            MinimumStock = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Khẩu trang y tế",
            Unit = "hộp",
            CurrentStock = 25,
            MinimumStock = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },

        // Dụng cụ khác
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Kéo y tế",
            Unit = "cái",
            CurrentStock = 5,
            MinimumStock = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Nhíp y tế",
            Unit = "cái",
            CurrentStock = 8,
            MinimumStock = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Túi đá lạnh tức thì",
            Unit = "cái",
            CurrentStock = 20,
            MinimumStock = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Bình xịt nước muối sinh lý",
            Unit = "chai",
            CurrentStock = 18,
            MinimumStock = 4,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },

        // Vật tư đặc biệt cho trẻ em
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Miếng dán hạ sốt trẻ em",
            Unit = "hộp",
            CurrentStock = 10,
            MinimumStock = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Kem chống muỗi trẻ em",
            Unit = "tuýp",
            CurrentStock = 15,
            MinimumStock = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Dây đeo thẻ y tế học sinh",
            Unit = "cái",
            CurrentStock = 100,
            MinimumStock = 20,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },

        // Thiết bị đo chiều cao cân nặng
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Thước đo chiều cao trẻ em",
            Unit = "cái",
            CurrentStock = 2,
            MinimumStock = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Cân điện tử y tế",
            Unit = "cái",
            CurrentStock = 2,
            MinimumStock = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },

        // Các vật tư vệ sinh và khử trùng
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Khăn giấy y tế",
            Unit = "hộp",
            CurrentStock = 30,
            MinimumStock = 8,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Dung dịch sát khuẩn tay",
            Unit = "chai",
            CurrentStock = 25,
            MinimumStock = 6,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        },
        new MedicalSupply
        {
            Id = Guid.NewGuid(),
            Name = "Túi rác y tế",
            Unit = "cuộn",
            CurrentStock = 20,
            MinimumStock = 4,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemGuid,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = systemGuid,
            IsDeleted = false
        }
    };

                await context.MedicalSupplies.AddRangeAsync(medicalSupplies);
                await context.SaveChangesAsync();
            }
            #endregion
        }
    }
}
