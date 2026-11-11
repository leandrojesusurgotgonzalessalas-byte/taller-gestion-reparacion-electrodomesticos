using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taller.Web.Data;
using Taller.Web.Services;
using Taller.Web.Models;
namespace Taller.Web.Controllers;
public class HomeController(TallerDb db,WorkshopService service):Controller {
 public async Task<IActionResult> Index() {
  var orders=await service.FullOrders().OrderByDescending(x=>x.ReceivedAt).ToListAsync();
  ViewBag.Customers=await db.Customers.CountAsync(x=>x.Active);
  ViewBag.LowStock=await db.Parts.CountAsync(x=>x.Active && x.Stock<=2);
  var today=Labels.Local(DateTime.UtcNow).Date;
  ViewBag.TodayPayments=orders.SelectMany(x=>x.Payments).Where(x=>Labels.Local(x.PaidAt).Date==today).Sum(x=>x.Amount);
  return View(orders);
 }
 public IActionResult Error()=>View();
}
