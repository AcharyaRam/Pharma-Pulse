using Pharma_Pulse.Data;
using Pharma_Pulse.Services;   // ✅ Add this using
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add services to the container
builder.Services.AddRazorPages();

// ✅ Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ✅ Register MedicineService (Add this line)
builder.Services.AddScoped<MedicineService>();

// ✅ Session Services MUST be before Build()
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Session Middleware MUST be after Routing
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
