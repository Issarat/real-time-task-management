using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealTimeTaskManagement.Application.Projects;

namespace RealTimeTaskManagement.Web.Hubs;

[Authorize]
public sealed class BoardHub(IProjectMembershipService projectMembershipService) : Hub
{
    public async Task JoinProject(Guid projectId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new HubException("Authentication is required.");
        }

        var isMember = await projectMembershipService.IsMemberAsync(
            projectId,
            userId,
            Context.ConnectionAborted);

        if (!isMember)
        {
            throw new HubException("You do not have access to this project.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(projectId));
    }

    public static string GroupName(Guid projectId) => $"project:{projectId:N}";
}
