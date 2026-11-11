using Microsoft.EntityFrameworkCore;
using Taller.Web.Data;
using Taller.Web.Models;
namespace Taller.Web.Services;
public class WalletService(TallerDb db) {
 public async Task<WalletSummary> Summary(DateTime? from,DateTime? to) {
  var payments=db.Payments.AsNoTracking().AsQueryable();
  var expenses=db.Expenses.AsNoTracking().AsQueryable();
  var zone=TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
  if(from.HasValue){var utc=TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(from.Value.Date,DateTimeKind.Unspecified),zone);payments=payments.Where(x=>x.PaidAt>=utc);expenses=expenses.Where(x=>x.Date>=from.Value.Date);}
  if(to.HasValue){var end=to.Value.Date.AddDays(1);var utc=TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(end,DateTimeKind.Unspecified),zone);payments=payments.Where(x=>x.PaidAt<utc);expenses=expenses.Where(x=>x.Date<end);}
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
  var income=await payments.Select(x=>new {x.PaidAt,x.Amount,x.RepairOrderId,x.Reference}).ToListAsync();
  var outgoing=await expenses.ToListAsync();await tx.CommitAsync();
  return new WalletSummary {From=from,To=to,Movements=income.Select(x=>new WalletMovement(Labels.Local(x.PaidAt).Date,"Ingreso",$"Orden OT-{x.RepairOrderId:00000}",x.Amount,0,x.RepairOrderId,x.Reference))
   .Concat(outgoing.Select(x=>new WalletMovement(x.Date,x.Kind==ExpenseKind.Compra?"Compra":"Gasto",x.Description,0,x.Amount,null,x.Reference))).OrderByDescending(x=>x.Date).ToList()};
 }
}
