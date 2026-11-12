# TALLER DE GESTION DE REPARACION DE ELECTRODOMESTICOS

Aplicación local para el negocio de Leandro: clientes, empleados, proveedores, equipos, piezas / repuestos, reparaciones en taller y a domicilio, diagnósticos, presupuestos, cobros e impresión de comprobantes. Importes en bolivianos.

## Abrir y trabajar

En el escritorio, hacer doble clic en **TALLER DE GESTION DE REPARACION DE ELECTRODOMESTICOS.lnk**. También puede ejecutar `Iniciar.cmd` de esta carpeta. El iniciador arranca la aplicación y YARP y abre http://127.0.0.1:7180. Reutiliza los procesos si ya están activos y muestra un mensaje si SQL Server está detenido o un puerto está ocupado. Una ventana breve de inicio permite ver cualquier error; no requiere escribir comandos.

La distribución compilada está en `runtime/publish` y contiene su propio runtime .NET. No necesita abrir Visual Studio ni compilar cada vez. SQL Server sigue siendo necesario y debe estar funcionando en esta PC. Cerrar el navegador no detiene el servidor; para detenerlo ejecutar `scripts/Detener.ps1`.

La base real **TallerElectrodomesticos** está en el SQL Server `localhost`, con autenticación Windows. No utiliza Oracle como base de datos. La primera entrega comienza sin datos ficticios.

## Instalar en otra PC (descargado desde GitHub)

El repositorio contiene todo lo necesario para crear la base de datos **vacía**; los datos reales del negocio nunca se suben a GitHub.

Requisitos de la PC nueva (Windows 10/11):

1. **SDK de .NET 9** o superior: https://dotnet.microsoft.com/download/dotnet/9.0
2. **SQL Server 2019 o posterior**; sirve SQL Server Express gratuito (instancia predeterminada o `SQLEXPRESS`): https://www.microsoft.com/sql-server/sql-server-downloads
3. Conexión a Internet la primera vez, para descargar los paquetes NuGet.

Pasos:

1. Descargar el repositorio (`git clone` o **Code → Download ZIP** y descomprimir).
2. Hacer doble clic en **`Instalar.cmd`**.

El instalador comprueba .NET y SQL Server, detecta la instancia (si es SQL Server Express guarda la conexión en `src/Taller.Web/appsettings.Local.json`, archivo propio de esa PC que no se sube a Git), compila la aplicación, crea el acceso directo en el escritorio y abre el taller. En el primer inicio la aplicación crea la base **TallerElectrodomesticos** vacía. Si la base ya existe, no se modifica ni se borra ningún dato.

Otras opciones:

- Indicar el servidor a mano: `powershell -ExecutionPolicy Bypass -File scripts\Instalar.ps1 -Servidor "localhost\SQLEXPRESS"`
- Crear la base manualmente desde SQL Server Management Studio: ejecutar `BaseDatos/CrearBaseDatos.sql` en un servidor sin la base.
- Base creada con una versión anterior: aplicar los scripts de `BaseDatos/Migraciones` en orden (son idempotentes).
- Llevar los datos reales a otra PC: copiar un respaldo de `scripts/Respaldo.ps1` (archivo `.bak`) por un medio privado y restaurarlo en SQL Server. No subir respaldos a GitHub.

Si aparece un error al crear la base, revisar `runtime/Taller.Web.error.log`; normalmente la cuenta de Windows no tiene permiso para crear bases en SQL Server.

## Flujo diario

1. Registrar al cliente y su electrodoméstico.
2. Registrar empleados y proveedores. Añadir piezas, precio y existencias; el proveedor es opcional.
3. Crear una orden: elegir taller o domicilio. Para domicilio registrar la dirección del servicio.
4. Abrir la orden, asignar un empleado, escribir diagnóstico, mano de obra y fecha estimada.
5. Añadir las piezas utilizadas. Se descuenta stock y se debe registrar la aprobación del presupuesto actualizado.
6. Registrar anticipos y cobros. El sistema calcula el saldo y rechaza sobrepagos.
7. Pasar por revisión, reparación y reparado. Entregar cuando el saldo sea cero.
8. Imprimir el comprobante; el diálogo del navegador permite guardarlo en PDF.

