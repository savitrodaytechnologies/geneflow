using GeneFlow.Core.Entities;
using GeneFlow.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GeneFlow.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(GeneFlowDbContext context, ILogger logger)
    {
        if (await context.Labs.AnyAsync())
        {
            logger.LogInformation("Database already seeded. Skipping.");
            return;
        }

        logger.LogInformation("Seeding database...");

        var lab = new Lab
        {
            LabId = Guid.NewGuid(),
            LabName = "GeneFlow Research Lab",
            InstitutionName = "Savitroday Technologies",
            Description = "Default lab for GeneFlow qPCR experiments",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var adminUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = "admin@geneflow.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123!"),
            FullName = "Lab Administrator",
            SystemRole = "SystemAdmin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var researcherUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = "researcher@geneflow.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Research@123!"),
            FullName = "Kishi Researcher",
            SystemRole = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var adminLabUser = new LabUser
        {
            LabUserId = Guid.NewGuid(),
            LabId = lab.LabId,
            UserId = adminUser.UserId,
            LabRole = LabRole.LabAdmin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        var researcherLabUser = new LabUser
        {
            LabUserId = Guid.NewGuid(),
            LabId = lab.LabId,
            UserId = researcherUser.UserId,
            LabRole = LabRole.Researcher,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        context.Labs.Add(lab);
        context.Users.AddRange(adminUser, researcherUser);
        context.LabUsers.AddRange(adminLabUser, researcherLabUser);

        await context.SaveChangesAsync();

        logger.LogInformation("Seeding complete. Lab: {LabName}, Admin: {AdminEmail}, Researcher: {ResearcherEmail}",
            lab.LabName, adminUser.Email, researcherUser.Email);
    }

    /// <summary>
    /// Adds any extra users that don't yet exist in the database.
    /// Safe to call even after initial seed — only inserts if email not found.
    /// </summary>
    public static async Task SeedExtraUsersAsync(GeneFlowDbContext context, ILogger logger)
    {
        // Look up the first active lab to attach users to
        var lab = await context.Labs.FirstOrDefaultAsync(l => l.IsActive);
        if (lab == null)
        {
            logger.LogWarning("SeedExtraUsers: No active lab found. Run initial seed first.");
            return;
        }

        var extraUsers = new[]
        {
            new { Email = "prakashp@geneflow.local", Password = "Password98!", FullName = "Prakash P", SystemRole = "User", LabRole = LabRole.Researcher },
        };

        foreach (var u in extraUsers)
        {
            if (await context.Users.AnyAsync(x => x.Email == u.Email.ToLowerInvariant()))
            {
                logger.LogInformation("User {Email} already exists. Skipping.", u.Email);
                continue;
            }

            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = u.Email.ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(u.Password),
                FullName = u.FullName,
                SystemRole = u.SystemRole,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var labUser = new LabUser
            {
                LabUserId = Guid.NewGuid(),
                LabId = lab.LabId,
                UserId = newUser.UserId,
                LabRole = u.LabRole,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            };

            context.Users.Add(newUser);
            context.LabUsers.Add(labUser);
            await context.SaveChangesAsync();

            logger.LogInformation("Created user {Email} with role {LabRole} in lab {LabName}",
                newUser.Email, u.LabRole, lab.LabName);
        }
    }
}
