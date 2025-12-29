using Microsoft.EntityFrameworkCore;
using Students_Portal_App.Data;
using Students_Portal_App.Interfaces;
using Students_Portal_App.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//Register Services 
builder.Services.AddScoped<IStudentInformationsServices, StudentInformationsServices>();
//Register DbContext to Mysql
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseMySql(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(
        builder.Configuration.GetConnectionString("DefaultConnection"))
    ));


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
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}


app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Student}/{action=Index}/{id?}");

app.Run();
