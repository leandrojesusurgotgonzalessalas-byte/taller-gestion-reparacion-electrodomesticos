-- Agrega la tabla de egresos (Cartera) a una base creada antes de ese módulo.
-- Es idempotente: si la tabla ya existe no cambia nada. La aplicación también la crea al iniciar.
USE [TallerElectrodomesticos];
GO
IF OBJECT_ID(N'[dbo].[Egresos]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Egresos](
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Egresos] PRIMARY KEY,
        [Description] nvarchar(200) NOT NULL,
        [Kind] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Date] date NOT NULL,
        [Reference] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [CK_Egreso_Importe] CHECK ([Amount]>0),
        CONSTRAINT [CK_Egreso_Tipo] CHECK ([Kind] IN (0,1)));
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Egresos_Date' AND object_id=OBJECT_ID(N'[dbo].[Egresos]'))
    CREATE INDEX [IX_Egresos_Date] ON [dbo].[Egresos]([Date]);
GO
