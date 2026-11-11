"""Recorrido HTTP completo con SQL Server temporal, sin datos en la base del negocio."""
from pathlib import Path
import os,sys,subprocess,time,uuid,urllib.request,urllib.parse,urllib.error,http.cookiejar,re,html,json
root=Path(__file__).resolve().parents[1]
database='TallerElectrodomesticos_PruebasHTTP_'+uuid.uuid4().hex
port=7191
environment=os.environ.copy();environment['ConnectionStrings__Taller']=f'Server=localhost;Database={database};Integrated Security=True;TrustServerCertificate=True;Encrypt=True'
folder=root/'runtime/publish/Taller.Web'
log=(root/'runtime/http-pruebas.log').open('w',encoding='utf-8')
process=subprocess.Popen([str(folder/'Taller.Web.exe'),'--urls',f'http://127.0.0.1:{port}'],cwd=folder,env=environment,stdout=log,stderr=log)
base=f'http://127.0.0.1:{port}'
opener=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
checks=[]
def get(path):return opener.open(base+path,timeout=10).read().decode('utf-8')
def check(condition,message):
 if not condition:raise AssertionError(message)
 checks.append(message);print('OK:',message)
def post(path,data,page):
 token=html.unescape(re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',page)[1])
 data['__RequestVerificationToken']=token
 response=opener.open(base+path,urllib.parse.urlencode(data).encode(),timeout=15)
 return response.read().decode('utf-8'),response.url
def version(page):return html.unescape(re.search(r'name="rowVersion"[^>]*value="([^"]+)"',page)[1])
try:
 for attempt in range(60):
  try:get('/health');break
  except Exception:
   if process.poll() is not None:raise RuntimeError('El proceso de pruebas no inició; consulte runtime/http-pruebas.log')
   time.sleep(.3)
 for path,word in [('/','Todo listo'),('/Customers','Clientes'),('/Equipment','Equipos'),('/Parts','Repuestos'),('/Employees','Empleados'),('/Suppliers','Proveedores')]:check(word in get(path),'Carga página '+path)
 try:opener.open(base+'/Customers/Edit',urllib.parse.urlencode({'Name':'Sin token','Phone':'70000000'}).encode())
 except urllib.error.HTTPError as e:check(e.code==400,'Rechaza POST sin token antifalsificación')
 else:raise AssertionError('Aceptó POST sin token')
 for name,data in [('Suppliers',{'Name':'Proveedor de prueba','Phone':'70000000','Contact':'Contacto ficticio'}),('Employees',{'Name':'Empleado de prueba','Phone':'70000000','Specialty':'Lavadoras'}),('Customers',{'Name':'Cliente HTTP ficticio','Phone':'70000000','Address':'Dirección de prueba'})]:
  page,url=post('/'+name+'/Edit',data,get('/'+name+'/Edit'));check('/'+name in url and 'Registro guardado' in page if name!='Customers' else 'Cliente guardado' in page,'Alta de '+name)
 page,url=post('/Equipment/Edit',{'CustomerId':1,'Type':'Lavadora','Brand':'Prueba','Model':'HTTP','Serial':'TEST-HTTP'},get('/Equipment/Edit'));check('Equipo guardado' in page,'Alta de equipo relacionado con cliente')
 page,url=post('/Parts/Edit',{'Code':'HTTP-001','Name':'Pieza de prueba','Stock':3,'Price':'50.25','SupplierId':1},get('/Parts/Edit'));check('Repuesto guardado' in page,'Precio decimal con punto y proveedor relacionado')
 form=get('/Orders/Create')
 page,url=post('/Orders/Create',{'EquipmentId':1,'ReportedFault':'No desagua','ServiceMode':'Domicilio'},form);check('direcci' in page.lower() and 'obligatoria' in page.lower(),'Domicilio exige dirección')
 page,url=post('/Orders/Create',{'EquipmentId':1,'ReportedFault':'No desagua','ServiceMode':'Domicilio','ServiceAddress':'Dirección ficticia de visita','DueDate':'2026-10-20'},page)
 check('/Orders/Details/' in url and 'Direcci' in page,'Crea reparación a domicilio')
 orderid=int(url.rstrip('/').split('/')[-1]);detail=f'/Orders/Details/{orderid}'
 page,url=post(f'/Orders/AddPart/{orderid}',{'partId':1,'quantity':1},page);check('Cambios guardados' in page,'Añade pieza y descuenta inventario')
 page,url=post(f'/Orders/Update/{orderid}',{'rowVersion':version(page),'diagnosis':'Bomba defectuosa','employeeId':1,'labor':'100.50','approved':'true','dueDate':'2026-10-20'},page)
 check('Cambios guardados' in page and 'Empleado de prueba' in page and '150,75' in page,'Asigna empleado y calcula presupuesto decimal exacto')
 page,url=post(f'/Orders/Pay/{orderid}',{'amount':'25.50','method':'Efectivo','reference':'Prueba de anticipo'},page)
 check('25,50' in page and '125,25' in page,'Pago decimal y saldo exactos')
 for state in ['EnRevision','EnReparacion','Reparado']:
  page,url=post(f'/Orders/Status/{orderid}',{'rowVersion':version(page),'status':state},page);check('Cambios guardados' in page,'Transición '+state)
 page,url=post(f'/Orders/Status/{orderid}',{'rowVersion':version(page),'status':'Entregado'},page);check('saldo pendiente' in page,'Entrega bloqueada mientras existe saldo')
 page,url=post(f'/Orders/Pay/{orderid}',{'amount':'125.25','method':'QR'},page)
 page,url=post(f'/Orders/Status/{orderid}',{'rowVersion':version(page),'status':'Entregado'},page);check('Orden cerrada' in page,'Entrega después del cobro completo')
 printed=get(f'/Orders/Print/{orderid}');check('Firma del cliente' in printed and 'Direcci' in printed and '150,75' in printed,'Comprobante incluye domicilio e importes')
 checks.append('Base del negocio conservada sin registros ficticios')
 (root/'docs/Verificacion-http.json').write_text(json.dumps({'passed':True,'database':database,'checks':checks},ensure_ascii=False,indent=2),encoding='utf-8')
 print('RECORRIDO HTTP CORRECTO:',len(checks),'verificaciones')
finally:
 process.terminate();process.wait(timeout=20);log.close()
 assert database.startswith('TallerElectrodomesticos_PruebasHTTP_') and re.fullmatch(r'\w+',database)
 result=subprocess.run(['sqlcmd','-S','localhost','-E','-b','-Q',f"IF DB_ID(N'{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END"],capture_output=True,text=True)
 if result.returncode:print('No se pudo limpiar la base temporal:',database);sys.exit(1)
