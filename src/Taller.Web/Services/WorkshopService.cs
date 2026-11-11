using System.Data;
using Microsoft.EntityFrameworkCore;
using Taller.Web.Data;
using Taller.Web.Models;
namespace Taller.Web.Services;
public class WorkshopService(TallerDb db) {
 public IQueryable<RepairOrder> FullOrders()=>db.Orders.Include(x=>x.Employee).Include(x=>x.Equipment)!.ThenInclude(x=>x!.Customer).Include(x=>x.Parts).ThenInclude(x=>x.Part).Include(x=>x.Payments).Include(x=>x.Events).AsSplitQuery();
 static void Open(RepairOrder o) {if(o.Status is RepairStatus.Entregado or RepairStatus.Cancelado) throw new InvalidOperationException("La orden está cerrada; no admite modificaciones.");}
 public void Event(RepairOrder o,string text) {o.UpdatedAt=DateTime.UtcNow;o.Events.Add(new OrderEvent{Description=text});}
 public async Task AddPart(int orderId,int partId,int quantity) {
  if(quantity<1 || quantity>1000000) throw new InvalidOperationException("Ingrese una cantidad válida.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
  var o=await FullOrders().SingleAsync(x=>x.Id==orderId);Open(o);
  if(o.Status==RepairStatus.Reparado) throw new InvalidOperationException("Regrese a En reparación para añadir repuestos.");
  var p=await db.Parts.SingleOrDefaultAsync(x=>x.Id==partId && x.Active) ?? throw new InvalidOperationException("El repuesto no está disponible.");
  if(p.Stock<quantity) throw new InvalidOperationException("No hay existencias suficientes.");
  p.Stock-=quantity;o.Parts.Add(new OrderPart{PartId=p.Id,Quantity=quantity,UnitPrice=p.Price});
  o.BudgetApproved=false;Event(o,$"Repuesto añadido: {p.Name}, cantidad {quantity}. Presupuesto pendiente de aprobación.");
  await db.SaveChangesAsync();await tx.CommitAsync();
 }
 public async Task AddPayment(int orderId,decimal amount,string method,string? reference) {
  if(amount<=0 || decimal.Round(amount,2)!=amount) throw new InvalidOperationException("El pago debe ser positivo y tener como máximo dos decimales.");
  if(!new[]{"Efectivo","Transferencia","QR","Tarjeta"}.Contains(method)) throw new InvalidOperationException("Método de pago inválido.");
  if(reference?.Length>100) throw new InvalidOperationException("La referencia admite hasta 100 caracteres.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
  var o=await FullOrders().SingleAsync(x=>x.Id==orderId);Open(o);
  if(!o.BudgetApproved) throw new InvalidOperationException("Apruebe el presupuesto antes de registrar pagos.");
  if(amount>o.Balance) throw new InvalidOperationException("El pago supera el saldo pendiente.");
  o.Payments.Add(new Payment{Amount=amount,Method=method,Reference=reference});Event(o,$"Pago registrado: {Labels.Money(amount)} ({method}).");
  await db.SaveChangesAsync();await tx.CommitAsync();
 }
 public async Task UpdateOrder(int id,string? diagnosis,int? employeeId,decimal labor,DateTime? dueDate,string? notes,bool approved,byte[] version) {
  if(labor<0 || labor>99999999 || decimal.Round(labor,2)!=labor) throw new InvalidOperationException("Mano de obra inválida.");
  if(diagnosis?.Length>2000 || notes?.Length>2000) throw new InvalidOperationException("El texto supera la longitud permitida.");
  if(employeeId.HasValue && !await db.Employees.AnyAsync(x=>x.Id==employeeId && x.Active))throw new InvalidOperationException("Seleccione un empleado válido.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
  var o=await FullOrders().SingleAsync(x=>x.Id==id);Open(o);
  db.Entry(o).Property(x=>x.RowVersion).OriginalValue=version;
  if(labor+o.Parts.Sum(x=>x.Quantity*x.UnitPrice)<o.Paid) throw new InvalidOperationException("El presupuesto no puede quedar por debajo de los pagos recibidos.");
  if(o.Status==RepairStatus.Reparado && (labor!=o.Labor || approved!=o.BudgetApproved)) throw new InvalidOperationException("Regrese a En reparación para modificar el presupuesto.");
  if(approved && string.IsNullOrWhiteSpace(diagnosis)) throw new InvalidOperationException("Registre el diagnóstico antes de aprobar el presupuesto.");
  o.Diagnosis=diagnosis;o.EmployeeId=employeeId;o.Labor=labor;o.DueDate=dueDate;o.Notes=notes;o.BudgetApproved=approved;
  Event(o,"Diagnóstico / presupuesto actualizado"+(approved?". Aprobación del cliente registrada.":". Presupuesto pendiente."));
  await db.SaveChangesAsync();await tx.CommitAsync();
 }
 public async Task ChangeStatus(int id,RepairStatus status,byte[] version) {
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
  var o=await FullOrders().SingleAsync(x=>x.Id==id);Open(o);
  if(!Enum.IsDefined(status) || o.Status==status) throw new InvalidOperationException("Seleccione un nuevo estado válido.");
  bool valid=o.Status switch {
   RepairStatus.Recibido=>status is RepairStatus.EnRevision or RepairStatus.Cancelado,
   RepairStatus.EnRevision=>status is RepairStatus.EsperandoAprobacion or RepairStatus.EsperandoRepuesto or RepairStatus.EnReparacion or RepairStatus.Cancelado,
   RepairStatus.EsperandoAprobacion=>status is RepairStatus.EnRevision or RepairStatus.EnReparacion or RepairStatus.EsperandoRepuesto or RepairStatus.Cancelado,
   RepairStatus.EsperandoRepuesto=>status is RepairStatus.EnRevision or RepairStatus.EnReparacion or RepairStatus.Cancelado,
   RepairStatus.EnReparacion=>status is RepairStatus.EnRevision or RepairStatus.EsperandoRepuesto or RepairStatus.Reparado or RepairStatus.Cancelado,
   RepairStatus.Reparado=>status is RepairStatus.EnReparacion or RepairStatus.Entregado,
   _=>false};
  if(!valid) throw new InvalidOperationException("La transición seleccionada no corresponde al estado actual.");
  if(status is RepairStatus.EnReparacion or RepairStatus.Reparado or RepairStatus.Entregado && !o.BudgetApproved) throw new InvalidOperationException("El presupuesto debe estar aprobado.");
  if(status==RepairStatus.Entregado && o.Balance!=0) throw new InvalidOperationException("Debe cobrar el saldo pendiente antes de entregar.");
  if(status==RepairStatus.Cancelado && o.Paid>0) throw new InvalidOperationException("La orden tiene pagos. No se puede cancelar sin conciliar la devolución.");
  db.Entry(o).Property(x=>x.RowVersion).OriginalValue=version;
  if(status==RepairStatus.Cancelado) {
   foreach(var line in o.Parts) {var part=await db.Parts.SingleAsync(x=>x.Id==line.PartId);part.Stock+=line.Quantity;}
   Event(o,"Cancelación: existencias de repuestos restituidas; líneas conservadas como historial.");
  }
  Event(o,$"Estado: {Labels.Status(o.Status)} → {Labels.Status(status)}.");o.Status=status;
  if(status==RepairStatus.Entregado)o.DeliveredAt=DateTime.UtcNow;
  await db.SaveChangesAsync();await tx.CommitAsync();
 }
}
