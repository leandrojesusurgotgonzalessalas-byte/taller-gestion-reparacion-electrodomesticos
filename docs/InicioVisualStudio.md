# Ejecutar desde Visual Studio

El acceso directo instalado usa Web 7181 y YARP 7180 en Production.
El perfil Taller.Web de Visual Studio usa Development y abre http://127.0.0.1:7281.
El perfil Taller.Gateway usa Development en http://127.0.0.1:7280 y redirige a Web 7281.
Los dos proyectos pueden iniciarse juntos en Visual Studio sin ocupar los puertos de la aplicación instalada.
Ambos entornos siguen usando la misma base SQL Server local: cualquier registro guardado es real.

Después de cambiar los perfiles, detener la depuración con Mayús+F5 y volver a iniciar con F5 para crear un proceso nuevo. Si Visual Studio pide recargar los archivos modificados, aceptar la recarga.
