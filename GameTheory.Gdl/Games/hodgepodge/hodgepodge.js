//------------------------------------------------------------------------------
// multiplebuttonsandlights_9.js
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
  table.setAttribute('border','4');
  makerow(table,0,component,state);
  return table}

function makerow (table,rownum,component,state)
 {var row =table.insertRow(rownum);
  makecell(row,seq('p',component),state);
  makecell(row,seq('q',component),state);
  makecell(row,seq('r',component),state);
  return row}

function makecell (row,light,state)
 {var cell = row.insertCell(row.cells.length);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  if (viewfindp(light,state,seq()))
     {cell.innerHTML = '<img src="/ggp/games/buttonsandlights/green.jpg"/>'}
     else {cell.innerHTML = '<img src="/ggp/games/buttonsandlights/red.jpg"/>'};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
