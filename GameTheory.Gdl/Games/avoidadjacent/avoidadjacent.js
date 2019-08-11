function renderstate (state) {
  var table = document.createElement('table');
  table.setAttribute('cellspacing', '0');
  table.setAttribute('bgcolor', 'white');
  table.setAttribute('border', '10');
  makerow(table, 0, state);
  return table
}

function makerow (table, rownum, state) {
  var row = table.insertRow(rownum);
  for (var i = 0; i < 40; i++) { makecell(row, rownum, i, state); }
  return row
}

function isnb(mark) { return mark && mark != 'b'; }

function makecell (row, rownum, colnum, state) {
  var cell = row.insertCell(colnum);
  cell.setAttribute('width', '40');
  cell.setAttribute('height', '40');
  cell.setAttribute('align', 'center');
  cell.setAttribute('valign', 'center');
  cell.innerHTML = '&nbsp;';
  var mark = compfindx('Z', seq('cell', (colnum + 1).toString(), 'Z'), state, seq());
  var markleft = compfindx('Z', seq('cell', (colnum + 0).toString(), 'Z'), state, seq());
  var markright = compfindx('Z', seq('cell', (colnum + 2).toString(), 'Z'), state, seq());
  if (isnb(mark) || isnb(markleft) || isnb(markright)) cell.setAttribute('bgcolor', 'black');
  
  return cell
}
