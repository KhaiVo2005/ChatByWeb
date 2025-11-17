using ChatByWeb.Services.Api;
using ChatByWeb.Services.Auth;
using ChatByWeb.Services.Conversation;
using ChatByWeb.Services.Message;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// Add Session (để lưu token)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
});

// Add services DI
builder.Services.AddScoped<IAuthService, KeycloakAuthService>();
builder.Services.AddScoped<KeycloakClient>();
builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<WebSocketChatClient>();

// Đăng ký IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Đăng ký AuthService và KeycloakClient
builder.Services.AddScoped<IAuthService, KeycloakAuthService>();
builder.Services.AddHttpClient<KeycloakClient>();

// Đăng ký ApiClient
builder.Services.AddHttpClient<IApiClient, ApiClient>();

// Controllers + Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllerRoute(
    name: "area",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);

app.Run();
