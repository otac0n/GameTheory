//------------------------------------------------------------------------------
// buttonsandlights
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('bgcolor','white');
  table.setAttribute('border','4');
  makerow(table,0,state);
  return table}

function makerow (table,rownum,state)
 {var row =table.insertRow(rownum);
  makecell(row,'p',state);
  makecell(row,'q',state);
  makecell(row,'r',state);
  return row}

function makecell (row,light,state)
 {var cell = row.insertCell(row.cells.length);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  if (compfindp(light,state,seq()))
     {cell.innerHTML = '<img src="/ggp/games/buttonsandlights/green.jpg"/>'}
     else {cell.innerHTML = '<img src="/ggp/games/buttonsandlights/red.jpg"/>'};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
