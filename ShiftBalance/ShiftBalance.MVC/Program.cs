using Microsoft.EntityFrameworkCore;
using ShiftBalance.MVC.DAL;
using ShiftBalance.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// DbConnection
builder.Services.AddEntityFrameworkNpgsql()
    .AddDbContext<DataContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<EmployeeRepository>();
builder.Services.AddScoped<EmployeeVacationsRepository>();

// Services
builder.Services.AddScoped<EmployeeService>();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();