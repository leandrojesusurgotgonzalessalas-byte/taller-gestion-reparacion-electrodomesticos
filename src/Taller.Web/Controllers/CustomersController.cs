using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taller.Web.Data;
using Taller.Web.Models;
namespace Taller.Web.Controllers;
public class CustomersController(TallerDb db):Controller {
 public async Task<IActionResult> Index(string? q) {
  ViewBag.Query=q;var query=db.Customers.AsNoTracking().Where(x=>x.Active);
  if(!string.IsNullOrWhiteSpace(q))query=query.Where(x=>x.Name.Contains(q)||x.Phone.Contains(q));
  return View(await query.OrderBy(x=>x.Name).ToListAsync());
 }
 public async Task<IActionResult> Edit(int? id) {var c=id.HasValue?await db.Customers.FindAsync(id):new Customer();return c==null?NotFound():View(c);}
 [HttpPost] public async Task<IActionResult> Edit(int? id,[Bind("Name,Phone,Address,Email")]Customer input) {
  if(!ModelState.IsValid){input.Id=id??0;return View(input);}
  var c=id.HasValue?await db.Customers.FindAsync(id):new Customer();if(c==null)return NotFound();
  c.Name=input.Name.Trim();c.Phone=input.Phone.Trim();c.Address=input.Address;c.Email=input.Email;
  if(!id.HasValue)db.Customers.Add(c);await db.SaveChangesAsync();TempData["Success"]="Cliente guardado.";return RedirectToAction(nameof(Index));
 }
}
