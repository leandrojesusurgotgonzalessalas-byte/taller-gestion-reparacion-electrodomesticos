using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taller.Web.Data;
using Taller.Web.Models;
namespace Taller.Web.Controllers;
public class EquipmentController(TallerDb db):Controller {
 async Task Lists()=>ViewBag.Customers=new SelectList(await db.Customers.Where(x=>x.Active).OrderBy(x=>x.Name).ToListAsync(),"Id","Name");
 public async Task<IActionResult> Index(string? q){ViewBag.Query=q;var query=db.Equipment.AsNoTracking().Include(x=>x.Customer).Where(x=>x.Active);if(!string.IsNullOrWhiteSpace(q))query=query.Where(x=>x.Brand.Contains(q)||x.Type.Contains(q)||x.Customer!.Name.Contains(q)||(x.Serial!=null&&x.Serial.Contains(q)));return View(await query.OrderByDescending(x=>x.Id).ToListAsync());}
 public async Task<IActionResult> Edit(int? id){await Lists();var e=id.HasValue?await db.Equipment.FindAsync(id):new Equipment();return e==null?NotFound():View(e);}
 [HttpPost] public async Task<IActionResult> Edit(int? id,[Bind("CustomerId,Type,Brand,Model,Serial,Accessories")]Equipment input) {
  if(!await db.Customers.AnyAsync(x=>x.Id==input.CustomerId && x.Active))ModelState.AddModelError("CustomerId","Seleccione un cliente válido.");
  var e=id.HasValue?await db.Equipment.FindAsync(id):new Equipment();if(e==null)return NotFound();
  if(id.HasValue && e.CustomerId!=input.CustomerId && await db.Orders.AnyAsync(x=>x.EquipmentId==id))ModelState.AddModelError("CustomerId","El equipo tiene órdenes; no puede cambiar su propietario.");
  if(!ModelState.IsValid){await Lists();input.Id=id??0;return View(input);}
  e.CustomerId=input.CustomerId;e.Type=input.Type;e.Brand=input.Brand;e.Model=input.Model;e.Serial=input.Serial;e.Accessories=input.Accessories;
  if(!id.HasValue)db.Equipment.Add(e);await db.SaveChangesAsync();TempData["Success"]="Equipo guardado.";return RedirectToAction(nameof(Index));
 }
}
