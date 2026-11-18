# Taller de Electrodomésticos · entrega conectada

Esta entrega independiente contiene el código completo del taller actual (incluidos Cartera, Agenda y avisos SMTP desactivados) y una base nueva vacía. Se preparó a partir del código existente, no de una plantilla sin funciones.

## Abrir en Visual Studio

1. Abrir TallerElectrodomesticos.sln.
2. Seleccionar Taller.Web como proyecto de inicio y pulsar F5.
3. La web de desarrollo usa http://127.0.0.1:7681; YARP de desarrollo usa 7680.
4. En Ver > Explorador de objetos de SQL Server, conectar a localhost con autenticación de Windows. Abrir Bases de datos > TallerElectrodomesticos_Entrega > Tablas.

La conexión está en src/Taller.Web/appsettings.json. Models define los datos; Data/TallerDb.cs asigna las tablas; Controllers recibe las acciones; Services aplica reglas; Views contiene las pantallas; wwwroot contiene estilos y recursos.

## Base nueva y diagramas

Base: TallerElectrodomesticos_Entrega, en localhost. Tiene diez tablas y ocho relaciones. La base original TallerElectrodomesticos y sus registros se conservan.

El script BaseDatos/CrearBaseDatos.sql instala el esquema en una base nueva; se detiene si ya contiene tablas. No volver a ejecutarlo sobre una base existente. La aplicación inicializa una base vacía mediante EF Core cuando es necesario.

Abrir BaseDatos/DataModeler/TallerElectrodomesticos.dmd en Oracle Data Modeler y conservar su carpeta compañera TallerElectrodomesticos. Los diagramas de consulta están en docs/ModeloFisico.svg y docs/ModeloLogico.svg. Se conservaron las relaciones y el diseño sin cruces; Egresos es una tabla independiente. Oracle solo documenta: la aplicación usa SQL Server.

Para publicar los ejecutables de esta entrega: scripts/Preparar.ps1. El iniciador usa los puertos 7580/7581. El instalador de acceso directo crea un nombre terminado en ENTREGA, distinto al del taller original.

## Verificaciones

docs/Verificacion-entrega.json: geometría del diagrama y esquema.
docs/Verificacion-integracion-entrega.json: 25 comprobaciones sobre SQL Server temporal.
docs/Verificacion-modeler-entrega.log: lectura nativa del modelo con las bibliotecas de Oracle Data Modeler.

No se copiaron datos de negocio ni respaldos. No se configuraron credenciales de correo.
