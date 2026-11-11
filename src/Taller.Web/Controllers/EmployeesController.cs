using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taller.Web.Data;
using Taller.Web.Models;
namespace Taller.Web.Controllers;
public class EmployeesController(TallerDb db):Controller {
 public async Task<IActionResult> Index(string? q){ViewBag.Query=q;var query=db.Employees.AsNoTracking().Where(x=>x.Active);if(!string.IsNullOrWhiteSpace(q))query=query.Where(x=>x.Name.Contains(q)||x.Phone.Contains(q));return View(await query.OrderBy(x=>x.Name).ToListAsync());}
 public async Task<IActionResult> Edit(int? id){var item=id.HasValue?await db.Employees.FindAsync(id):new Employee();return item==null?NotFound():View(item);}
 [HttpPost] public async Task<IActionResult> Edit(int? id,[Bind("Name,Phone,Specialty")]Employee input){
  if(!ModelState.IsValid){input.Id=id??0;return View(input);}
  var item=id.HasValue?await db.Employees.FindAsync(id):new Employee();if(item==null)return NotFound();
  item.Name=input.Name;item.Phone=input.Phone;item.Specialty=input.Specialty;
  if(!id.HasValue)db.Employees.Add(item);await db.SaveChangesAsync();TempData["Success"]="Registro guardado.";return RedirectToAction(nameof(Index));
 }
}
