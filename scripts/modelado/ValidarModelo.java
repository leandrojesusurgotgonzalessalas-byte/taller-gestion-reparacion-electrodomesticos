import java.nio.file.*;
import oracle.dbtools.crest.swingui.ApplicationView;
import oracle.dbtools.crest.model.metadata.*;
import oracle.dbtools.crest.model.design.*;
class ValidarModelo {
 public static void main(String[] args)throws Exception {
  MetaFileLoader.standalone=true;MetaFileLoader.standaloneBaseDir=Path.of(args[0]).toAbsolutePath().getParent().toString();
  ApplicationView aplicacion=ApplicationView.getInstance(false,args[0]);Design diseno=aplicacion.getCurrentDesign();
  XMLTransformationManager gestor=new XMLTransformationManager();
  Path base=Path.of(args[1]).toAbsolutePath();diseno.setDesignPath(base.toString());diseno.setFileName("TallerElectrodomesticos");
  gestor.loadDomainFile(base.resolve("TallerElectrodomesticos/domains/TallerElectrodomesticos_Domains.xml").toFile(),diseno);
  var relacional=diseno.addRelationalDesign("ModeloFisicoSQLServer");diseno.setRelationalDesign(relacional);
  boolean rel=gestor.loadObjectWithExternals(Path.of(args[2]).toAbsolutePath().toString(),relacional);
  boolean log=gestor.loadObjectWithExternals(Path.of(args[3]).toAbsolutePath().toString(),diseno.getLogicalDesign());
  int tablas=relacional.getTableSet().size(),foraneas=relacional.getFKIndexAssociationSet().size();
  int entidades=diseno.getLogicalDesign().getEntitySet().size(),relaciones=diseno.getLogicalDesign().getRelationSet().size();
  System.out.println("RESULTADO: "+rel+" "+log+" tablas="+tablas+" clavesForaneas="+foraneas+" entidades="+entidades+" relaciones="+relaciones);
  if(!rel || !log || tablas!=9 || foraneas!=8 || entidades!=9 || relaciones!=8)System.exit(1);
  System.exit(0);
 }
}
