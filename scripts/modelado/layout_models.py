from pathlib import Path
import xml.etree.ElementTree as E,uuid,copy,re,json,html
root=Path(__file__).resolve().parents[2]
folder=root/'BaseDatos/DataModeler/TallerElectrodomesticos'
relpath=next((folder/'rel').glob('*.xml'));rel=E.parse(relpath).getroot();log=E.parse(folder/'logical/Logical.xml').getroot()
def uid():return str(uuid.uuid4()).upper()
def el(p,n,t):v=E.SubElement(p,n);v.text=str(t);return v
def save(p,r):p.parent.mkdir(parents=True,exist_ok=True);E.indent(r,space='  ');E.ElementTree(r).write(p,encoding='utf-8',xml_declaration=True)
tables=rel.findall('./Tables/Table');fks=rel.findall('./ForeignKeys/ForeignKey');byid={t.get('id'):t for t in tables}
assert len(tables)==9 and len(fks)==8
log.set('name','ModeloLogicoTaller');entities=E.SubElement(log,'Entities');relations=E.SubElement(log,'Relationships')
entityids={};attrids={};relationids={}
names={'Clientes':'Cliente','Equipos':'Electrodoméstico','Ordenes':'Orden de reparación','Pagos':'Pago','HistorialOrdenes':'Evento de orden','OrdenRepuestos':'Repuesto utilizado','Repuestos':'Repuesto','Empleados':'Empleado','Proveedores':'Proveedor'}
for t in tables:
 eid=uid();entityids[t.get('id')]=eid
 entity=E.SubElement(entities,'Entity',{'class':'oracle.dbtools.crest.model.design.logical.Entity','id':eid,'name':names[t.get('name')],'directorySegmentName':'seg_0'})
 el(entity,'ownerDesignName','TallerElectrodomesticos');el(entity,'normalForm','Third');el(entity,'fwdEngineeringStrategyName','Single Table')
 attrs=E.SubElement(entity,'attributes',{'itemClass':'oracle.dbtools.crest.model.design.logical.Attribute'})
 for c in t.findall('./columns/Column'):
  aid=uid();attrids[c.get('id')]=aid;a=E.SubElement(attrs,'Attribute',{'id':aid,'name':c.get('name')})
  for tag in ['ownerDesignName','nullsAllowed','use','logicalDatatype','domain','ownDataTypeParameters']:
   p=c.find(tag)
   if p is not None:a.append(copy.deepcopy(p))
 keys=E.SubElement(entity,'identifiers',{'itemClass':'oracle.dbtools.crest.model.design.logical.CandidateKey'})
 for idx in t.findall('./indexes/ind_PK_UK'):
  if idx.findtext('pk')=='true' or idx.findtext('indexState')=='Unique Constraint':
   key=E.SubElement(keys,'identifier',{'id':uid(),'name':idx.get('name')});el(key,'pk',idx.findtext('pk','false'));used=E.SubElement(key,'usedAttributes')
   for usage in idx.findall('./indexColumnUsage/colUsage'):el(used,'attributeRef',attrids[usage.get('columnID')])
for fk in fks:
 rid=uid();relationids[fk.get('id')]=rid
 r=E.SubElement(relations,'Relationship',{'class':'oracle.dbtools.crest.model.design.logical.Relation','id':rid,'name':fk.get('name'),'directorySegmentName':'seg_0'})
 optional=byid[fk.findtext('referredTableID')].get('name') in ['Empleados','Proveedores']
 for n,v in [('ownerDesignName','TallerElectrodomesticos'),('identifying','false'),('optionalSource',str(optional).lower()),('optionalTarget','true'),('sourceCardinality','1'),('sourceEntity',entityids[fk.findtext('referredTableID')]),('targetCardinalityString','*'),('targetEntity',entityids[fk.get('containerWithKeyObject')]),('transferable','true')]:el(r,n,v)
