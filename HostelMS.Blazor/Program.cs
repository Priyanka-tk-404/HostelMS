using Blazored.LocalStorage;
using HostelMS.Blazor;
using HostelMS.Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API base URL - HTTP for local dev
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://hostelms-production.up.railway.app/") });

// Auth
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Feature services - all now receive ILocalStorageService for token attachment
builder.Services.AddScoped<IcdStudentApiService, StudentApiService>();
builder.Services.AddScoped<IRoomApiService, RoomApiService>();
builder.Services.AddScoped<IFeeApiService, FeeApiService>();
builder.Services.AddScoped<IVisitorApiService, VisitorApiService>();
builder.Services.AddScoped<IComplaintApiService, ComplaintApiService>();
builder.Services.AddScoped<IAttendanceApiService, AttendanceApiService>();
builder.Services.AddScoped<IDashboardApiService, DashboardApiService>();
builder.Services.AddScoped<INoticeApiService, NoticeApiService>();

await builder.Build().RunAsync();
