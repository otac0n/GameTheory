//------------------------------------------------------------------------------
// Rainbow
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('bgcolor','white');
  table.setAttribute('border','10');
  var row =table.insertRow(0);
  makecell(row,'r1',state);
  makecell(row,'r2',state);
  row =table.insertRow(1);
  makecell(row,'r3',state);
  makecell(row,'r4',state);
  row =table.insertRow(2);
  makecell(row,'r5',state);
  makecell(row,'r6',state);
  return table}

function makecell (row,region,state)
 {var cell = row.insertCell(row.cells.length);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  var mark = compfindx('Z',seq('color',region,'Z'),state,seq());
  if (mark) {cell.setAttribute('bgcolor',mark)};
  cell.innerHTML='&nbsp;';
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
