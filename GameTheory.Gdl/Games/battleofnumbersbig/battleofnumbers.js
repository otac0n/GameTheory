//------------------------------------------------------------------------------
// battleofnumbers
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('bgcolor','white');
  table.setAttribute('border','10');
  makerow(table,0,state);
  makerow(table,1,state);
  makerow(table,2,state);
  makerow(table,3,state);
  makerow(table,4,state);
  return table}

function makerow (table,rownum,state)
 {var row =table.insertRow(rownum);
  makecell(row,rownum,0,state);
  makecell(row,rownum,1,state);
  makecell(row,rownum,2,state);
  makecell(row,rownum,3,state);
  makecell(row,rownum,4,state);
  return row}

function makecell (row,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','50');
  cell.setAttribute('height','50');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  cell.setAttribute('style','font-family:helvetica;font-size:18pt');
  rownum = (rownum+1).toString();
  colnum = (colnum+1).toString();
  var greenmark = compfindx('Number', seq('cell', rownum, colnum, 'Number', 'green'), state, seq());
  var redmark = compfindx('Number', seq('cell', rownum, colnum, 'Number', 'red'), state, seq());
  if(greenmark){
        cell.innerHTML = greenmark;
        cell.setAttribute('bgcolor','#66CD00')
    } else if (redmark) {
        cell.innerHTML = redmark;
        cell.setAttribute('bgcolor','#FF0000')
    } else {
        cell.innerHTML = '&nbsp';
    };
    return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
