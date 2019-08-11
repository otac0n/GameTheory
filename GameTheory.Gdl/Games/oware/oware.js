//----------------------------------------------------------------------------
// Oware visualization
//----------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('bgcolor','white');
  table.setAttribute('border','10');
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  makescorehouse(cell, 'stone',state);
  cell = row.insertCell(1);
  cell.appendChild(makeboard(state));
  cell = row.insertCell(2);
  makescorehouse(cell, 'rock', state);
  return table}

function makeboard (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('bgcolor','white');
  table.setAttribute('border','0');
  makerow(table, 0, state);
  makerow(table, 1, state);
  return table}

function makerow (table, rownum,state)
 {var row =table.insertRow(rownum);
  makecell(row, rownum,0,state);
  makecell(row, rownum,1,state);
  makecell(row, rownum,2,state);
  makecell(row, rownum,3,state);
  makecell(row, rownum,4,state);
  makecell(row, rownum,5,state);
  return row}

function makecell (row, rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  if(colnum > 0)cell.style.borderLeft = '1px black solid';
  if(rownum > 0) cell.style.borderTop = '1px black solid';
  if (rownum == 0) {
    rownum = 'stone';
    colnum = (12-colnum).toString();
  } else {
    rownum = 'rock';
    colnum = (colnum+1).toString();
  }
  var mark = viewfindx('Z',seq('cell', rownum, colnum,'Z'),state,seq());
  if (mark && mark != 'b') {cell.innerHTML = mark} else {cell.innerHTML = ''};
  return cell}

function makescorehouse (cell, role, state)
 {cell.setAttribute('width','40');
  cell.setAttribute('height','80');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  var mark = viewfindx('Z',seq('capture', role, 'Z'),state,seq());
  if (mark && mark != 'b') {cell.innerHTML = mark} else {cell.innerHTML = ''};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

