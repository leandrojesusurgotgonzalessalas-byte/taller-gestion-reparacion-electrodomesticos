USE [TallerElectrodomesticos];
GO
IF OBJECT_ID(N'Empleados',N'U') IS NULL CREATE TABLE Empleados(Id int IDENTITY NOT NULL CONSTRAINT PK_Empleados PRIMARY KEY, Name nvarchar(100) NOT NULL, Phone nvarchar(30) NOT NULL, Specialty nvarchar(100) NULL, Active bit NOT NULL);
IF OBJECT_ID(N'Proveedores',N'U') IS NULL CREATE TABLE Proveedores(Id int IDENTITY NOT NULL CONSTRAINT PK_Proveedores PRIMARY KEY, Name nvarchar(120) NOT NULL, Phone nvarchar(30) NOT NULL, Address nvarchar(200) NULL, Contact nvarchar(100) NULL, Active bit NOT NULL);
IF COL_LENGTH('Ordenes','EmployeeId') IS NULL ALTER TABLE Ordenes ADD EmployeeId int NULL;
IF COL_LENGTH('Ordenes','ServiceMode') IS NULL ALTER TABLE Ordenes ADD ServiceMode int NOT NULL CONSTRAINT DF_Ordenes_ServiceMode DEFAULT 0;
IF COL_LENGTH('Ordenes','ServiceAddress') IS NULL ALTER TABLE Ordenes ADD ServiceAddress nvarchar(300) NULL;
IF COL_LENGTH('Repuestos','SupplierId') IS NULL ALTER TABLE Repuestos ADD SupplierId int NULL;
GO
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_Ordenes_Empleados_EmployeeId') ALTER TABLE Ordenes ADD CONSTRAINT FK_Ordenes_Empleados_EmployeeId FOREIGN KEY(EmployeeId) REFERENCES Empleados(Id);
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_Repuestos_Proveedores_SupplierId') ALTER TABLE Repuestos ADD CONSTRAINT FK_Repuestos_Proveedores_SupplierId FOREIGN KEY(SupplierId) REFERENCES Proveedores(Id);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Ordenes_EmployeeId') CREATE INDEX IX_Ordenes_EmployeeId ON Ordenes(EmployeeId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Repuestos_SupplierId') CREATE INDEX IX_Repuestos_SupplierId ON Repuestos(SupplierId);
GO
