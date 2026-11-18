# Agenda y avisos

En **Agenda**, el calendario semanal o mensual muestra cada orden abierta el día de su entrega estimada. El cronograma representa el período desde la recepción hasta la entrega, no las horas efectivas de trabajo. Las órdenes sin fecha aparecen en **Por programar**. Cambia la fecha desde **Editar orden**.

Los avisos usan SMTP con STARTTLS y un único destinatario del taller. Están desactivados hasta que el administrador configure el envío. No hay acceso al banco ni conexión con Google Calendar. Una solicitud se registra como una nueva orden; no existe un formulario público separado.

En la PC instalada, agrega una sección `AvisosCorreo` al archivo privado `runtime/publish/Taller.Web/appsettings.Local.json`, conservando cualquier otra sección existente. Para usar Visual Studio, el archivo privado va en `src/Taller.Web`. Puedes guardar la credencial como variable de entorno `AvisosCorreo__Clave` para evitar escribirla en el archivo. Usa una contraseña de aplicación cuando tu proveedor lo permita. No compartas ni subas ese archivo.

```json
{
  "AvisosCorreo": {
    "Habilitado": true,
    "Servidor": "smtp.ejemplo.com",
    "Puerto": 587,
    "Remitente": "taller@ejemplo.com",
    "Destinatario": "taller@ejemplo.com",
    "Usuario": "taller@ejemplo.com"
  }
}
```

Reinicia el taller tras configurar el correo. En la primera activación se empiezan a detectar nuevas órdenes y pagos desde ese momento. A partir de las 08:00 (Bolivia), cuando el taller está abierto, se prepara un resumen diario de trabajos vencidos, entregas de hoy o mañana y saldos aprobados por cobrar. Los avisos se revisan cada dos minutos. El envío se reintenta si falla y requiere que la PC y la aplicación estén encendidas. Antes de darlo por listo, registra una orden en un entorno de práctica y comprueba la recepción.
