using Microsoft.AspNetCore.Mvc;
using Taller.Web.Data;
using Taller.Web.Models;
using Taller.Web.Services;
namespace Taller.Web.Controllers;
public class WalletController(TallerDb db):Controller {
 public async Task<IActionResult> Index(DateTime? from,DateTime? to) {
  if(!ModelState.IsValid || (from.HasValue&&to.HasValue&&from>to) || to?.Year>=9999 || from?.Year<1900){ModelState.AddModelError("","Indica un rango de fechas válido.");return View(new WalletSummary{From=from,To=to});}
  return View(await new WalletService(db).Summary(from,to));
 }
 public IActionResult Create()=>View(new Expense());
 [HttpPost] public async Task<IActionResult> Create([Bind("Description,Kind,Amount,Date,Reference")]Expense input) {
  if(decimal.Round(input.Amount,2)!=input.Amount)ModelState.AddModelError("Amount","Usa como máximo dos decimales.");
  if(input.Date.Date>Labels.Local(DateTime.UtcNow).Date || input.Date.Year<2000)ModelState.AddModelError("Date","Indica una fecha entre el año 2000 y hoy.");
  if(!ModelState.IsValid)return View(input);
  input.Date=input.Date.Date;input.CreatedAt=DateTime.UtcNow;
  db.Expenses.Add(input);await db.SaveChangesAsync();TempData["Success"]="Egreso registrado en cartera.";return RedirectToAction(nameof(Index));
 }
}
