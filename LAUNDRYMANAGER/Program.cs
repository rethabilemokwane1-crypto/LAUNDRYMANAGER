using Microsoft.EntityFrameworkCore;
using LaundryManager.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Log out after 30 mins of doing nothing
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add MVC services to your application container
builder.Services.AddControllersWithViews();

// FIX: This explicitly tells the app to use your specific ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// SESSION CONFIGURATION (For keeping students logged in)
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

// TURN ON SESSION MEMORY
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}");

app.Run();