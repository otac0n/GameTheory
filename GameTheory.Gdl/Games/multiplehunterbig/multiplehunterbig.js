//------------------------------------------------------------------------------
// hunter
//------------------------------------------------------------------------------

function renderstate (state)
 {
   var bigtable = document.createElement("table");
   rows = []
   for(let k = 0; k < 3; k++) {
     rows[k] = bigtable.insertRow(k);
     for (let j = 0; j < 4; j++) {
       let cell = rows[k].insertCell(j);
       let i = 3*k+j;
       var table = document.createElement('table');
        table.setAttribute('cellspacing','0');
        //table.setAttribute('bgcolor','white');
        table.setAttribute('border','10');
        makerow(table,i,0,state);
        makerow(table,i,1,state);
        makerow(table,i,2,state);
        makerow(table,i,3,state);
        makerow(table,i,4,state);
        makerow(table,i,5,state);
        console.log("got here");
       cell.appendChild(table); 
     }
   }
   console.log(bigtable);
   return bigtable;
  
}

function makerow (table,i,rownum,state) 
 {var row =table.insertRow(rownum);
  makecell(row,i,rownum,0,state);
  makecell(row,i,rownum,1,state);
  makecell(row,i,rownum,2,state);
  makecell(row,i,rownum,3,state);
  makecell(row,i,rownum,4,state);
  return row}

function makecell (row,i,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','20');
  cell.setAttribute('height','20');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  rownum = (rownum+1).toString();
  colnum = (colnum+1).toString();
  var mark = compfindx('Z',seq('cell',"" + i,rownum,colnum,'Z'),state,seq());
  if (mark=='knight') {cell.innerHTML = '<img src="http://games.ggp.org/base/resources/images/chess/White_Knight.png" width="15" height="15"/>'};
  if (mark=='pawn') {cell.setAttribute('bgcolor','#000000')};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