positions={'Clientes':(50,40),'Equipos':(510,40),'Empleados':(970,40),'Ordenes':(970,430),'Pagos':(1490,430),'HistorialOrdenes':(1490,800),'OrdenRepuestos':(970,1300),'Repuestos':(510,1300),'Proveedores':(50,1300)}
boxes={t.get('name'):(*positions[t.get('name')],370,80+26*len(t.findall('./columns/Column'))) for t in tables}
routes={
 ('Clientes','Equipos'):[(420,180),(510,180)],
 ('Equipos','Ordenes'):[(880,200),(925,200),(925,560),(970,560)],
 ('Empleados','Ordenes'):[(1155,boxes['Empleados'][1]+boxes['Empleados'][3]),(1155,430)],
 ('Ordenes','Pagos'):[(1340,550),(1490,550)],
 ('Ordenes','HistorialOrdenes'):[(1340,910),(1490,910)],
 ('Ordenes','OrdenRepuestos'):[(1155,boxes['Ordenes'][1]+boxes['Ordenes'][3]),(1155,1300)],
 ('Repuestos','OrdenRepuestos'):[(880,1430),(970,1430)],
 ('Proveedores','Repuestos'):[(420,1430),(510,1430)]}
for model,islogical in [(rel,False),(log,True)]:
 view=model.find('mainView');view.set('name',model.get('name'))
 view.find('display').text='true';view.find('showLabels').text='false'
 for tag in ['objectViews','connectors']:
  node=view.find(tag)
  if node is not None:view.remove(node)
 objs=E.SubElement(view,'objectViews');cons=E.SubElement(view,'connectors');vids={}
 for t in tables:
  tid=t.get('id');vids[tid]=uid();x,y,w,h=boxes[t.get('name')]
  v=E.SubElement(objs,'OView',{'class':'oracle.dbtools.crest.swingui.'+('logical.TVEntity' if islogical else 'relational.TVTable'),'oid':entityids[tid] if islogical else tid,'otype':'Entity' if islogical else 'Table','vid':vids[tid]})
  E.SubElement(v,'bounds',{'x':str(x),'y':str(y),'width':str(w),'height':str(h)})
 for fk in fks:
  parent=fk.findtext('referredTableID');child=fk.get('containerWithKeyObject');route=routes[(byid[parent].get('name'),byid[child].get('name'))]
  c=E.SubElement(cons,'Connector',{'class':'oracle.dbtools.crest.swingui.'+('logical.TVRelation' if islogical else 'relational.TVFKRelation'),'oid':relationids[fk.get('id')] if islogical else fk.get('id'),'otype':'Relation' if islogical else 'FKIndexAssociation','vid_source':vids[parent],'vid_target':vids[child]})
  el(c,'lineWidth','1');points=E.SubElement(c,'points')
  for x,y in route:E.SubElement(points,'point',{'x':str(x),'y':str(y)})
 model.find('shouldBeOpen').text='true'
save(relpath,rel);save(folder/'logical/Logical.xml',log)
# Canonical model contains all definitions inline, so its external cache files are not authoritative.
# Keep the native format and directories returned by Oracle's serializer.
ddl=(root/'BaseDatos/DataModeler/ModeloFisicoSQLServer.ddl').read_text(encoding='utf-8-sig')
schema={}
for match in re.finditer(r'CREATE TABLE \[(\w+)\] \((.*?)\n\);',ddl,re.S):
 cols=[]
 for name,type_,rest in re.findall(r'^\s*\[(\w+)\]\s+([\w]+(?:\([\d,]+\))?)\s+(.*?)(?:,)?$',match[2],re.M):cols.append({'name':name,'type':type_,'nullable':'NOT NULL' not in rest})
 schema[match[1]]=cols
fkcols={fk.get('containerWithKeyObject'):[] for fk in fks}
for fk in fks:
 t=byid[fk.get('containerWithKeyObject')];idx=next(i for i in t.findall('./indexes/ind_PK_UK') if i.get('id')==fk.get('localFKIndex'))
 for u in idx.findall('./indexColumnUsage/colUsage'):
  fkcols[t.get('id')].append(next(c.get('name') for c in t.findall('./columns/Column') if c.get('id')==u.get('columnID')))
