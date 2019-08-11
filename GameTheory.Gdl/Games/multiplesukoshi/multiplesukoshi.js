//------------------------------------------------------------------------------
// multipleminisudoku
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('border','0');
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  cell.appendChild(maketable('1','1',state));
  cell = row.insertCell(1);
  cell.appendChild(maketable('1','2',state));
  cell = row.insertCell(2);
  cell.appendChild(maketable('1','3',state));
  row = table.insertRow(1);
  cell = row.insertCell(0);
  cell.appendChild(maketable('2','1',state));
  cell = row.insertCell(1);
  cell.appendChild(maketable('2','2',state));
  cell = row.insertCell(2);
  cell.appendChild(maketable('2','3',state));
  row = table.insertRow(2);
  cell = row.insertCell(0);
  cell.appendChild(maketable('3','1',state));
  cell = row.insertCell(1);
  cell.appendChild(maketable('3','2',state));
  cell = row.insertCell(2);
  cell.appendChild(maketable('3','3',state));
  return table}

function maketable (boardrow,boardcol,state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('border','2');
  makerow(table,boardrow,boardcol,0,state);
  makerow(table,boardrow,boardcol,1,state);
  makerow(table,boardrow,boardcol,2,state);
  return table}

function makerow (table,boardrow,boardcol,rownum,state)
 {var row =table.insertRow(rownum);
  makecell(row,boardrow,boardcol,rownum,0,state);
  makecell(row,boardrow,boardcol,rownum,1,state);
  makecell(row,boardrow,boardcol,rownum,2,state);
  return row}

function makecell (row,boardrow,boardcol,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  cell.setAttribute('style','font-family:helvetica;font-size:18pt');
  rownum = (rownum+1).toString();
  colnum = (colnum+1).toString();
  var mark = compfindx('Z',seq('cell',boardrow,boardcol,rownum,colnum,'Z'),state,seq());
  if (mark && mark != 'b') {cell.innerHTML = mark} else {cell.innerHTML = '&nbsp;'};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
