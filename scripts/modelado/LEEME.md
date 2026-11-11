# Herramientas de documentación nativa

Se conserva el código de importación, distribución y validación que produjo los archivos de Data Modeler. Utiliza bibliotecas de Oracle Data Modeler 24.3.1 instalado en esta PC y Python 3. Los scripts no escriben en SQL Server.

1. Actualizar ModeloFisicoSQLServer.ddl desde la aplicación con --schema, usando su carpeta como directorio de trabajo.
2. Ejecutar create_models.py para importar el DDL con las bibliotecas oficiales de Oracle. Conserva el diseño anterior en runtime/modelado.
3. Ejecutar layout_models.py para preparar entidades lógicas, cardinalidades y vistas sin cruces, y SVG / diccionario.
4. Ejecutar verify_models.py para validar nueve tablas, ocho FK, nueve entidades y ocho relaciones y comprobar los conectores.

La distribución de posiciones corresponde a este esquema de nueve tablas. Si el esquema cambia, hay que revisar las posiciones y las expectativas de validación. Los scripts Java usan APIs de documentación de Oracle; no son proyectos de ejecución del taller.
