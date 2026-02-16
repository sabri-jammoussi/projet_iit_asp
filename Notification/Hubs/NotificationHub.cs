using System.Diagnostics;
using System.Security.Claims;
using Back.Data.Infrastructure.EF;
using Back.Data.Infrastructure.EF.Enums;
using Back.Data.Infrastructure.EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Notification.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private const string LOG = "[notif-hub]";

    private readonly ILogger<NotificationHub> _logger;
    private readonly OltpDbContext _dbContext;

    private const string GROUP_ADMIN = "Admin";
    private const string GROUP_CLIENT = "Client";
    public readonly static string GroupAdmin = GROUP_ADMIN;
    public readonly static string GroupClient = GROUP_CLIENT;
    private const string GROUP_OTHERS = "Others";

    public NotificationHub(ILogger<NotificationHub> logger, OltpDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var sw = Stopwatch.StartNew();
			var userName = Context.User?.FindFirstValue(ClaimTypes.GivenName) ?? "?";
			var userRole = Context.User?.FindFirstValue(ClaimTypes.Role) ?? "?";
			var groupName = await GetGroupName(Context.UserIdentifier);
            if (groupName != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation($"[{LOG}] client add to group [{groupName}]");
            }

            await base.OnConnectedAsync();

            sw.Stop();
            _logger.LogInformation($"[{LOG}] client connected {Context.UserIdentifier}|{userName} ({userRole})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{LOG}] client {Context.UserIdentifier} add to group");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var userName = Context.User?.FindFirstValue(ClaimTypes.GivenName) ?? "?";
            var userRole = Context.User?.FindFirstValue(ClaimTypes.Role) ?? "?";
            var groupName = await GetGroupName(Context.UserIdentifier);
            if (groupName != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation($"[{LOG}] client {Context.UserIdentifier} remove from group [{groupName}]");
            }
            await base.OnDisconnectedAsync(exception);

            sw.Stop();
            _logger.LogInformation($"[{LOG}] client disconnected {Context.UserIdentifier}|{userName} ({userRole})|{Context.ConnectionId} {exception?.Message}|{sw.ElapsedMilliseconds}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{LOG}] client remove from group");
        }
    }
    private async Task<string?> GetGroupName(string? userIdentifier)
    {
        // Try to obtain the user identifier from the claims if not provided
        var uid = userIdentifier;
        if (string.IsNullOrEmpty(uid))
        {
            uid = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        if (!int.TryParse(uid, out var userId))
        {
            _logger.LogWarning("{Log} cannot parse user id: {UserIdentifier}", LOG, uid ?? "<null>");
            return null;
        }

        var groupName = GROUP_OTHERS;
        var user = await _dbContext.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("{Log} user not found for id {UserId}", LOG, userId);
            return null;
        }


        if (IsClient(user))
        {
            groupName = GROUP_CLIENT;
        }

        else if (IsAdmin(user))
        {
            groupName = GROUP_ADMIN;
        }

        return groupName;
    }
    public static bool IsAdmin(AccountDao user)
            => user.Role == UserRole.Admin;
    public static bool IsClient(AccountDao user)
            => user.Role == UserRole.Client;
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("ConnectionId {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("ConnectionId {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }
}
