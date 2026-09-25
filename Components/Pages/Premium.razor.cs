using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using PersonalFinanceTracker.Services;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PersonalFinanceTracker.Components.Pages;

public partial class Premium
{
    [Inject]
    private IRazorpayPaymentService RazorpayPaymentService { get; set; } = default!;

    [Inject]
    private INotificationService NotificationService { get; set; } = default!;

    [Inject]
    private IPaymentService PaymentService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthStateTask { get; set; }

    private bool processing;
    private bool isPremium;
    private string? error;

    private int currentUserId;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateTask!;

        var user = authState.User;

        var userIdClaim = user.FindFirst("AppUserId")?.Value;

        if (int.TryParse(userIdClaim, out var userId))
        {
            isPremium = await PaymentService.IsPremiumAsync(userId);
        }
    }

    private async Task StartPayment()
    {
        error = null;
        processing = true;

        try
        {
            var authState = await AuthStateTask!;

            var user = authState.User;

            if (user.IsInRole("Admin"))
            {
                error =
                    "Administrators do not need Premium.";

                return;
            }

            var userIdClaim = user.FindFirst("AppUserId")?.Value;

            if (!int.TryParse(userIdClaim,out var userId))
            {
                error ="Unable to identify your account.";

                return;
            }

            currentUserId = userId;

            var name = user.FindFirst(ClaimTypes.Name)?.Value ?? "FinanceTrack User";

            var email =user.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                error = "Your account email could not be found.";

                return;
            }

            var order =  await RazorpayPaymentService.CreateOrderAsync(userId, name,email);

            var dotNetReference = DotNetObjectReference.Create(this);

            await JS.InvokeVoidAsync(
                "openRazorpayCheckout",
                order.KeyId,
                order.OrderId,
                order.Amount,
                name,
                email,
                dotNetReference);
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
        finally
        {
            processing = false;
        }
    }

    [JSInvokable]
    public async Task HandleRazorpayPayment(
        string paymentId,
        string orderId,
        string signature)
    {
        try
        {
            var valid =
                await RazorpayPaymentService.VerifyPaymentAsync(orderId,paymentId,signature);

            if (!valid)
            {
                error =
                    "Payment verification failed.";

                return;
            }

            await NotificationService.CreateNotificationAsync(
             currentUserId,
             "Premium Activated",
             "Your ₹99 Premium payment was successful. Premium features are now available.", true);

            isPremium = true;

            await InvokeAsync(StateHasChanged);

            Navigation.NavigateTo(
                "/premium?payment=success",
                true);
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }
}