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

                    // Tạo Student tương ứng cho Parent
                    var student = new Student
                    {
                        Id = Guid.NewGuid(),
                        StudentCode = $"HS{DateTime.UtcNow:yyyyMMddHHmmssxx}",
                        FirstName = studentFirst,
                        LastName = studentLast,
                        DateOfBirth = DateTime.UtcNow.AddYears(-10),
                        Grade = "1",
                        Section = "1A",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = systemGuid,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = systemGuid,
                        IsDeleted = false
                    };
                    await context.Students.AddAsync(student);

                    // Tạo bản ghi Parent liên kết
                    var parent = new Parent
                    {
                        UserId = parentUser.Id,
                        StudentId = student.Id,
                        Relationship = "Other",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = systemGuid,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = systemGuid,
                        IsDeleted = false
                    };
                    await context.Parents.AddAsync(parent);

                    Console.WriteLine($"Seeded Parent+Student for {email}");
                }

                await context.SaveChangesAsync();
            }
            #endregion

            #region Seed School Nurse Users cùng StaffProfile
            if (!await context.StaffProfiles.AnyAsync())
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
                    var profile = new StaffProfile
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
                    await context.StaffProfiles.AddAsync(profile);

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
        }
    }
}
