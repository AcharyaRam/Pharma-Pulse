var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// ✅ SESSION ENABLE
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// ✅ SESSION MIDDLEWARE (IMPORTANT POSITION)
app.UseSession();

app.UseAuthorization();

// Static files
app.MapStaticAssets();

// ✅ Default Redirect to Login
app.MapGet("/", context =>
{
    context.Response.Redirect("/Login");
    return Task.CompletedTask;
});

// Razor Pages
app.MapRazorPages()
   .WithStaticAssets();

app.Run();