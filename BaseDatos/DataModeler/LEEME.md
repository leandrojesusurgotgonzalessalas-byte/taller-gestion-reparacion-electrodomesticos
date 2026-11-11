# Diseño para documentación

Abrir TallerElectrodomesticos.dmd en Oracle SQL Developer Data Modeler y conservar la carpeta TallerElectrodomesticos junto a él. Contiene el modelo lógico (entidades y relaciones), el relacional con tablas / PK / FK / índices, y el modelo físico para SQL Server.

Las posiciones y conectores se distribuyeron manualmente. Los diagramas de consulta están en ../../docs/ModeloLogico.svg y ../../docs/ModeloFisico.svg desde la raíz del proyecto (docs).

El esquema ejecutable es SQL Server, no Oracle. ModeloFisicoSQLServer.ddl utiliza rowversion. ModeloImportacion.ddl separa las FK en ALTER TABLE y representa rowversion como binary(8) para la compatibilidad con el importador SQL Server 2012 del modelador; es una adaptación exclusivamente documental. No ejecutar ModeloImportacion.ddl para instalar el sistema.

Hay diez tablas y ocho FK. Egresos documenta Cartera y no tiene relaciones. La asignación a empleado y la referencia a proveedor son opcionales; el resto de referencias son obligatorias. Cada padre puede tener cero o muchos hijos. Las bajas físicas relacionadas se restringen.
