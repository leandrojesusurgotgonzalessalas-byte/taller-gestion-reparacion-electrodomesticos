using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taller.Web.Data;
using Taller.Web.Models;
namespace Taller.Web.Controllers;
public class PartsController(TallerDb db):Controller {
 async Task Suppliers()=>ViewBag.Suppliers=new SelectList(await db.Suppliers.Where(x=>x.Active).OrderBy(x=>x.Name).ToListAsync(),"Id","Name");
 public async Task<IActionResult> Index(string? q){ViewBag.Query=q;var query=db.Parts.AsNoTracking().Where(x=>x.Active);if(!string.IsNullOrWhiteSpace(q))query=query.Where(x=>x.Name.Contains(q)||x.Code.Contains(q));return View(await query.OrderBy(x=>x.Name).ToListAsync());}
 public async Task<IActionResult> Edit(int? id){await Suppliers();var p=id.HasValue?await db.Parts.FindAsync(id):new Part();return p==null?NotFound():View(p);}
 [HttpPost] public async Task<IActionResult> Edit(int? id,[Bind("Code,Name,Stock,Price,RowVersion,SupplierId")]Part input) {
  if(input.SupplierId.HasValue&&!await db.Suppliers.AnyAsync(x=>x.Id==input.SupplierId&&x.Active))ModelState.AddModelError("SupplierId","Proveedor inválido.");
  if(await db.Parts.AnyAsync(x=>x.Code==input.Code && x.Id!=(id??0)))ModelState.AddModelError("Code","Ese código ya existe.");
  if(decimal.Round(input.Price,2)!=input.Price)ModelState.AddModelError("Price","Use como máximo dos decimales.");
  if(!ModelState.IsValid){await Suppliers();input.Id=id??0;return View(input);}
  var p=id.HasValue?await db.Parts.FindAsync(id):new Part();if(p==null)return NotFound();
  p.Code=input.Code.Trim();p.Name=input.Name;p.Stock=input.Stock;p.Price=input.Price;p.SupplierId=input.SupplierId;
  if(!id.HasValue)db.Parts.Add(p);else db.Entry(p).Property(x=>x.RowVersion).OriginalValue=input.RowVersion;
  try{await db.SaveChangesAsync();}catch(DbUpdateConcurrencyException){TempData["Error"]="El repuesto cambió en otra ventana. Recargue y vuelva a editar.";return RedirectToAction(nameof(Edit),new{id});}
  TempData["Success"]="Repuesto guardado.";return RedirectToAction(nameof(Index));
 }
}
