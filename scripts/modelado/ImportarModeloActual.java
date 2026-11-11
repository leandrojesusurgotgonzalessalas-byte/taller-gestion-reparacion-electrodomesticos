import java.io.*;
import java.nio.file.*;
import java.util.*;
import java.lang.reflect.*;
import oracle.dbtools.crest.swingui.ApplicationView;
import oracle.dbtools.crest.model.design.Design;
import oracle.dbtools.crest.model.design.relational.RelationalDesign;
import oracle.dbtools.crest.model.design.storage.*;
import oracle.dbtools.crest.imports.ddl.DDLHandler;
import oracle.dbtools.crest.imports.ddl.sqlserver.DDLStatementHandlerSqlServer;
import oracle.dbtools.crest.model.metadata.MetaFileLoader;
import oracle.dbtools.crest.model.metadata.XMLTransformationManager;
import oracle.dbtools.crest.util.logging.ImportLogger;
import oracle.dbtools.crest.imports.parser.ScriptParser;
class ImportarModeloActual {
 public static void main(String[] args) throws Exception {
  MetaFileLoader.standalone=true;
  MetaFileLoader.standaloneBaseDir=Path.of(args[0]).toAbsolutePath().getParent().toString();
  ApplicationView aplicacion=ApplicationView.getInstance(false,args[0]);
  Design diseno=aplicacion.getCurrentDesign();
  diseno.setName("TallerElectrodomesticos");
  RelationalDesign relacional=diseno.addRelationalDesign("ModeloFisicoSQLServer");diseno.setRelationalDesign(relacional);
  relacional.setName("ModeloFisicoSQLServer");
  RDBMSType sqlserver=null;
  for(Iterator i=Design.getDefaultRDBMSTypeSet().iterator();i.hasNext();){
   RDBMSType tipo=(RDBMSType)i.next();
   if(tipo.getLongName().contains("SQL Server")){System.out.println("Motor: "+tipo.getLongName()+" "+tipo.getType()); if(tipo.getLongName().contains("2012"))sqlserver=tipo;}
  }
  if(sqlserver==null)throw new Exception("No se encontró SQL Server 2012");
  RDBMSSite sitio=diseno.createRDBMSSite();sitio.setName("SQL Server Taller");sitio.setType(sqlserver);sitio.setTypeID(sqlserver.getType());
  diseno.setSelectedRDBMSSite(sitio);relacional.setSelectedRDBMSSite(sitio);
  StorageDesign fisico=diseno.getStorageDesign(sitio);
  String ddl=Files.readString(Path.of(args[1]));
  ScriptParser parser=new ScriptParser(ddl);parser.parse();
  System.out.println("Sentencias: "+parser.getSqlStatements().length);
  DDLHandler importador=new DDLHandler(aplicacion,relacional,false,true,fisico);
  for(String nombre:new String[]{"design","designDomains"}) {Field campo=DDLHandler.class.getDeclaredField(nombre);campo.setAccessible(true);campo.set(importador,diseno);}
  Field sitioCampo=DDLHandler.class.getDeclaredField("site");sitioCampo.setAccessible(true);sitioCampo.set(importador,sitio);
  Field motorCampo=DDLHandler.class.getDeclaredField("dbType");motorCampo.setAccessible(true);motorCampo.setInt(importador,13);
  LinkedList<String> sentencias=new LinkedList<>();for(var sentencia:parser.getSqlStatements())sentencias.add(sentencia.getSql());
  Field sentenciasCampo=DDLHandler.class.getDeclaredField("statementCollection");sentenciasCampo.setAccessible(true);sentenciasCampo.set(importador,sentencias);
  Method dominios=DDLHandler.class.getDeclaredMethod("constructDomains");dominios.setAccessible(true);dominios.invoke(importador);
  Method construir=DDLHandler.class.getDeclaredMethod("buildSqlServerv2k12Chain");construir.setAccessible(true);construir.invoke(importador);
  Field cadenaCampo=DDLHandler.class.getDeclaredField("chainSqlServer");cadenaCampo.setAccessible(true);
  DDLStatementHandlerSqlServer cadena=(DDLStatementHandlerSqlServer)cadenaCampo.get(importador);
  ImportLogger registro=new ImportLogger();
  cadena.setImportLog(registro);
  for(var sentencia:parser.getSqlStatements()) {
    String texto=sentencia.getSql().trim();if(texto.equalsIgnoreCase("GO") || texto.isEmpty() || texto.startsWith("--"))continue;
    cadena.nextHandler(texto);
  }
  cadena.getCreateTableHandler().setImportLog(registro);
  cadena.getCreateTableHandler().processForeignKeys();
  cadena.getAlterTableHandler().setImportLog(registro);
  cadena.getAlterTableHandler().processForeignKeys();
  System.out.println("Tablas: "+relacional.getTableSet().size()+", relaciones: "+relacional.getFKIndexAssociationSet().size());
  System.out.println("Errores: "+registro.getErrors());
  diseno.getReverseEngineering().execute(new HashMap());
  diseno.getLogicalDesign().setName("ModeloLogicoTaller");
  System.out.println("Entidades lógicas: "+diseno.getLogicalDesign().getEntitySet().size());
  System.out.println("Modelo importado: "+relacional.getName());
  System.out.println("Modelos registrados: "+diseno.getRelationalDesigns().size());
  Path destino=Path.of(args[2]).toAbsolutePath();
  diseno.setDesignPath(destino.getParent().toString());diseno.setFileName("TallerElectrodomesticos");
  try {System.out.println("Guardado: "+diseno.saveAll());}catch(Exception error){System.out.println("Verificar archivos guardados: "+error.getMessage());}
  XMLTransformationManager gestor=new XMLTransformationManager();
  gestor.saveDesignFile(diseno);gestor.saveDomains(diseno);
  String identificador=relacional.getObjectID();String corto=identificador.substring(0,8)+"-"+identificador.substring(24);
  Path carpeta=destino.getParent().resolve("TallerElectrodomesticos");
  System.out.println("Modelo relacional guardado: "+gestor.saveObjectWithExternals(carpeta.resolve("rel").resolve(corto+".xml").toString(),relacional));
  gestor.saveObjectWithExternals(carpeta.resolve("logical").resolve("Logical.xml").toString(),diseno.getLogicalDesign());
  Path carpetaFisica=carpeta.resolve("rel").resolve(corto).resolve("phys").resolve(sitio.getShortObjectID());Files.createDirectories(carpetaFisica);
  System.out.println("Modelo físico guardado: "+gestor.saveObjectWithExternals(carpetaFisica.resolve(sitio.getShortObjectID()+".xml").toString(),fisico));
  System.exit(0);
 }
}
