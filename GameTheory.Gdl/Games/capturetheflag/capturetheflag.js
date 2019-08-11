//------------------------------------------------------------------------------
// knights tour duo
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  //table.setAttribute('bgcolor','white');
  table.setAttribute('border','10');
  for (var i=0; i<5; i++) {
    makerow(table,i,state);
  }
  return table}

function makerow (table,rownum,state)
 {var row =table.insertRow(rownum);
  for (var i=0; i<10; i++) {
    makecell(row,rownum,i,state);
  }
  return row}

function makecell (row,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  rownum_str = (rownum+1).toString();
  colnum_str = (colnum+1).toString();
  var mark = compfinds('Z',seq('cell',rownum_str,colnum_str,'Z'),state,seq());
  if (colnum < 5) {cell.setAttribute('bgcolor','#BBDDFF');}
  else {cell.setAttribute('bgcolor','#FFDDBB');}
  
  if (mark.includes('flag1')) {cell.setAttribute('bgcolor','#FF0000');};
  if (mark.includes('flag4')){cell.setAttribute('bgcolor','#0000FF');};
  if (mark.includes('knight1')) {cell.innerHTML = '<img src="/ggp/games/capturetheflag/images/Red_Knight.png" width="36" height="36"/>'};
  if (mark.includes('knight2')) {cell.innerHTML = '<img src="/ggp/games/capturetheflag/images/DarkRed_Knight.png" width="36" height="36"/>'};
  if (mark.includes('knight3')) {cell.innerHTML = '<img src="/ggp/games/capturetheflag/images/Blue_Knight.png" width="36" height="36"/>'};
  if (mark.includes('knight4')) {cell.innerHTML = '<img src="/ggp/games/capturetheflag/images/DarkBlue_Knight.png" width="36" height="36"/>'};
  if (mark==false) {cell.innerHTML = '&nbsp;'};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
