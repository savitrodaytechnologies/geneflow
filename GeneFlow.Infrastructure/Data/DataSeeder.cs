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
}
