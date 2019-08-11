//------------------------------------------------------------------------------
// hitori2
//------------------------------------------------------------------------------
var dim = 8;
function renderstate (state)
{var table = document.createElement('table');
table.setAttribute('cellspacing','0');
table.setAttribute('border','2');
for (var i = 0; i < dim; i++) makerow(table, i, state);
return table}
function makerow (table,rownum,state)
{var row =table.insertRow(rownum);
for (var i = 0; i < dim; i++) makecell(row,rownum,i,state);
return row}
function makecell (row,rownum,colnum,state)
{var cell = row.insertCell(colnum);
cell.setAttribute('width','40');
cell.setAttribute('height','40');
cell.setAttribute('align','center');
cell.setAttribute('valign','center');
cell.setAttribute('style','font-family:helvetica;font-size:18pt');
rownum = (rownum+1).toString();
colnum = (colnum+1).toString();
var mark = compfindx('Z',seq('cell',rownum,colnum,'Z'),state,seq());
if (mark && mark != 'b') {cell.innerHTML = mark} else {cell.innerHTML = '&nbsp;'};
return cell}
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
