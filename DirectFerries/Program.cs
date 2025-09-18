using DirectFerries.Services;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);

// register services
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<IApiService, ApiService>();
//Using session for simplicity here
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddLogging();

//Would use ms identity for authentication if using database or federation. Using session for simplicity 
//Add the authorization set up for ms identity so the cookie automatically gets added when using the built in UserManager loginAsync
////Identity options example
//builder.Services.AddIdentity<TesIdentityApplicationUser, IdentityRole>(options =>
//    {
//        options.SignIn.RequireConfirmedAccount = true;
//        options.Password.RequireDigit = true;
//        options.Password.RequireLowercase = true;
//        options.Password.RequireUppercase = true;
//        options.Password.RequiredLength = 14;
//        options.Password.RequireNonAlphanumeric = true;

//        // Default Lockout settings.
//        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
//        options.Lockout.MaxFailedAccessAttempts = 6;
//        options.Lockout.AllowedForNewUsers = true;
//    })
//    .AddEntityFrameworkStores<TesIdentityDbContext>()
//    .AddDefaultTokenProviders();

//or add cookie authentication scheme example

/* builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(); */


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
//app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();


app.UseEndpoints(endpoints =>
{
    // Redirect root to login
    endpoints.MapGet("/", context =>
    {
        context.Response.Redirect("/Login");
        return Task.CompletedTask;
    });
});


app.Run();