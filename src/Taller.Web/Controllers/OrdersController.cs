using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taller.Web.Data;
using Taller.Web.Models;
using Taller.Web.Services;
namespace Taller.Web.Controllers;
public class OrdersController(TallerDb db,WorkshopService service):Controller {
 async Task EquipmentList(){var list=await db.Equipment.Include(x=>x.Customer).Where(x=>x.Active).ToListAsync();ViewBag.Equipment=new SelectList(list.Select(x=>new {x.Id,Label=$"{x.Customer!.Name} — {x.Type} {x.Brand} {x.Model}"}),"Id","Label");}
 public async Task<IActionResult> Index(string? q,RepairStatus? status){ViewBag.Query=q;ViewBag.Status=status;var query=service.FullOrders();if(!string.IsNullOrWhiteSpace(q)){var term=q.Replace("OT-","");if(int.TryParse(term,out var id))query=query.Where(x=>x.Id==id);else query=query.Where(x=>x.Equipment!.Customer!.Name.Contains(q)||x.Equipment.Brand.Contains(q)||x.ReportedFault.Contains(q));}if(status.HasValue)query=query.Where(x=>x.Status==status);return View(await query.OrderByDescending(x=>x.Id).ToListAsync());}
 public async Task<IActionResult> Create(int? equipmentId){await EquipmentList();return View(new RepairOrder{EquipmentId=equipmentId??0});}
 [HttpPost] public async Task<IActionResult> Create([Bind("EquipmentId,ReportedFault,DueDate,ServiceMode,ServiceAddress,EmployeeId")]RepairOrder input) {
  if(!await db.Equipment.AnyAsync(x=>x.Id==input.EquipmentId&&x.Active))ModelState.AddModelError("EquipmentId","Seleccione un equipo válido.");
  if(!Enum.IsDefined(input.ServiceMode))ModelState.AddModelError("ServiceMode","Seleccione una modalidad válida.");
  if(input.ServiceMode==ServiceMode.Domicilio&&string.IsNullOrWhiteSpace(input.ServiceAddress))ModelState.AddModelError("ServiceAddress","La dirección es obligatoria para el servicio a domicilio.");
  if(input.EmployeeId.HasValue&&!await db.Employees.AnyAsync(x=>x.Id==input.EmployeeId&&x.Active))ModelState.AddModelError("EmployeeId","Empleado inválido.");
  if(!ModelState.IsValid){await EquipmentList();return View(input);}
  input.ReceivedAt=DateTime.UtcNow;service.Event(input,"Equipo recibido. Orden creada.");db.Orders.Add(input);await db.SaveChangesAsync();TempData["Success"]="Orden de reparación creada.";return RedirectToAction(nameof(Details),new{id=input.Id});
 }
 public async Task<IActionResult> Details(int id){var o=await service.FullOrders().SingleOrDefaultAsync(x=>x.Id==id);if(o==null)return NotFound();ViewBag.Parts=new SelectList(await db.Parts.Where(x=>x.Active&&x.Stock>0).OrderBy(x=>x.Name).ToListAsync(),"Id","Name");ViewBag.Employees=new SelectList(await db.Employees.Where(x=>x.Active).OrderBy(x=>x.Name).ToListAsync(),"Id","Name",o.EmployeeId);return View(o);}
 async Task<IActionResult> Perform(int id,Func<Task> task){try{await task();TempData["Success"]="Cambios guardados.";}catch(DbUpdateConcurrencyException){TempData["Error"]="La orden cambió en otra ventana. Recargue y vuelva a intentar.";}catch(InvalidOperationException e){TempData["Error"]=e.Message;}catch(DbUpdateException){TempData["Error"]="No se pudo guardar. Revise los datos y vuelva a intentar.";}return RedirectToAction(nameof(Details),new{id});}
 [HttpPost] public Task<IActionResult> Update(int id,string? diagnosis,int? employeeId,decimal labor,DateTime? dueDate,string? notes,bool approved,byte[] rowVersion)=>Perform(id,()=>ModelState.IsValid?service.UpdateOrder(id,diagnosis,employeeId,labor,dueDate,notes,approved,rowVersion):throw new InvalidOperationException("Revise los importes y la fecha."));
 [HttpPost] public Task<IActionResult> Status(int id,RepairStatus status,byte[] rowVersion)=>Perform(id,()=>ModelState.IsValid?service.ChangeStatus(id,status,rowVersion):throw new InvalidOperationException("Estado inválido."));
 [HttpPost] public Task<IActionResult> AddPart(int id,int partId,int quantity)=>Perform(id,()=>ModelState.IsValid?service.AddPart(id,partId,quantity):throw new InvalidOperationException("Revise la cantidad."));
 [HttpPost] public Task<IActionResult> Pay(int id,decimal amount,string method,string? reference)=>Perform(id,()=>ModelState.IsValid?service.AddPayment(id,amount,method,reference):throw new InvalidOperationException("Importe inválido."));
 public async Task<IActionResult> Print(int id){var o=await service.FullOrders().SingleOrDefaultAsync(x=>x.Id==id);return o==null?NotFound():View(o);}
}
