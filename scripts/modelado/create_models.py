from pathlib import Path
import subprocess,json,re,sys,shutil,uuid
sys.stdout.reconfigure(encoding='utf-8')
root=Path(__file__).resolve().parents[2]
workspace=root/'runtime/modelado';workspace.mkdir(parents=True,exist_ok=True);(workspace/'work').mkdir(exist_ok=True)
tool=Path(r'C:\Users\Samira\AppData\Local\Programs\Oracle\datamodeler')
model=root/'BaseDatos/DataModeler'
ddl=(model/'ModeloFisicoSQLServer.ddl').read_text(encoding='utf-8-sig')
fks=[]
def extract(match):
 table,body=match.groups(); lines=[]
 for line in body.splitlines():
  if 'FOREIGN KEY' in line:fks.append('ALTER TABLE ['+table+'] ADD '+line.strip().rstrip(',')+';\nGO\n')
  else:lines.append(line)
 return 'CREATE TABLE ['+table+'] ('+'\n'.join(lines).rstrip().rstrip(',')+'\n);'
ddl=re.sub(r'CREATE TABLE \[(\w+)\] \((.*?)\n\);',extract,ddl,flags=re.S)
ddl=ddl.replace('rowversion','binary(8)')+'\n'+''.join(fks)
(model/'ModeloImportacion.ddl').write_text(ddl,encoding='utf-8')
for name in ['TallerElectrodomesticos','TallerElectrodomesticos.dmd']:
 p=model/name
 if p.exists():
  archive=workspace/'work'/(name+'.archivo-'+uuid.uuid4().hex[:8])
  assert p.resolve().is_relative_to(model.resolve())
  shutil.move(str(p),str(archive))
source=Path(__file__).with_name('ImportarModeloActual.java').read_text(encoding='utf-8')
source=source.replace('GestionConstructora','TallerElectrodomesticos').replace('ModeloLogicoConstructora','ModeloLogicoTaller').replace('SQL Server Constructora','SQL Server Taller')
java_source=workspace/'work/ImportarModeloActual.java';java_source.write_text(source,encoding='utf-8')
args=['-Djava.awt.headless=true','-cp',';'.join(str(p) for p in tool.rglob('*.jar')),str(java_source),str(tool/'datamodeler/types'),str(model/'ModeloImportacion.ddl'),str(model/'TallerElectrodomesticos.dmd')]
argfile=workspace/'work/modeler-args.txt'
argfile.write_text('\n'.join('"'+a.replace('\\','/')+'"' for a in args),encoding='utf-8')
r=subprocess.run([str(tool/'jdk/jre/bin/java.exe'),'@'+str(argfile)],capture_output=True,text=True,encoding='utf-8',errors='replace')
(workspace/'work/importacion-modeler.log').write_text(r.stdout+'\n'+r.stderr,encoding='utf-8')
print(r.stdout[:4000]);print('Código de salida:',r.returncode);sys.exit(r.returncode)
