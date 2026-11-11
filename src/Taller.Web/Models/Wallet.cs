using System.ComponentModel.DataAnnotations;
namespace Taller.Web.Models;
public enum ExpenseKind { Compra, Gasto }
public class Expense {
 public int Id {get;set;}
 [Required,MaxLength(200),Display(Name="Concepto")] public string Description {get;set;}="";
 [EnumDataType(typeof(ExpenseKind)),Display(Name="Tipo")] public ExpenseKind Kind {get;set;}
 [Range(typeof(decimal),"0.01","99999999",ParseLimitsInInvariantCulture=true),Display(Name="Importe (Bs)")] public decimal Amount {get;set;}
 [DataType(DataType.Date),Display(Name="Fecha")] public DateTime Date {get;set;}=Labels.Local(DateTime.UtcNow).Date;
 [MaxLength(100),Display(Name="Comprobante / referencia")] public string? Reference {get;set;}
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
}
public record WalletMovement(DateTime Date,string Type,string Description,decimal Income,decimal Expense,int? OrderId,string? Reference);
public class WalletSummary {
 public DateTime? From {get;set;}
 public DateTime? To {get;set;}
 public List<WalletMovement> Movements {get;set;}=[];
 public decimal Income=>Movements.Sum(x=>x.Income);
 public decimal Expenses=>Movements.Sum(x=>x.Expense);
 public decimal Net=>Income-Expenses;
}
