//------------------------------------------------------------------------------
// pentago
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing',0);
  table.setAttribute('cellpadding',0);
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  cell.appendChild(makequadrant(1,state));
  cell = row.insertCell(1);
  cell.appendChild(makequadrant(2,state));
  row = table.insertRow(1);
  cell = row.insertCell(0);
  cell.appendChild(makequadrant(4,state));
  cell = row.insertCell(1);
  cell.appendChild(makequadrant(3,state));
  return table}

function makequadrant (quadrant,state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('border','4');
  makerow(table,quadrant,0,state);
  makerow(table,quadrant,1,state);
  makerow(table,quadrant,2,state);
  return table}

function makerow (table,quadrant,rownum,state)
 {var row = table.insertRow(rownum);
  makecell(row,quadrant,rownum,0,state);
  makecell(row,quadrant,rownum,1,state);
  makecell(row,quadrant,rownum,2,state);
  return row}

function makecell (row,quadrant,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','50');
  cell.setAttribute('height','50');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  cell.setAttribute('style','background-color:#cccccc');
  quadrant = quadrant.toString();
  rownum = (rownum+1).toString();
  colnum = (colnum+1).toString();
  cell.innerHTML = getimage(quadrant,rownum,colnum,state);
  return cell}

function getimage (quadrant,row,col,state)
 {var owner = compfindx('C',seq('cellholds',quadrant,row,col,'C'),state,library);
  if (owner==='red')
     {return '<img src="/ggp/games/pentago/red.png" width="40" height="40"/>'};
  if (owner==='black')
     {return '<img src="/ggp/games/pentago/black.png" width="40" height="40"/>'};
  return '&nbsp;'}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
