//------------------------------------------------------------------------------
// Dual Rainbow
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('border','0');
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  cell.appendChild(maketable('left',state));
  cell = row.insertCell(1);
  cell.appendChild(maketable('right',state));
  return table}

function maketable (component,state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('bgcolor','white');
  table.setAttribute('border','10');
  var row =table.insertRow(0);
  makecell(row,component,'r1',state);
  makecell(row,component,'r2',state);
  row =table.insertRow(1);
  makecell(row,component,'r3',state);
  makecell(row,component,'r4',state);
  row =table.insertRow(2);
  makecell(row,component,'r5',state);
  makecell(row,component,'r6',state);
  return table}

function makecell (row,component,region,state)
 {var cell = row.insertCell(row.cells.length);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  var mark = compfindx('Z',seq('color',component,region,'Z'),state,seq());
  if (mark) {cell.setAttribute('bgcolor',mark)};
  cell.innerHTML='&nbsp;';
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