for logical in [False,True]:
 title='Modelo lógico' if logical else 'Modelo físico · SQL Server'
 svg=['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1930 1680" width="1930" height="1680">','<rect width="1930" height="1680" fill="#f8fafb"/>',f'<text x="50" y="1640" font-family="Arial" font-size="18" fill="#567">TALLER DE GESTION DE REPARACION DE ELECTRODOMESTICOS · {title} · 9 entidades / tablas · 8 relaciones</text>']
 for key,points in routes.items():
  svg.append('<polyline points="'+' '.join(f'{x},{y}' for x,y in points)+'" fill="none" stroke="#268e88" stroke-width="3"/>')
  x,y=points[0];ex,ey=points[-1];card='0..1' if key[0] in ['Empleados','Proveedores'] else '1';svg.append(f'<text x="{x+9}" y="{y-8}" font-family="Arial" font-size="15" fill="#147d79">{card}</text><text x="{ex-42}" y="{ey-8}" font-family="Arial" font-size="15" fill="#147d79">0..N</text>')
 for t in tables:
  n=t.get('name');x,y,w,h=boxes[n];svg.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="8" fill="white" stroke="#aac4cc"/><path d="M{x+8} {y}h{w-16}q8 0 8 8v34h-{w}v-34q0-8 8-8" fill="#173d49"/>')
  svg.append(f'<text x="{x+18}" y="{y+28}" fill="white" font-size="19" font-family="Arial" font-weight="bold">{html.escape(names[n] if logical else n)}</text>')
  for i,c in enumerate(schema[n]):
   name=c['name'];key=' PK' if name=='Id' else ' FK' if name in fkcols.get(t.get('id'),[]) else ''
   typ= c['type'] if not logical else ('Texto' if 'char' in c['type'] else 'Importe' if 'decimal' in c['type'] else 'Fecha' if 'date' in c['type'] else 'Sí / No' if c['type']=='bit' else 'Versión' if c['type']=='rowversion' else 'Entero')
   text=name+key+' : '+typ+(' · opcional' if c['nullable'] else '')
   svg.append(f'<text x="{x+18}" y="{y+68+i*26}" font-size="15" font-family="Arial" fill="#294651">{html.escape(text)}</text>')
 svg.append('</svg>');(root/'docs'/('ModeloLogico.svg' if logical else 'ModeloFisico.svg')).write_text('\n'.join(svg),encoding='utf-8')
(root/'docs/Esquema.json').write_text(json.dumps(schema,ensure_ascii=False,indent=2),encoding='utf-8')
dictionary=['# Diccionario de datos','','Origen: esquema EF Core instalado en SQL Server. Importes en bolivianos con decimal(18,2).','','Las referencias a empleado y proveedor son opcionales; las demás son obligatorias. Todas tienen eliminación restringida (NO ACTION). Un padre puede tener cero o muchos hijos.','']
for n,cols in schema.items():
 dictionary+=['## '+n,'','| Campo | Tipo SQL Server | Permite nulos |','|---|---|---|']+[f"| {c['name']} | {c['type']} | {'Sí' if c['nullable'] else 'No'} |" for c in cols]+['']
dictionary+=['## Particularidades','','- Id es int IDENTITY y clave primaria.','- RowVersion es rowversion generado por SQL Server para controlar ediciones concurrentes. El perfil SQL Server 2012 de Data Modeler lo representa como binary(8); no use el DDL de importación para instalar la base.','- Status guarda los valores del enum RepairStatus: 0 recibido, 1 en revisión, 2 esperando aprobación, 3 esperando repuesto, 4 en reparación, 5 reparado, 6 entregado, 7 cancelado.','- Fechas operativas se guardan en UTC y se muestran en America/La_Paz; DueDate es una fecha de calendario.','- Código de repuesto único; stock y precios no negativos; cantidad de línea y pagos positivos.','- Ordenes.Total, Paid y Balance se calculan, no son columnas persistidas.']
(root/'docs/DiccionarioDatos.md').write_text('\n'.join(dictionary),encoding='utf-8')
print('Modelo nativo: 9 tablas, 8 FK, 9 entidades, 8 relaciones. Diagramas distribuidos sin cruces.')
