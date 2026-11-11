# Cartera

Ingresos: pagos y anticipos de Pagos vinculados a su orden. No se duplican ni se suman presupuestos pendientes.
Egresos: compras y gastos pagados, con fecha local, concepto, importe positivo de dos decimales y referencia opcional. Se conservan sin borrado.
Balance neto de caja: ingresos menos egresos; admite negativos. El filtro incluye ambos días en horario de Bolivia. Sin filtro se muestra todo el historial. No es utilidad contable.

SQL Server: tabla Egresos: Id PK identity, Description nvarchar(200), Kind int (0 Compra, 1 Gasto), Amount decimal(18,2)>0, Date date indexado, Reference nvarchar(100) nullable, CreatedAt datetime2 UTC. Sin nuevas relaciones.
WalletSchema.EnsureAsync crea la tabla al iniciar sobre instalaciones anteriores sin alterar los registros existentes. EnsureCreated incluye la tabla en bases nuevas.
Una compra en cartera registra dinero pagado; las existencias se administran en Piezas / repuestos.
