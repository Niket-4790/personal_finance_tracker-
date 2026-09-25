using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using PersonalFinanceTracker.Components;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Repositories;
using PersonalFinanceTracker.Services;
using QuestPDF.Infrastructure;
using System.Security.Claims;
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Events.OnValidatePrincipal = async context =>
        {
            var userIdClaim =
                context.Principal?.FindFirst("AppUserId")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.RejectPrincipal();
                return;
            }

            var userRepository =
                context.HttpContext.RequestServices
                    .GetRequiredService<IUserRepository>();

            var user =
                await userRepository.GetByIdAsync(userId);

            if (user is null || !user.IsActive)
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                context.HttpContext.Response.Redirect(
                    "/Account/Login?error=inactive");
            }
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId =
            builder.Configuration["Authentication:Google:ClientId"]!;

        options.ClientSecret =
            builder.Configuration["Authentication:Google:ClientSecret"]!;

        //After Google successfully authenticates the user, use my application's
        //cookie authentication scheme to sign the user in.
        options.SignInScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.Events.OnCreatingTicket = async context =>
        {
            var googleSubjectId =
                context.Identity?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

            var email =
                context.Identity?
                    .FindFirst(ClaimTypes.Email)?
                    .Value;

            var name =
                context.Identity?
                    .FindFirst(ClaimTypes.Name)?
                    .Value;

            if (string.IsNullOrWhiteSpace(googleSubjectId) ||
                string.IsNullOrWhiteSpace(email))
            {
                context.Fail(
                    "Google account information is incomplete.");

                return;
            }

            name ??= email;//if name is null , use email as name

            var authService =
                context.HttpContext.RequestServices
                    .GetRequiredService<IAuthService>();

            var appUser =
                await authService.AuthenticateGoogleAsync(
                    googleSubjectId,
                    name,
                    email);

            if (appUser is null)
            {
                context.Fail("Your account is inactive.");
                return;
            }

            var identity = context.Identity;

            if (identity is null)
            {
                context.Fail(
                    "Google identity could not be created.");

                return;
            }

            // Remove claims that we want our application to control.
            foreach (var claim in identity.Claims
                         .Where(c =>
                             c.Type == ClaimTypes.NameIdentifier ||
                             c.Type == ClaimTypes.Name ||
                             c.Type == ClaimTypes.Email ||
                             c.Type == ClaimTypes.Role ||
                             c.Type == "AppUserId")
                         .ToList())
            {
                identity.RemoveClaim(claim);
            }

            // Our application's database user ID.
            identity.AddClaim(
                new Claim(
                    "AppUserId",
                    appUser.Id.ToString()));

            // Application name.
            identity.AddClaim(
                new Claim(
                    ClaimTypes.Name,
                    appUser.Name));

            // Application email.
            identity.AddClaim(
                new Claim(
                    ClaimTypes.Email,
                    appUser.Email));

            // Application role.
            identity.AddClaim(
                new Claim(
                    ClaimTypes.Role,
                    appUser.Role));
        };
        options.Events.OnRemoteFailure = context =>
        {
            context.Response.Redirect(
                "/Account/Login?error=inactive");

            context.HandleResponse();

            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

// Database
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Repositories (Dapper)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<INotificationRepository,NotificationRepository>();



// Services (business logic)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IReportPdfService, ReportPdfService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService,NotificationService>();
builder.Services.AddScoped<IRazorpayPaymentService,RazorpayPaymentService>();
builder.Services.AddScoped<IEmailService,EmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/Account/Logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("/Account/Login");
}).RequireAuthorization();

app.MapGet("/Account/GoogleLogin", async (HttpContext context) =>
{
    var properties = new GoogleChallengeProperties
    {
        RedirectUri = "/",
        Prompt = "select_account"
    };

    await context.ChallengeAsync(
        GoogleDefaults.AuthenticationScheme,
        properties);
});



app.Run();
