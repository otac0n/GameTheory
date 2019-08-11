//------------------------------------------------------------------------------
// simultaneoussukoshi
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('border','0');
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  cell.appendChild(maketable('red',state));
  cell = row.insertCell(1);
  cell.appendChild(maketable('green',state));
  cell = row.insertCell(2);
  cell.appendChild(maketable('blue',state));

  return table}

function maketable (board,state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('border','2');
  makerow(table,board,0,state);
  makerow(table,board,1,state);
  makerow(table,board,2,state);
  return table}

function makerow (table,board,rownum,state)
 {var row =table.insertRow(rownum);
  makecell(row,board,rownum,0,state);
  makecell(row,board,rownum,1,state);
  makecell(row,board,rownum,2,state);
  return row}

function makecell (row,board,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  cell.setAttribute('style','font-family:helvetica;font-size:18pt');
  rownum = (rownum+1).toString();
  colnum = (colnum+1).toString();
  var mark = compfindx('Z',seq('cell',board,rownum,colnum,'Z'),state,seq());
  if (mark && mark != 'b') {cell.innerHTML = mark} else {cell.innerHTML = '&nbsp;'};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
