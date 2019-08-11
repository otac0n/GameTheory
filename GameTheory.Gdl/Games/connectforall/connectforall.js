//------------------------------------------------------------------------------
// connectforall visualization (includes image paths)
//------------------------------------------------------------------------------
 
function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('bgcolor','white');
  table.setAttribute('border','0');
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  cell.appendChild(makeTableC3(state));
  cell = row.insertCell(1);
  cell.appendChild(makeTableFFA(state));
  return table}
 
  function makeTableC3(state){
  var tableC3 = document.createElement('table');
  tableC3.setAttribute('border','10');
  makerowC3(tableC3,0,state);
  makerowC3(tableC3,1,state);
  makerowC3(tableC3,2,state);
  makerowC3(tableC3,3,state);
  makerowC3(tableC3,4,state);
  makerowC3(tableC3,5,state);
  return tableC3}
 
  function makeTableFFA(state){
   var tableFFA = document.createElement('table');
   tableFFA.setAttribute('border','10');
   makerowFFA(tableFFA,0,state);
   makerowFFA(tableFFA,1,state);
   makerowFFA(tableFFA,2,state);
   makerowFFA(tableFFA,3,state);
   makerowFFA(tableFFA,4,state);
   makerowFFA(tableFFA,5,state);
  return tableFFA}
 
function makerowC3 (table,rownum,state)
 {var row =table.insertRow(rownum);
  makecellC3(row,rownum,0,state);
  makecellC3(row,rownum,1,state);
  makecellC3(row,rownum,2,state);
  makecellC3(row,rownum,3,state);
  return row}
 
function makecellC3 (row,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  rownum = (6-rownum).toString();
  colnum = (colnum+1).toString();
  var mark = viewfindx('Z',seq('cell2',colnum,rownum,'Z'),state,seq());
  var pic;
  if(mark == 'red') pic = "<img src='/ggp/games/connectforall/red.png' height='35' width='35'/>";
  if(mark == 'black') pic = "<img src='/ggp/games/connectforall/black.png' height='35' width='35'/>";
  if (pic) {cell.innerHTML = pic} else {cell.innerHTML = ''};
  return cell}
 
 
function makerowFFA (table,rownum,state)
 {var row =table.insertRow(rownum);
  makecellFFA(row,rownum,0,state);
  makecellFFA(row,rownum,1,state);
  makecellFFA(row,rownum,2,state);
  makecellFFA(row,rownum,3,state);
  makecellFFA(row,rownum,4,state);
  makecellFFA(row,rownum,5,state);
  return row}
 
 
function makecellFFA (row,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  cell.style.fontSize = '15px';
  rownum = (rownum+1).toString();
  colnum = (colnum+1).toString();
  var piecetype = viewfindx('X',seq('cell',rownum,colnum,'red','X'),state,seq());
  if(!piecetype) piecetype = viewfindx('X',seq('cell',rownum,colnum,'black','X'),state,seq());
  if(piecetype){
	var mark = viewfindx('X',seq('cell',rownum,colnum,'X',piecetype),state,seq());
	var pic;
        if (piecetype == 'knight' && mark == 'red') pic = "<img id='redknight' src='/ggp/games/connectforall/Red_Knight.png' height='35' width='35'/>";
        if (piecetype == 'knight' && mark == 'black') pic = "<img id='blackknight' src='/ggp/games/connectforall/Black_Knight.png' height='35' width='35'/>";
	if (piecetype == 'pawn' && mark == 'red') pic = "<img id='redpawn' src='/ggp/games/connectforall/Red_Pawn.png' height='35' width='35'/>";
	if (piecetype == 'pawn' && mark == 'black') pic = "<img id='blackpawn' src='/ggp/games/connectforall/Black_Pawn.png' height='35' width='35'/>";


            	if (pic) {cell.innerHTML = pic} else {cell.innerHTML = ''};
            	};
  return cell}
 
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
