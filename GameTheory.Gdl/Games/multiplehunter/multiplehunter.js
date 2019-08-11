//------------------------------------------------------------------------------
// Multiplehunter
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('border','0');
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  cell.appendChild(maketable('1',state));
  cell = row.insertCell(1);
  cell.appendChild(maketable('2',state));
  cell = row.insertCell(2);
  cell.appendChild(maketable('3',state));
  row = table.insertRow(1);
  cell = row.insertCell(0);
  cell.appendChild(maketable('4',state));
  cell = row.insertCell(1);
  cell.appendChild(maketable('5',state));
  cell = row.insertCell(2);
  cell.appendChild(maketable('6',state));
  row = table.insertRow(2);
  cell = row.insertCell(0);
  cell.appendChild(maketable('7',state));
  cell = row.insertCell(1);
  cell.appendChild(maketable('8',state));
  cell = row.insertCell(2);
  cell.appendChild(maketable('9',state));
  return table}

function maketable (component,state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('bgcolor','white');
  table.setAttribute('border','10');
  makerow(table,component,0,state);
  makerow(table,component,1,state);
  makerow(table,component,2,state);
  makerow(table,component,3,state);
  makerow(table,component,4,state);
  return table}

function makerow (table,component,rownum,state)
 {var row =table.insertRow(rownum);
  makecell(row,component,rownum,0,state);
  makecell(row,component,rownum,1,state);
  makecell(row,component,rownum,2,state);
  return row}

function makecell (row,component,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  rownum = (rownum+1).toString();
  colnum = (colnum+1).toString();
  var mark = viewfindx('Z',seq('cell',component,rownum,colnum,'Z'),state,seq());
  if (mark=='knight') {cell.innerHTML = 'K'};
  if (mark=='pawn') {cell.innerHTML = 'p'};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
