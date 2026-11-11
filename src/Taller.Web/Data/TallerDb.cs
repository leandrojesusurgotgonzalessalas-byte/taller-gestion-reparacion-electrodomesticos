using Microsoft.EntityFrameworkCore;
using Taller.Web.Models;
namespace Taller.Web.Data;
public class TallerDb(DbContextOptions<TallerDb> options):DbContext(options) {
 public DbSet<Expense> Expenses=>Set<Expense>();
 public DbSet<Customer> Customers=>Set<Customer>();
 public DbSet<Employee> Employees=>Set<Employee>();
 public DbSet<Supplier> Suppliers=>Set<Supplier>();
 public DbSet<Equipment> Equipment=>Set<Equipment>();
 public DbSet<RepairOrder> Orders=>Set<RepairOrder>();
 public DbSet<Part> Parts=>Set<Part>();
 public DbSet<OrderPart> OrderParts=>Set<OrderPart>();
 public DbSet<Payment> Payments=>Set<Payment>();
 public DbSet<OrderEvent> Events=>Set<OrderEvent>();
 protected override void OnModelCreating(ModelBuilder b) {
  b.Entity<Expense>().ToTable("Egresos",t=>{t.HasCheckConstraint("CK_Egreso_Importe","[Amount]>0");t.HasCheckConstraint("CK_Egreso_Tipo","[Kind] IN (0,1)");});
  b.Entity<Expense>().Property(x=>x.Date).HasColumnType("date");
  b.Entity<Expense>().HasIndex(x=>x.Date);
  b.Entity<Customer>().ToTable("Clientes");b.Entity<Equipment>().ToTable("Equipos");
  b.Entity<Employee>().ToTable("Empleados");b.Entity<Supplier>().ToTable("Proveedores");
  b.Entity<RepairOrder>().ToTable("Ordenes",t=>t.HasCheckConstraint("CK_Orden_ManoObra","[Labor]>=0"));
  b.Entity<Part>().ToTable("Repuestos",t=>{t.HasCheckConstraint("CK_Repuesto_Stock","[Stock]>=0");t.HasCheckConstraint("CK_Repuesto_Precio","[Price]>=0");});
  b.Entity<OrderPart>().ToTable("OrdenRepuestos",t=>{t.HasCheckConstraint("CK_Linea_Cantidad","[Quantity]>0");t.HasCheckConstraint("CK_Linea_Precio","[UnitPrice]>=0");});
  b.Entity<Payment>().ToTable("Pagos",t=>t.HasCheckConstraint("CK_Pago_Importe","[Amount]>0"));
  b.Entity<OrderEvent>().ToTable("HistorialOrdenes");
  b.Entity<Part>().HasIndex(x=>x.Code).IsUnique();
  b.Entity<RepairOrder>().HasIndex(x=>x.Status);b.Entity<RepairOrder>().HasIndex(x=>x.ReceivedAt);
  b.Entity<Customer>().HasIndex(x=>x.Name);
  b.Entity<RepairOrder>().Ignore(x=>x.Total).Ignore(x=>x.Paid).Ignore(x=>x.Balance).Ignore(x=>x.Number);
  foreach(var fk in b.Model.GetEntityTypes().SelectMany(x=>x.GetForeignKeys())) fk.DeleteBehavior=DeleteBehavior.Restrict;
  foreach(var p in b.Model.GetEntityTypes().SelectMany(x=>x.GetProperties()).Where(x=>x.ClrType==typeof(decimal))) {p.SetPrecision(18);p.SetScale(2);}
 }
}
