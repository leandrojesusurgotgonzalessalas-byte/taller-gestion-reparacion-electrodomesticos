using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Taller.Web.Data;
var builder=WebApplication.CreateBuilder(args);
// Configuración propia de cada PC (por ejemplo, SQL Server Express). La crea scripts/Instalar.ps1 y no se sube a Git.
builder.Configuration.AddJsonFile("appsettings.Local.json",optional:true,reloadOnChange:false);
builder.Configuration.AddEnvironmentVariables();builder.Configuration.AddCommandLine(args); // las variables de entorno y argumentos siguen teniendo prioridad (pruebas)
builder.Services.AddControllersWithViews(o => {o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());o.ModelBinderProviders.Insert(0,new Taller.Web.Models.DecimalModelBinderProvider());});
builder.Services.AddDbContext<TallerDb>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Taller")));
builder.Services.AddScoped<Taller.Web.Services.WorkshopService>();
var app=builder.Build();
var culture=CultureInfo.GetCultureInfo("es-BO");
CultureInfo.DefaultThreadCurrentCulture=culture;CultureInfo.DefaultThreadCurrentUICulture=culture;
using(var scope=app.Services.CreateScope()) {
 var db=scope.ServiceProvider.GetRequiredService<TallerDb>();
 await db.Database.EnsureCreatedAsync();
 await WalletSchema.EnsureAsync(db);
 if(args.Contains("--schema")) {
  Console.WriteLine(db.Database.GenerateCreateScript());return;
 }
}
app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();app.UseRouting();
app.MapGet("/health",async(TallerDb db)=> await db.Database.CanConnectAsync() ? Results.Ok(new {application="Taller.Web",database="TallerElectrodomesticos",status="ok"}) : Results.StatusCode(503));
app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
app.Run();
