using System.ComponentModel.DataAnnotations;
namespace Taller.Web.Models;
public class Customer {
 public int Id {get;set;}
 [Required,MaxLength(100),Display(Name="Nombre completo")] public string Name {get;set;}="";
 [Required,MaxLength(30),Display(Name="Teléfono")] public string Phone {get;set;}="";
 [MaxLength(200),Display(Name="Dirección")] public string? Address {get;set;}
 [EmailAddress,MaxLength(100),Display(Name="Correo")] public string? Email {get;set;}
 public bool Active {get;set;}=true;
 public ICollection<Equipment> Equipment {get;set;}=[];
}
public class Equipment {
 public int Id {get;set;}
 [Range(1,int.MaxValue),Display(Name="Cliente")] public int CustomerId {get;set;}
 public Customer? Customer {get;set;}
 [Required,MaxLength(60),Display(Name="Tipo de electrodoméstico")] public string Type {get;set;}="";
 [Required,MaxLength(60),Display(Name="Marca")] public string Brand {get;set;}="";
 [MaxLength(80),Display(Name="Modelo")] public string? Model {get;set;}
 [MaxLength(100),Display(Name="Número de serie")] public string? Serial {get;set;}
 [MaxLength(300),Display(Name="Accesorios / condición de ingreso")] public string? Accessories {get;set;}
 public bool Active {get;set;}=true;
 public ICollection<RepairOrder> Orders {get;set;}=[];
}
public enum RepairStatus { Recibido, EnRevision, EsperandoAprobacion, EsperandoRepuesto, EnReparacion, Reparado, Entregado, Cancelado }
public enum ServiceMode { Taller, Domicilio }
public class Employee {
 public int Id {get;set;}
 [Required,MaxLength(100),Display(Name="Nombre completo")] public string Name {get;set;}="";
 [Required,MaxLength(30),Display(Name="Teléfono")] public string Phone {get;set;}="";
 [MaxLength(100),Display(Name="Especialidad / función")] public string? Specialty {get;set;}
 public bool Active {get;set;}=true;
}
public class Supplier {
 public int Id {get;set;}
 [Required,MaxLength(120),Display(Name="Nombre / razón social")] public string Name {get;set;}="";
 [Required,MaxLength(30),Display(Name="Teléfono")] public string Phone {get;set;}="";
 [MaxLength(200),Display(Name="Dirección")] public string? Address {get;set;}
 [MaxLength(100),Display(Name="Persona de contacto")] public string? Contact {get;set;}
 public bool Active {get;set;}=true;
}
public class RepairOrder {
 public int Id {get;set;}
 [Range(1,int.MaxValue),Display(Name="Equipo")] public int EquipmentId {get;set;}
 public Equipment? Equipment {get;set;}
 [Display(Name="Modalidad")] public ServiceMode ServiceMode {get;set;}
 [MaxLength(300),Display(Name="Dirección del servicio a domicilio")] public string? ServiceAddress {get;set;}
 [Display(Name="Empleado asignado")] public int? EmployeeId {get;set;}
 public Employee? Employee {get;set;}
 public DateTime ReceivedAt {get;set;}=DateTime.UtcNow;
 public DateTime UpdatedAt {get;set;}=DateTime.UtcNow;
 [Display(Name="Fecha estimada"),DataType(DataType.Date)] public DateTime? DueDate {get;set;}
 [Required,MaxLength(1500),Display(Name="Falla reportada")] public string ReportedFault {get;set;}="";
 [MaxLength(2000)] public string? Diagnosis {get;set;}
 [MaxLength(100)] public string? Technician {get;set;}
 public RepairStatus Status {get;set;}=RepairStatus.Recibido;
 public decimal Labor {get;set;}
 public bool BudgetApproved {get;set;}
 [MaxLength(2000)] public string? Notes {get;set;}
 public DateTime? DeliveredAt {get;set;}
 [Timestamp] public byte[] RowVersion {get;set;}=[];
 public ICollection<OrderPart> Parts {get;set;}=[];
 public ICollection<Payment> Payments {get;set;}=[];
 public ICollection<OrderEvent> Events {get;set;}=[];
 public decimal Total => Labor+Parts.Sum(p=>p.Quantity*p.UnitPrice);
 public decimal Paid => Payments.Sum(p=>p.Amount);
 public decimal Balance => Total-Paid;
 public string Number => $"OT-{Id:00000}";
}
public class Part {
 public int Id {get;set;}
 [Display(Name="Proveedor")] public int? SupplierId {get;set;}
 public Supplier? Supplier {get;set;}
 [Required,MaxLength(40),Display(Name="Código")] public string Code {get;set;}="";
 [Required,MaxLength(120),Display(Name="Nombre")] public string Name {get;set;}="";
 [Range(0,1000000),Display(Name="Existencias")] public int Stock {get;set;}
 [Range(typeof(decimal),"0","99999999"),Display(Name="Precio de venta (Bs)")] public decimal Price {get;set;}
 public bool Active {get;set;}=true;
 [Timestamp] public byte[] RowVersion {get;set;}=[];
}
public class OrderPart {
 public int Id {get;set;}
 public int RepairOrderId {get;set;}
 public RepairOrder? RepairOrder {get;set;}
 public int PartId {get;set;}
 public Part? Part {get;set;}
 public int Quantity {get;set;}
 public decimal UnitPrice {get;set;}
}
public class Payment {
 public int Id {get;set;}
 public int RepairOrderId {get;set;}
 public RepairOrder? RepairOrder {get;set;}
 public decimal Amount {get;set;}
 [MaxLength(30)] public string Method {get;set;}="Efectivo";
 [MaxLength(100)] public string? Reference {get;set;}
 public DateTime PaidAt {get;set;}=DateTime.UtcNow;
}
public class OrderEvent {
 public int Id {get;set;}
 public int RepairOrderId {get;set;}
 public RepairOrder? RepairOrder {get;set;}
 [MaxLength(500)] public string Description {get;set;}="";
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
}
public static class Labels {
 public static string Status(RepairStatus s)=>s switch {RepairStatus.EnRevision=>"En revisión",RepairStatus.EsperandoAprobacion=>"Esperando aprobación",RepairStatus.EsperandoRepuesto=>"Esperando repuesto",RepairStatus.EnReparacion=>"En reparación",_=>s.ToString()};
 public static string Money(decimal n)=>"Bs "+n.ToString("N2");
 public static DateTime Local(DateTime d)=>TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(d,DateTimeKind.Utc),TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time"));
}
