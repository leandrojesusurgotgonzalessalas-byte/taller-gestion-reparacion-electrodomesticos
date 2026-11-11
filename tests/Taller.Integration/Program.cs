using Microsoft.EntityFrameworkCore;
using Taller.Web.Data;
using Taller.Web.Models;
using Taller.Web.Services;
using System.Text.Json;
var database="TallerElectrodomesticos_Pruebas_"+Guid.NewGuid().ToString("N");
var connection=$"Server=localhost;Database={database};Integrated Security=True;TrustServerCertificate=True";
var options=new DbContextOptionsBuilder<TallerDb>().UseSqlServer(connection).Options;
var checks=new List<string>();
void Check(bool ok,string message){if(!ok)throw new Exception(message);checks.Add(message);Console.WriteLine("OK: "+message);}
async Task Reject(Func<Task> task,string message){try{await task();}catch(InvalidOperationException){checks.Add(message);Console.WriteLine("OK: "+message);return;}throw new Exception("No rechazó: "+message);}
async Task<RepairOrder> Read(int id){await using var ctx=new TallerDb(options);return await new WorkshopService(ctx).FullOrders().SingleAsync(x=>x.Id==id);}
async Task Act(Func<WorkshopService,Task> action){await using var ctx=new TallerDb(options);await action(new WorkshopService(ctx));}
await using var setup=new TallerDb(options);
try {
 await setup.Database.EnsureCreatedAsync();
 var customer=new Customer{Name="Cliente de prueba",Phone="70000000"};
 var equipment=new Equipment{Customer=customer,Type="Lavadora",Brand="Prueba",Serial="TEST-001"};
 var part=new Part{Code="TEST-REP",Name="Bomba de prueba",Stock=4,Price=50};
 var order=new RepairOrder{Equipment=equipment,ReportedFault="No desagua",Labor=100};
 setup.AddRange(order,part);await setup.SaveChangesAsync();
 Check(await setup.Database.CanConnectAsync(),"SQL Server: conexión y esquema real");
 var current=await Read(order.Id);
 await Reject(()=>Act(s=>s.ChangeStatus(order.Id,RepairStatus.Entregado,current.RowVersion)),"Rechaza entrega directa desde recibido");
 await Reject(()=>Act(s=>s.AddPart(order.Id,part.Id,5)),"Rechaza uso de stock insuficiente");
 await Act(s=>s.AddPart(order.Id,part.Id,2));
 current=await Read(order.Id);
 Check(current.Total==200 && current.Parts.Count==1,"Calcula presupuesto con mano de obra y repuestos");
 await using(var ctx=new TallerDb(options))Check((await ctx.Parts.FindAsync(part.Id))!.Stock==2,"Descuenta existencias exactamente una vez");
 await Reject(()=>Act(s=>s.AddPayment(order.Id,20,"Efectivo",null)),"Rechaza pago sin presupuesto aprobado");
 await Act(s=>s.UpdateOrder(order.Id,"Bomba averiada",null,100,null,null,true,current.RowVersion));
 try {await Act(s=>s.UpdateOrder(order.Id,"Otra edición",null,100,null,null,true,current.RowVersion));throw new Exception("No rechazó edición concurrente");}
 catch(DbUpdateConcurrencyException){checks.Add("Rechaza edición concurrente con RowVersion");Console.WriteLine("OK: Rechaza edición concurrente con RowVersion");}
 var id=await setup.Orders.Select(x=>x.Id).FirstAsync();
 current=await Read(id);
 await Reject(()=>Act(s=>s.AddPayment(id,201,"Efectivo",null)),"Rechaza sobrepago");
 await Reject(()=>Act(s=>s.AddPayment(id,0,"Efectivo",null)),"Rechaza importe cero");
 await Reject(()=>Act(s=>s.AddPayment(id,0.001m,"Efectivo",null)),"Rechaza más de dos decimales");
 await Act(s=>s.ChangeStatus(id,RepairStatus.EnRevision,current.RowVersion));
 current=await Read(id);await Act(s=>s.ChangeStatus(id,RepairStatus.EnReparacion,current.RowVersion));
 current=await Read(id);await Act(s=>s.ChangeStatus(id,RepairStatus.Reparado,current.RowVersion));
 current=await Read(id);await Reject(()=>Act(s=>s.ChangeStatus(id,RepairStatus.Entregado,current.RowVersion)),"Rechaza entrega con saldo pendiente");
 await Act(s=>s.AddPayment(id,80,"Efectivo","Anticipo"));
 current=await Read(id);Check(current.Paid==80 && current.Balance==120,"Registra anticipo y calcula saldo");
 // Dos cobros concurrentes de 120 no pueden superar el saldo de 120.
 async Task<bool> RacePayment(){try{await Act(s=>s.AddPayment(id,120,"QR",null));return true;}catch(InvalidOperationException){return false;}catch(DbUpdateException){return false;}catch(Microsoft.Data.SqlClient.SqlException){return false;}}
 var race=await Task.WhenAll(RacePayment(),RacePayment());
 current=await Read(id);Check(race.Count(x=>x)==1 && current.Paid==200 && current.Balance==0,"Cobros simultáneos conservan el límite del saldo");
 await Act(s=>s.ChangeStatus(id,RepairStatus.Entregado,current.RowVersion));
 current=await Read(id);Check(current.Status==RepairStatus.Entregado && current.DeliveredAt!=null,"Entrega solo al completar reparación y cobro");
 await Reject(()=>Act(s=>s.AddPart(id,1,1)),"Orden entregada bloquea modificaciones");
 part=await setup.Parts.SingleAsync();
 equipment=await setup.Equipment.SingleAsync();
 var cancelled=new RepairOrder{EquipmentId=equipment.Id,ReportedFault="Prueba de cancelación"};
 setup.Orders.Add(cancelled);await setup.SaveChangesAsync();
 await Act(s=>s.AddPart(cancelled.Id,part.Id,1));
 var before=await Read(cancelled.Id);await Act(s=>s.ChangeStatus(cancelled.Id,RepairStatus.Cancelado,before.RowVersion));
 await using(var ctx=new TallerDb(options))Check((await ctx.Parts.FindAsync(part.Id))!.Stock==2,"Cancelar restituye stock y conserva historial");
 Check((await Read(id)).Events.Count>=7,"Auditoría conserva cambios y pagos");
 await using(var ctx=new TallerDb(options)) {
  try {await ctx.Database.ExecuteSqlRawAsync("UPDATE Repuestos SET Stock=-1");throw new Exception("SQL permitió stock negativo");}
  catch(Microsoft.Data.SqlClient.SqlException){checks.Add("Restricción SQL impide stock negativo");}
 }
 var result=new {database,completedAt=DateTime.UtcNow,passed=true,checks};
 // Comprueba la actualización sobre una base existente y su repetición segura.
 await setup.Database.ExecuteSqlRawAsync("DROP TABLE Egresos");
 await WalletSchema.EnsureAsync(setup);await WalletSchema.EnsureAsync(setup);
 Check(await setup.Orders.CountAsync()==2,"Actualizar cartera conserva las órdenes existentes");
 var day=new DateTime(2026,10,14);
 setup.Expenses.AddRange(new Expense{Kind=ExpenseKind.Compra,Description="Compra de prueba",Amount=50,Date=day},new Expense{Kind=ExpenseKind.Gasto,Description="Gasto de prueba",Amount=20,Date=day.AddDays(-1)});
 await setup.SaveChangesAsync();
 var wallet=await new WalletService(setup).Summary(null,null);
 Check(wallet.Income==200&&wallet.Expenses==70&&wallet.Net==130,"Cartera usa cobros reales una sola vez y resta compras y gastos");
 wallet=await new WalletService(setup).Summary(day,day);
 Check(wallet.Expenses==50,"Filtro de egresos incluye ambos extremos del día");
 var payment=await setup.Payments.FirstAsync();payment.PaidAt=new DateTime(2026,10,14,3,59,0,DateTimeKind.Utc);await setup.SaveChangesAsync();
 wallet=await new WalletService(setup).Summary(day.AddDays(-1),day.AddDays(-1));
 Check(wallet.Movements.Any(x=>x.OrderId==id&&x.Income==payment.Amount),"Filtro de ingresos respeta medianoche de Bolivia");
 wallet=await new WalletService(setup).Summary(new DateTime(2000,1,1),new DateTime(2000,1,1));
 Check(wallet.Net==0&&wallet.Movements.Count==0,"Cartera sin movimientos muestra cero");
 try{await setup.Database.ExecuteSqlRawAsync("INSERT INTO Egresos(Description,Kind,Amount,Date,CreatedAt) VALUES(N'Inválido',0,-1,'20261014',GETUTCDATE())");throw new Exception("SQL permitió egreso negativo");}
 catch(Microsoft.Data.SqlClient.SqlException){Check(true,"SQL rechaza egresos negativos");}
 var invalid=new Expense{Description="Prueba",Kind=(ExpenseKind)7,Amount=0};
 var validation=new List<System.ComponentModel.DataAnnotations.ValidationResult>();
 Check(!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(invalid,new(invalid),validation,true)&&validation.Count>=2,"Formulario rechaza categoría inválida e importe cero");
 var output=args.FirstOrDefault()??"resultado-integracion.json";
 await File.WriteAllTextAsync(output,JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));
 Console.WriteLine($"PRUEBAS CORRECTAS: {checks.Count}");
} finally {
 await setup.Database.EnsureDeletedAsync();
 Console.WriteLine("Base temporal de pruebas eliminada: "+database);
}
