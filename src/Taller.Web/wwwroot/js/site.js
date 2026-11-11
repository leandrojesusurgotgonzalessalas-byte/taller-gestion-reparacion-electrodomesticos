// Taller · comportamiento mínimo del panel. Sin dependencias (Bootstrap solo para offcanvas/collapse).
(function () {
  'use strict';

  // Catálogo de referencia: guarda el nombre real del formulario, sin asignar precio ni existencias.
  var referenciaPieza = document.getElementById('equipoReferenciaPieza');
  var listaPiezas = document.getElementById('piezasElectrodomesticos');
  var nombrePieza = document.querySelector('input[name="Name"][list="piezasElectrodomesticos"]');
  if (referenciaPieza && listaPiezas && nombrePieza) {
    var sugerenciasPiezas = Array.from(listaPiezas.options).map(function (o) { return o.cloneNode(true); });
    var filtrarPiezas = function () {
      var tipo = referenciaPieza.value;
      listaPiezas.replaceChildren();
      sugerenciasPiezas.forEach(function (o) {
        if (!tipo || o.dataset.equipo === tipo) listaPiezas.appendChild(o.cloneNode(true));
      });
    };
    var categoriaActual = sugerenciasPiezas.find(function (o) { return o.value === nombrePieza.value; });
    if (categoriaActual) referenciaPieza.value = categoriaActual.dataset.equipo;
    referenciaPieza.addEventListener('change', filtrarPiezas);
    filtrarPiezas();
  }

  document.querySelectorAll('table.table').forEach(function (table) {
    table.setAttribute('role', 'table');
    table.querySelectorAll('thead,tbody').forEach(function (e) { e.setAttribute('role', 'rowgroup'); });
    table.querySelectorAll('tr').forEach(function (e) { e.setAttribute('role', 'row'); });
    table.querySelectorAll('th').forEach(function (e) { e.setAttribute('role', 'columnheader'); });
    table.querySelectorAll('td').forEach(function (e) { e.setAttribute('role', 'cell'); });
  });

  var tabla = document.querySelector('.repairs');
  if (tabla) {
    // Roles ARIA explícitos: conservan la semántica de tabla cuando se ve como tarjetas en móvil.
    tabla.setAttribute('role', 'table');
    tabla.querySelectorAll('thead,tbody').forEach(function (e) { e.setAttribute('role', 'rowgroup'); });
    tabla.querySelectorAll('tr').forEach(function (e) { e.setAttribute('role', 'row'); });
    tabla.querySelectorAll('th').forEach(function (e) { e.setAttribute('role', 'columnheader'); });
    tabla.querySelectorAll('td').forEach(function (e) { e.setAttribute('role', 'cell'); });

    // Filtro por modalidad + búsqueda (en cliente, sobre las filas ya renderizadas).
    var filas = Array.prototype.slice.call(tabla.querySelectorAll('tbody tr'));
    var chips = document.querySelectorAll('.chip[data-filtro]');
    var buscar = document.getElementById('buscar');
    var conteo = document.getElementById('conteo');
    var vacio = document.getElementById('sinResultados');
    var filtro = 'todas';

    var norm = function (s) { return s.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, ''); };

    var aplicar = function () {
      var q = buscar ? norm(buscar.value.trim()) : '';
      var n = 0;
      filas.forEach(function (f) {
        var ok = (filtro === 'todas' || f.dataset.modalidad === filtro) && (!q || norm(f.textContent).indexOf(q) > -1);
        f.hidden = !ok;
        if (ok) n++;
      });
      if (conteo) conteo.textContent = 'Mostrando ' + n + ' de ' + filas.length + ' reparaciones recientes';
      if (vacio) vacio.hidden = n > 0;
    };

    chips.forEach(function (c) {
      c.addEventListener('click', function () {
        filtro = c.dataset.filtro;
        chips.forEach(function (o) { o.setAttribute('aria-pressed', o === c ? 'true' : 'false'); });
        aplicar();
      });
    });
    if (buscar) buscar.addEventListener('input', aplicar);
  }

  // Texto del botón de existencias bajas
  var det = document.getElementById('stockDetalle');
  var btn = document.getElementById('btnStock');
  if (det && btn) {
    det.addEventListener('show.bs.collapse', function () { btn.textContent = 'Ocultar detalle'; });
    det.addEventListener('hide.bs.collapse', function () { btn.textContent = 'Ver detalle'; });
  }
})();
