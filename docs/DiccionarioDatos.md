# Diccionario de datos

Origen: esquema EF Core instalado en SQL Server. Importes en bolivianos con decimal(18,2).

Las referencias a empleado y proveedor son opcionales; las demás son obligatorias. Todas tienen eliminación restringida (NO ACTION). Un padre puede tener cero o muchos hijos.

## Clientes

| Campo | Tipo SQL Server | Permite nulos |
|---|---|---|
| Id | int | No |
| Name | nvarchar(100) | No |
| Phone | nvarchar(30) | No |
| Address | nvarchar(200) | Sí |
| Email | nvarchar(100) | Sí |
| Active | bit | No |

## Empleados

| Campo | Tipo SQL Server | Permite nulos |
|---|---|---|
| Id | int | No |
| Name | nvarchar(100) | No |
| Phone | nvarchar(30) | No |
| Specialty | nvarchar(100) | Sí |
| Active | bit | No |

## Proveedores

| Campo | Tipo SQL Server | Permite nulos |
|---|---|---|
| Id | int | No |
| Name | nvarchar(120) | No |
| Phone | nvarchar(30) | No |
| Address | nvarchar(200) | Sí |
| Contact | nvarchar(100) | Sí |
| Active | bit | No |

## Equipos

| Campo | Tipo SQL Server | Permite nulos |
|---|---|---|
| Id | int | No |
| CustomerId | int | No |
| Type | nvarchar(60) | No |
| Brand | nvarchar(60) | No |
| Model | nvarchar(80) | Sí |
| Serial | nvarchar(100) | Sí |
| Accessories | nvarchar(300) | Sí |
| Active | bit | No |

## Repuestos

| Campo | Tipo SQL Server | Permite nulos |
|---|---|---|
| Id | int | No |
| SupplierId | int | Sí |
| Code | nvarchar(40) | No |
| Name | nvarchar(120) | No |
| Stock | int | No |
| Price | decimal(18,2) | No |
| Active | bit | No |
| RowVersion | rowversion | No |

## Ordenes

| Campo | Tipo SQL Server | Permite nulos |
|---|---|---|
| Id | int | No |
| EquipmentId | int | No |
| ServiceMode | int | No |
| ServiceAddress | nvarchar(300) | Sí |
| EmployeeId | int | Sí |
| ReceivedAt | datetime2 | No |
| UpdatedAt | datetime2 | No |
| DueDate | datetime2 | Sí |
| ReportedFault | nvarchar(1500) | No |
| Diagnosis | nvarchar(2000) | Sí |
| Technician | nvarchar(100) | Sí |
| Status | int | No |
| Labor | decimal(18,2) | No |
| BudgetApproved | bit | No |
| Notes | nvarchar(2000) | Sí |
| DeliveredAt | datetime2 | Sí |
| RowVersion | rowversion | No |

## HistorialOrdenes

| Campo | Tipo SQL Server | Permite nulos |
|---|---|---|
| Id | int | No |
| RepairOrderId | int | No |
| Description | nvarchar(500) | No |
| CreatedAt | datetime2 | No |

## OrdenRepuestos

| Campo | Tipo SQL Server | Permite nulos |
|---|---|---|
| Id | int | No |
| RepairOrderId | int | No |
| PartId | int | No |
| Quantity | int | No |
| UnitPrice | decimal(18,2) | No |

## Pagos

| Campo | Tipo SQL Server | Permite nulos |
|---|---|---|
| Id | int | No |
| RepairOrderId | int | No |
| Amount | decimal(18,2) | No |
| Method | nvarchar(30) | No |
| Reference | nvarchar(100) | Sí |
| PaidAt | datetime2 | No |

## Egresos

| Campo | Tipo SQL Server | Permite nulos |
|---|---|---|
| Id | int | No |
| Description | nvarchar(200) | No |
| Kind | int | No |
| Amount | decimal(18,2) | No |
| Date | date | No |
| Reference | nvarchar(100) | Sí |
| CreatedAt | datetime2 | No |

Compras (Kind = 0) y gastos (Kind = 1), con importe positivo. Tabla independiente, sin claves foráneas.

## Particularidades

- Id es int IDENTITY y clave primaria.
- RowVersion es rowversion generado por SQL Server para controlar ediciones concurrentes. El perfil SQL Server 2012 de Data Modeler lo representa como binary(8); no use el DDL de importación para instalar la base.
- Status guarda los valores del enum RepairStatus: 0 recibido, 1 en revisión, 2 esperando aprobación, 3 esperando repuesto, 4 en reparación, 5 reparado, 6 entregado, 7 cancelado.
- Fechas operativas se guardan en UTC y se muestran en America/La_Paz; DueDate es una fecha de calendario.
- Código de repuesto único; stock y precios no negativos; cantidad de línea y pagos positivos.
- Ordenes.Total, Paid y Balance se calculan, no son columnas persistidas.
