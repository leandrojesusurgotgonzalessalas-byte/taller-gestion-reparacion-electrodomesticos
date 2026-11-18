using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Taller.Web.Data;
using Taller.Web.Models;

namespace Taller.Web.Services;

public sealed class AvisosCorreo(IServiceScopeFactory scopes, IConfiguration config, ILogger<AvisosCorreo> logger) : BackgroundService
{
    private sealed record Avance(DateTime HastaUtc, DateOnly? UltimoResumen);
    private static string Numero(int id) => $"OT-{id:00000}";
    private static string Importe(decimal importe) => importe.ToString("N2", CultureInfo.GetCultureInfo("es-BO"));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var correo = config.GetSection("AvisosCorreo");
        if (!correo.GetValue<bool>("Habilitado")) return;
        if (string.IsNullOrWhiteSpace(correo["Servidor"]) ||
            !MailAddress.TryCreate(correo["Remitente"], out var remitente) ||
            !MailAddress.TryCreate(correo["Destinatario"], out var destinatario))
        {
            logger.LogWarning("Avisos por correo sin configurar: indica servidor, remitente y destinatario.");
            return;
        }

        var carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TallerElectrodomesticos_Entrega", "Avisos");
        Directory.CreateDirectory(carpeta);
        // Una sola instancia envía avisos aunque se abra también Visual Studio.
        FileStream control;
        try { control = new FileStream(Path.Combine(carpeta, "envio.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException) { logger.LogWarning("Otra instancia del taller ya gestiona los avisos."); return; }
        using var bloqueo = control;
        var ruta = Path.Combine(carpeta, "avance.json");
        void Guardar(Avance avance)
        {
            File.WriteAllText(ruta + ".tmp", JsonSerializer.Serialize(avance));
            File.Move(ruta + ".tmp", ruta, overwrite: true);
        }

        Avance avance;
        try
        {
            avance = File.Exists(ruta)
                ? JsonSerializer.Deserialize<Avance>(File.ReadAllText(ruta)) ?? throw new InvalidDataException()
                : new Avance(DateTime.UtcNow, null);
            if (!File.Exists(ruta)) Guardar(avance);
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            logger.LogWarning("No se pudo leer o guardar el avance de los avisos; no se enviarán correos.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TallerDb>();
                // Dejar tiempo a que terminen las transacciones de órdenes y pagos.
                var corte = DateTime.UtcNow.AddMinutes(-1);
                var local = Labels.Local(DateTime.UtcNow);
                var hoy = DateOnly.FromDateTime(local);
                var resumen = local.Hour >= 8 && avance.UltimoResumen != hoy;
                var lineas = new List<string>();
                if (corte > avance.HastaUtc)
                {
                    var nuevas = await db.Orders.AsNoTracking().Where(o => o.ReceivedAt > avance.HastaUtc && o.ReceivedAt <= corte)
                        .OrderBy(o => o.Id).Select(o => o.Id).ToListAsync(stoppingToken);
                    var cobros = await db.Payments.AsNoTracking().Where(p => p.PaidAt > avance.HastaUtc && p.PaidAt <= corte)
                        .OrderBy(p => p.Id).Select(p => new { p.RepairOrderId, p.Method, p.Amount }).ToListAsync(stoppingToken);
                    lineas.AddRange(nuevas.Select(id => $"Nueva orden o solicitud: {Numero(id)}"));
                    lineas.AddRange(cobros.Select(p => $"{(p.Method.Equals("Transferencia", StringComparison.OrdinalIgnoreCase) ? "Transferencia registrada" : "Cobro registrado")}: {Numero(p.RepairOrderId)} · Bs {Importe(p.Amount)}"));
                }

                if (resumen)
                {
                    var manana = local.Date.AddDays(1);
                    var abiertas = await db.Orders.AsNoTracking()
                        .Where(o => o.Status != RepairStatus.Cancelado && o.Status != RepairStatus.Entregado)
                        .Select(o => new { o.Id, o.DueDate, o.BudgetApproved,
                            Saldo = o.Labor + o.Parts.Sum(p => p.Quantity * p.UnitPrice) - o.Payments.Sum(p => p.Amount) })
                        .OrderBy(o => o.Id).ToListAsync(stoppingToken);
                    foreach (var o in abiertas)
                    {
                        if (o.DueDate?.Date <= manana)
                        {
                            var aviso = o.DueDate.Value.Date < local.Date ? "Trabajo vencido" : "Entrega próxima";
                            lineas.Add($"{aviso}: {Numero(o.Id)} · {o.DueDate.Value:dd/MM/yyyy}");
                        }
                        if (o.BudgetApproved && o.Saldo > 0)
                            lineas.Add($"Saldo pendiente: {Numero(o.Id)} · Bs {Importe(o.Saldo)}");
                    }
                }
                if (lineas.Count > 0)
                {
                    using var mensaje = new MailMessage(remitente, destinatario)
                    {
                        Subject = "Avisos del taller",
                        Body = string.Join(Environment.NewLine, lineas),
                    };
                    using var smtp = new SmtpClient(correo["Servidor"]!, correo.GetValue("Puerto", 587)) { EnableSsl = true };
                    if (!string.IsNullOrWhiteSpace(correo["Usuario"]))
                        smtp.Credentials = new NetworkCredential(correo["Usuario"], correo["Clave"]);
                    await smtp.SendMailAsync(mensaje, stoppingToken);
                }
                var siguiente = new Avance(corte > avance.HastaUtc ? corte : avance.HastaUtc, resumen ? hoy : avance.UltimoResumen);
                if (siguiente != avance) { Guardar(siguiente); avance = siguiente; }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogWarning("Los avisos no se completaron ({Tipo}). Se volverá a intentar.", ex.GetType().Name);
            }
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}
