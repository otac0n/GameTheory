//------------------------------------------------------------------------------
// ttcc4
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('cellpadding','0');
  table.setAttribute('border','0');
  var row = table.insertRow(0);
  makenone(row,0,0,state);
  makenone(row,0,1,state);
  makecell(row,0,2,state);
  makecell(row,0,3,state);
  makecell(row,0,4,state);
  makenone(row,0,5,state);
  makenone(row,0,6,state);
  row = table.insertRow(1);
  makenone(row,1,0,state);
  makecell(row,1,1,state);
  makecell(row,1,2,state);
  makecell(row,1,3,state);
  makecell(row,1,4,state);
  makecell(row,1,5,state);
  makenone(row,1,6,state);
  row = table.insertRow(2);
  makecell(row,2,0,state);
  makecell(row,2,1,state);
  makegrey(row,2,2,state);
  makegrey(row,2,3,state);
  makegrey(row,2,4,state);
  makecell(row,2,5,state);
  makecell(row,2,6,state);
  row = table.insertRow(3);
  makecell(row,3,0,state);
  makecell(row,3,1,state);
  makegrey(row,3,2,state);
  makegrey(row,3,3,state);
  makegrey(row,3,4,state);
  makecell(row,3,5,state);
  makecell(row,3,6,state);
  row = table.insertRow(4);
  makecell(row,4,0,state);
  makecell(row,4,1,state);
  makegrey(row,4,2,state);
  makegrey(row,4,3,state);
  makegrey(row,4,4,state);
  makecell(row,4,5,state);
  makecell(row,4,6,state);
  row = table.insertRow(5);
  makenone(row,5,0,state);
  makecell(row,5,1,state);
  makecell(row,5,2,state);
  makecell(row,5,3,state);
  makecell(row,5,4,state);
  makecell(row,5,5,state);
  makenone(row,5,6,state);
  row = table.insertRow(6);
  makenone(row,6,0,state);
  makenone(row,6,1,state);
  makecell(row,6,2,state);
  makecell(row,6,3,state);
  makecell(row,6,4,state);
  makenone(row,6,5,state);
  makenone(row,6,6,state);
  return table}

function makenone (row,rownum,colnum)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.innerHTML = '&nbsp;';
  return cell}

function makegrey (row,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  cell.setAttribute('style','border: 2px solid #000;background-color:#dddddd');
  rownum = (rownum+1).toString();
  colnum = (colnum+1).toString();
  var mark = compfindx('Z',seq('cell',rownum,colnum,'Z'),state,seq());
  if (mark=='whitepawn') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/whitepawn.png" height="30" width="30"/>'};
  if (mark=='whitechecker') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/whitechecker.png" height="30" width="30"/>'};
  if (mark=='whiteknight') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/whiteknight" height="30" width="30"/>'};
  if (mark=='whitedisk') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/whitedisk" height="30" width="30"/>'};
  if (mark=='blackpawn') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/blackpawn.png" height="30" width="30"/>'};
  if (mark=='blackchecker') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/blackchecker.png" height="30" width="30"/>'};
  if (mark=='blackknight') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/blackknight.png" height="30" width="30"/>'};
  if (mark=='blackdisk') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/blackdisk.png" height="30" width="30"/>'};
  if (mark==false) {cell.innerHTML = '&nbsp;'};
  return cell}

function makecell (row,rownum,colnum,state)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  cell.setAttribute('style','border: 2px solid #000');
  rownum = (rownum+1).toString();
  colnum = (colnum+1).toString();
  var mark = compfindx('Z',seq('cell',rownum,colnum,'Z'),state,seq());
  if (mark=='whitepawn') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/whitepawn.png" height="30" width="30"/>'};
  if (mark=='whitechecker') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/whitechecker.png" height="30" width="30"/>'};
  if (mark=='whiteknight') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/whiteknight" height="30" width="30"/>'};
  if (mark=='whitedisk') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/whitedisk" height="30" width="30"/>'};
  if (mark=='blackpawn') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/blackpawn.png" height="30" width="30"/>'};
  if (mark=='blackchecker') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/blackchecker.png" height="30" width="30"/>'};
  if (mark=='blackknight') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/blackknight.png" height="30" width="30"/>'};
  if (mark=='blackdisk') {cell.innerHTML = '<img src="http://arrogant.stanford.edu/ggp/games/ttcc4/blackdisk.png" height="30" width="30"/>'};
  if (mark==false) {cell.innerHTML = '&nbsp;'};
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
