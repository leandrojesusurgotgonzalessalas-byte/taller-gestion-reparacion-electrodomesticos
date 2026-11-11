from pathlib import Path
import xml.etree.ElementTree as E,subprocess,json,re,sys
root=Path(__file__).resolve().parents[2]
work=root/'runtime/modelado';work.mkdir(parents=True,exist_ok=True)
model=root/'BaseDatos/DataModeler/TallerElectrodomesticos'
relpath=next((model/'rel').glob('*.xml'));logpath=model/'logical/Logical.xml'
for name,p in [('relacional',relpath),('logico',logpath)]:
 r=E.parse(p).getroot();r.remove(r.find('mainView'));E.ElementTree(r).write(work/(name+'.xml'),encoding='utf-8',xml_declaration=True)
source=Path(__file__).with_name('ValidarModelo.java').read_text(encoding='utf-8').replace('GestionConstructora','TallerElectrodomesticos').replace('!=43','!=9').replace('!=129','!=8')
(work/'ValidarModelo.java').write_text(source,encoding='utf-8')
tool=Path(r'C:\Users\Samira\AppData\Local\Programs\Oracle\datamodeler')
args=['-Djava.awt.headless=true','-cp',';'.join(str(p) for p in tool.rglob('*.jar')),str(work/'ValidarModelo.java'),str(tool/'datamodeler/types'),str(root/'BaseDatos/DataModeler'),str(work/'relacional.xml'),str(work/'logico.xml')]
(work/'validate-args.txt').write_text('\n'.join('"'+a.replace('\\','/')+'"' for a in args),encoding='utf-8')
r=subprocess.run([str(tool/'jdk/jre/bin/java.exe'),'@'+str(work/'validate-args.txt')],capture_output=True,text=True,encoding='utf-8',errors='replace')
print(r.stdout[-2500:]);(work/'validacion-modeler.log').write_text(r.stdout+r.stderr,encoding='utf-8')
if r.returncode:print(r.stderr[-1800:]);sys.exit(r.returncode)
rel=E.parse(relpath).getroot();log=E.parse(logpath).getroot()
segments=[]
for connector in rel.findall('./mainView/connectors/Connector'):
 pts=[(int(p.get('x')),int(p.get('y'))) for p in connector.findall('./points/point')]
 for a,b in zip(pts,pts[1:]):segments.append((a,b,connector.get('oid')))
def cross(a,b,c,d):
 # All routes are orthogonal. Endpoints of shared nodes are not crossings.
 if a[1]==b[1] and c[0]==d[0]:return min(a[0],b[0])<c[0]<max(a[0],b[0]) and min(c[1],d[1])<a[1]<max(c[1],d[1])
 if a[0]==b[0] and c[1]==d[1]:return cross(c,d,a,b)
 return False
crossings=sum(cross(a,b,c,d) for i,(a,b,f) in enumerate(segments) for c,d,g in segments[i+1:] if f!=g)
assert crossings==0
result={'passed':True,'tool':'Oracle SQL Developer Data Modeler 24.3.1, bibliotecas oficiales','tables':len(rel.findall('./Tables/Table')),'foreignKeys':len(rel.findall('./ForeignKeys/ForeignKey')),'logicalEntities':len(log.findall('./Entities/Entity')),'logicalRelationships':len(log.findall('./Relationships/Relationship')),'connectorCrossings':crossings,'method':'Lectura nativa con XMLTransformationManager de Oracle y comprobación geométrica de conectores. Lectura sin vista gráfica para validar en modo headless.','rowVersionMapping':'binary(8) documental; rowversion en SQL Server real'}
(root/'docs/Verificacion-DataModeler.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print('Verificación nativa y diagramas:',json.dumps(result,ensure_ascii=False))
