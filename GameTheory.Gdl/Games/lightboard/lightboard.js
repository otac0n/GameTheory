//------------------------------------------------------------------------------
// lightboard
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('bgcolor','#f4f6f8');
  for (var i=0; i<8; i++) {makerow(table,i,state)};
  return table}

function makerow (table,rownum,state)
 {var row =table.insertRow(rownum);
  for (var j=0; j<8; j++) {makecell(row,rownum,j,state)};
  return row}

function makecell (row,rownum,colnum,state)
 {var cell = row.insertCell(row.cells.length);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  if (compfindp(seq('on',(rownum+1).toString(),(colnum+1).toString()),state,seq()))
     {cell.innerHTML = '<img src="/ggp/games/lightboard/green.jpg"/>'}
     else {cell.innerHTML = '<img src="/ggp/games/lightboard/red.jpg"/>'};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