La aprobación del presupuesto se registra por el operador al recibir autorización del cliente. Los métodos QR, tarjeta y transferencia registran pagos ya recibidos; no conectan con bancos ni realizan transacciones. La cancelación restituye las piezas, conserva el historial y se bloquea si hay pagos pendientes de conciliar. No se implementa devolución de dinero automática.

## Código en Visual Studio

Abrir `TallerElectrodomesticos.sln`, o el acceso directo **Taller - Código en Visual Studio**. Incluye:

- `src/Taller.Web`: modelos, controladores MVC, servicios, EF Core, vistas Razor y recursos locales Bootstrap.
- `src/Taller.Gateway`: YARP, entrada del sistema en el puerto 7180.
- `tests/Taller.Integration`: pruebas de reglas contra SQL Server temporal.
- `BaseDatos`: SQL y diseño nativo de Oracle Data Modeler.
- `docs`: manual, diccionario, diagramas y evidencias de verificación.
- `scripts`: publicación, inicio, detención y respaldo.

El perfil compartido `Taller completo con YARP` inicia los dos proyectos. Antes de depurar detener la distribución local para liberar los puertos 7180 y 7181. Visual Studio 2022 17.14 y SDK .NET 9 están instalados en esta PC; el proyecto utiliza net9.0, como PARABA.

Para volver a compilar y actualizar los ejecutables después de modificar el código:

```powershell
.\scripts\Detener.ps1
.\scripts\Preparar.ps1
.\scripts\Iniciar.ps1
```

## SQL Server y documentación

`BaseDatos/CrearBaseDatos.sql` instala el esquema completo (diez tablas, incluida `Egresos` de Cartera) en una base nueva. La aplicación inicializa una base nueva mediante EF Core; `EnsureCreated` no actualiza esquemas ya existentes. Los cambios de esquema se versionan con scripts revisables en `BaseDatos/Migraciones`.

El diseño `BaseDatos/DataModeler/TallerElectrodomesticos.dmd` y su carpeta compañera contienen nueve tablas / entidades y ocho relaciones. Abrirlo en Oracle SQL Developer Data Modeler. Los modelos lógico y físico tienen posiciones y recorridos manuales sin cruces de relaciones. La herramienta solo se usa para documentación.

El perfil SQL Server 2012 del modelador representa RowVersion como binary(8); SQL Server usa rowversion real. El DDL de importación explica esa adaptación y no debe ejecutarse en SQL Server. El esquema de instalación y el diccionario conservan los tipos reales.

## Respaldo y alcance local

Ejecutar `scripts/Respaldo.ps1`: crea un respaldo SQL Server COPY_ONLY con CHECKSUM, lo verifica y copia el archivo a `backups`. Guardar otra copia fuera de esta PC. El código del repositorio Git no reemplaza un respaldo de la base.

Esta versión se ejecuta solo en loopback para una PC de confianza; no tiene inicio de sesión ni roles de aplicación. El módulo de empleados es un registro del personal y su asignación a reparaciones. El uso por red o internet requiere añadir autenticación, permisos y configurar despliegue. No incluye facturación fiscal, nómina, compras ni agenda con horas.

## Verificación

```powershell
dotnet build TallerElectrodomesticos.sln -c Release
dotnet run --project tests/Taller.Integration -c Release -- docs/Verificacion-integracion.json
python tests/http_flow.py
```

Las pruebas crean bases temporales con nombres únicos, realizan los recorridos y las eliminan al terminar. `docs/Verificacion-integracion.json` y `docs/Verificacion-http.json` contienen los resultados. La base del negocio no recibe datos de prueba.

## Historial y referencias

Repositorio Git publicado en https://github.com/leandrojesusurgotgonzalessalas-byte/taller-gestion-reparacion-electrodomesticos. Los cambios quedan registrados por commits; el registro resumido está en `docs/RegistroTrabajo.md`.

Referencias técnicas: [YARP](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/config-files), [EF Core para SQL Server](https://learn.microsoft.com/ef/core/providers/sql-server/), [Perfiles múltiples de Visual Studio](https://learn.microsoft.com/visualstudio/ide/how-to-set-multiple-startup-projects).
