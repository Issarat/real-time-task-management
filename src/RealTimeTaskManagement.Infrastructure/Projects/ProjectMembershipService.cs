using Microsoft.EntityFrameworkCore;
using RealTimeTaskManagement.Application.Projects;
using RealTimeTaskManagement.Infrastructure.Persistence;

namespace RealTimeTaskManagement.Infrastructure.Projects;

internal sealed class ProjectMembershipService(ApplicationDbContext dbContext)
    : IProjectMembershipService
{
    public Task<bool> IsMemberAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ProjectMembers
            .AsNoTracking()
            .AnyAsync(
                member => member.ProjectId == projectId && member.UserId == userId,
                cancellationToken);
    }
}
