using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taller.Web.Data;
using Taller.Web.Models;

namespace Taller.Web.Controllers;

public class AgendaController(TallerDb db) : Controller
{
    public async Task<IActionResult> Index(DateTime? fecha, string? vista, int? empleado, RepairStatus? estado, ServiceMode? modalidad)
    {
        var hoy = Labels.Local(DateTime.UtcNow).Date;
        var dia = fecha is { Year: >= 1901 and <= 9997 } ? fecha.Value.Date : hoy;
        var mensual = vista == "mes";
        var inicio = mensual ? new DateTime(dia.Year, dia.Month, 1) : dia.AddDays(-((int)dia.DayOfWeek + 6) % 7);
        var fin = mensual ? inicio.AddMonths(1).AddDays(-1) : inicio.AddDays(6);

        var consulta = db.Orders.AsNoTracking()
            .Where(o => o.Status != RepairStatus.Cancelado && o.Status != RepairStatus.Entregado);
        if (empleado.HasValue) consulta = consulta.Where(o => o.EmployeeId == empleado);
        if (estado.HasValue) consulta = consulta.Where(o => o.Status == estado);
        if (modalidad.HasValue) consulta = consulta.Where(o => o.ServiceMode == modalidad);
        var ordenes = await consulta.Include(o => o.Equipment).Include(o => o.Employee)
            .OrderBy(o => o.DueDate).ThenBy(o => o.Id).ToListAsync();
        ViewBag.Empleados = await db.Employees.AsNoTracking().OrderBy(e => e.Name).ToListAsync();
        ViewBag.Empleado = empleado;
        ViewBag.Estado = estado;
        ViewBag.Modalidad = modalidad;
        ViewBag.Inicio = inicio;
        ViewBag.Fin = fin;
        ViewBag.Hoy = hoy;
        ViewBag.Vista = mensual ? "mes" : "semana";
        return View(ordenes);
    }
}
