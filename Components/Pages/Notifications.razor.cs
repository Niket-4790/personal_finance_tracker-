using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Components.Pages;

public partial class Notifications
{
    [Inject]
    private INotificationService NotificationService { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private List<Notification> notifications = new();

    private int currentUserId;

    protected override async Task OnInitializedAsync()
    {
        var authState =
            await AuthenticationStateProvider
                .GetAuthenticationStateAsync();

        var userIdClaim =
            authState.User.FindFirst("AppUserId")?.Value;

        if (!int.TryParse(userIdClaim, out currentUserId))
            return;

        var result =
            await NotificationService
                .GetNotificationsAsync(currentUserId);

        notifications = result.ToList();
    }

    private async Task MarkAsRead(Notification notification)
    {
        if (notification.IsRead)
            return;

        await NotificationService.MarkAsReadAsync(
            notification.Id,
            currentUserId);

        notification.IsRead = true;

        Navigation.Refresh();
    }

    private async Task MarkAllAsRead()
    {
        await NotificationService
            .MarkAllAsReadAsync(currentUserId);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        Navigation.Refresh();
    }

    private async Task DeleteNotification(Notification notification)
    {
        await NotificationService.DeleteNotificationAsync(
            notification.Id,
            currentUserId);

        notifications.Remove(notification);

        Navigation.Refresh();
    }
}