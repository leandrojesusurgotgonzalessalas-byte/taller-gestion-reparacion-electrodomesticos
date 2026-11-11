CREATE TABLE [Clientes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Phone] nvarchar(30) NOT NULL,
    [Address] nvarchar(200) NULL,
    [Email] nvarchar(100) NULL,
    [Active] bit NOT NULL,
    CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Empleados] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Phone] nvarchar(30) NOT NULL,
    [Specialty] nvarchar(100) NULL,
    [Active] bit NOT NULL,
    CONSTRAINT [PK_Empleados] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Proveedores] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(120) NOT NULL,
    [Phone] nvarchar(30) NOT NULL,
    [Address] nvarchar(200) NULL,
    [Contact] nvarchar(100) NULL,
    [Active] bit NOT NULL,
    CONSTRAINT [PK_Proveedores] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Equipos] (
    [Id] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [Type] nvarchar(60) NOT NULL,
    [Brand] nvarchar(60) NOT NULL,
    [Model] nvarchar(80) NULL,
    [Serial] nvarchar(100) NULL,
    [Accessories] nvarchar(300) NULL,
    [Active] bit NOT NULL,
    CONSTRAINT [PK_Equipos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Equipos_Clientes_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Repuestos] (
    [Id] int NOT NULL IDENTITY,
    [SupplierId] int NULL,
    [Code] nvarchar(40) NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    [Stock] int NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Active] bit NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Repuestos] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Repuesto_Precio] CHECK ([Price]>=0),
    CONSTRAINT [CK_Repuesto_Stock] CHECK ([Stock]>=0),
    CONSTRAINT [FK_Repuestos_Proveedores_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Proveedores] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Ordenes] (
    [Id] int NOT NULL IDENTITY,
    [EquipmentId] int NOT NULL,
    [ServiceMode] int NOT NULL,
    [ServiceAddress] nvarchar(300) NULL,
    [EmployeeId] int NULL,
    [ReceivedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [DueDate] datetime2 NULL,
    [ReportedFault] nvarchar(1500) NOT NULL,
    [Diagnosis] nvarchar(2000) NULL,
    [Technician] nvarchar(100) NULL,
    [Status] int NOT NULL,
    [Labor] decimal(18,2) NOT NULL,
    [BudgetApproved] bit NOT NULL,
    [Notes] nvarchar(2000) NULL,
    [DeliveredAt] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Ordenes] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Orden_ManoObra] CHECK ([Labor]>=0),
    CONSTRAINT [FK_Ordenes_Empleados_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Empleados] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Ordenes_Equipos_EquipmentId] FOREIGN KEY ([EquipmentId]) REFERENCES [Equipos] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [HistorialOrdenes] (
    [Id] int NOT NULL IDENTITY,
    [RepairOrderId] int NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_HistorialOrdenes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HistorialOrdenes_Ordenes_RepairOrderId] FOREIGN KEY ([RepairOrderId]) REFERENCES [Ordenes] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [OrdenRepuestos] (
    [Id] int NOT NULL IDENTITY,
    [RepairOrderId] int NOT NULL,
    [PartId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_OrdenRepuestos] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Linea_Cantidad] CHECK ([Quantity]>0),
    CONSTRAINT [CK_Linea_Precio] CHECK ([UnitPrice]>=0),
    CONSTRAINT [FK_OrdenRepuestos_Ordenes_RepairOrderId] FOREIGN KEY ([RepairOrderId]) REFERENCES [Ordenes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_OrdenRepuestos_Repuestos_PartId] FOREIGN KEY ([PartId]) REFERENCES [Repuestos] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Pagos] (
    [Id] int NOT NULL IDENTITY,
    [RepairOrderId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Method] nvarchar(30) NOT NULL,
    [Reference] nvarchar(100) NULL,
    [PaidAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Pagos] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Pago_Importe] CHECK ([Amount]>0),
    CONSTRAINT [FK_Pagos_Ordenes_RepairOrderId] FOREIGN KEY ([RepairOrderId]) REFERENCES [Ordenes] ([Id]) ON DELETE NO ACTION
);
GO


CREATE INDEX [IX_Clientes_Name] ON [Clientes] ([Name]);
GO


CREATE INDEX [IX_Equipos_CustomerId] ON [Equipos] ([CustomerId]);
GO


CREATE INDEX [IX_HistorialOrdenes_RepairOrderId] ON [HistorialOrdenes] ([RepairOrderId]);
GO


CREATE INDEX [IX_Ordenes_EmployeeId] ON [Ordenes] ([EmployeeId]);
GO


CREATE INDEX [IX_Ordenes_EquipmentId] ON [Ordenes] ([EquipmentId]);
GO


CREATE INDEX [IX_Ordenes_ReceivedAt] ON [Ordenes] ([ReceivedAt]);
GO


CREATE INDEX [IX_Ordenes_Status] ON [Ordenes] ([Status]);
GO


CREATE INDEX [IX_OrdenRepuestos_PartId] ON [OrdenRepuestos] ([PartId]);
GO


CREATE INDEX [IX_OrdenRepuestos_RepairOrderId] ON [OrdenRepuestos] ([RepairOrderId]);
GO


CREATE INDEX [IX_Pagos_RepairOrderId] ON [Pagos] ([RepairOrderId]);
GO


CREATE UNIQUE INDEX [IX_Repuestos_Code] ON [Repuestos] ([Code]);
GO


CREATE INDEX [IX_Repuestos_SupplierId] ON [Repuestos] ([SupplierId]);
GO

CREATE TABLE [Egresos] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(200) NOT NULL,
    [Kind] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Date] date NOT NULL,
    [Reference] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Egresos] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Egreso_Importe] CHECK ([Amount]>0),
    CONSTRAINT [CK_Egreso_Tipo] CHECK ([Kind] IN (0,1))
);
CREATE INDEX [IX_Egresos_Date] ON [Egresos] ([Date]);
