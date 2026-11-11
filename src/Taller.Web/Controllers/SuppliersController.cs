using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taller.Web.Data;
using Taller.Web.Models;
namespace Taller.Web.Controllers;
public class SuppliersController(TallerDb db):Controller {
 public async Task<IActionResult> Index(string? q){ViewBag.Query=q;var query=db.Suppliers.AsNoTracking().Where(x=>x.Active);if(!string.IsNullOrWhiteSpace(q))query=query.Where(x=>x.Name.Contains(q)||x.Phone.Contains(q));return View(await query.OrderBy(x=>x.Name).ToListAsync());}
 public async Task<IActionResult> Edit(int? id){var item=id.HasValue?await db.Suppliers.FindAsync(id):new Supplier();return item==null?NotFound():View(item);}
 [HttpPost] public async Task<IActionResult> Edit(int? id,[Bind("Name,Phone,Address,Contact")]Supplier input){
  if(!ModelState.IsValid){input.Id=id??0;return View(input);}
  var item=id.HasValue?await db.Suppliers.FindAsync(id):new Supplier();if(item==null)return NotFound();
  item.Name=input.Name;item.Phone=input.Phone;item.Address=input.Address;item.Contact=input.Contact;
  if(!id.HasValue)db.Suppliers.Add(item);await db.SaveChangesAsync();TempData["Success"]="Registro guardado.";return RedirectToAction(nameof(Index));
 }
}
