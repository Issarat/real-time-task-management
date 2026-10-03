namespace RealTimeTaskManagement.Application.Projects;

public interface IProjectMembershipService
{
    Task<bool> IsMemberAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default);
}
