//------------------------------------------------------------------------------
// tictactoe
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('border','0');
  for (var i = 0; i < 3; i++) {
    var row = table.insertRow(i);
    var cell = row.insertCell(0);
    cell.appendChild(maketable(i,'1',state));
    cell = row.insertCell(1);
    cell.appendChild(maketable(i,'2',state));
    cell = row.insertCell(2);
    cell.appendChild(maketable(i,'3',state));
    cell = row.insertCell(3);
    cell.appendChild(maketable(i,'4',state));
  }
  return table}

function maketable (i,component,state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('bgcolor','white');
  table.setAttribute('border','10');
  makerow(i,table,component,0,state);
  makerow(i,table,component,1,state);
  makerow(i,table,component,2,state);
  makerow(i,table,component,3,state);
  return table}

function makerow (i,table,component,rownum,state)
 {var row =table.insertRow(rownum);
  makecell(i,row,component,rownum,0,state);
  makecell(i,row,component,rownum,1,state);
  makecell(i,row,component,rownum,2,state);
  makecell(i,row,component,rownum,3,state);
  return row}

function makecell (i, row,component,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','30');
  cell.setAttribute('height','30');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  cell.setAttribute('style','font-family:helvetica;font-size:18pt');
  rownum = (rownum+1).toString();
  colnum = (colnum+1).toString();
  if (i == 0) var mark = compfindx('Z',seq('cell',component,rownum,colnum,'Z'),state,seq());
  if (i == 1) var mark = compfindx('Z',seq('cell',rownum,component,colnum,'Z'),state,seq());
  if (i == 2) var mark = compfindx('Z',seq('cell',rownum,colnum,component,'Z'),state,seq());
  if (mark && mark != 'b') {cell.innerHTML = mark} else {cell.innerHTML = '&nbsp;'};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
