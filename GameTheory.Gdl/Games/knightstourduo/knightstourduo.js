//------------------------------------------------------------------------------
// knights tour duo
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  //table.setAttribute('bgcolor','white');
  table.setAttribute('border','10');
  makerow(table,0,state);
  makerow(table,1,state);
  makerow(table,2,state);
  makerow(table,3,state);
  makerow(table,4,state);
  makerow(table,5,state);
  return table}

function makerow (table,rownum,state)
 {var row =table.insertRow(rownum);
  makecell(row,rownum,0,state);
  makecell(row,rownum,1,state);
  makecell(row,rownum,2,state);
  makecell(row,rownum,3,state);
  makecell(row,rownum,4,state);
  makecell(row,rownum,5,state);
  makecell(row,rownum,6,state);
  makecell(row,rownum,7,state);
  makecell(row,rownum,8,state);
  makecell(row,rownum,9,state);
  return row}

function makecell (row,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  rownum = (rownum+1).toString();
  colnum = (colnum+1).toString();
  var mark = compfindx('Z',seq('cell',rownum,colnum,'Z'),state,seq());
  if (mark=='knight1') {cell.innerHTML = '<img src="http://games.ggp.org/base/resources/images/chess/White_Knight.png" width="36" height="36"/>'};
  if (mark=='knight2') {cell.innerHTML = '<img src="http://games.ggp.org/base/resources/images/chess/Black_Knight.png" width="36" height="36"/>'};
  if (mark=='hole') {cell.setAttribute('bgcolor','#000000'); cell.innerHTML = '&nbsp;'};
  if (mark==false) {cell.innerHTML = '&nbsp;'};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
