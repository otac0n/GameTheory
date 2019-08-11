//------------------------------------------------------------------------------
// prisoner
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('bgcolor','white');
  table.setAttribute('border','10');
  var roundd = parseInt(compfindx('Z',seq('roundd','Z'),state,seq()));
  var header = table.insertRow(0);
  header.insertCell(0).innerHTML = 'Round #';
  header.insertCell(1).innerHTML = 'Player A eat';
  header.insertCell(2).innerHTML = 'Player A pass';
  header.insertCell(3).innerHTML = 'Player B eat';
  header.insertCell(4).innerHTML = 'Player B pass';
  header.insertCell(5).innerHTML = 'Reward (A,B)';

  for (var i = 0; i < roundd; i++) {
    makerow(table,i,state);
  }
  return table}

function makerow (table,rownum,state)
 {var row =table.insertRow(rownum+1);
  makecell(row,rownum,0,rownum.toString());
  var ateA = compfindp(seq('ate','a',rownum.toString()),state,seq()) == true ? 'X' : '&nbsp;';
  makecell(row,rownum,1,ateA);
  var passedA = compfindp(seq('passed','a',rownum.toString()),state,seq()) == true ? 'X' : '&nbsp;';
  makecell(row,rownum,2,passedA);
  var ateB = compfindp(seq('ate','b',rownum.toString()),state,seq()) == true ? 'X' : '&nbsp;';
  makecell(row,rownum,3,ateB);
  var passedB = compfindp(seq('passed','b',rownum.toString()),state,seq()) == true ? 'X' : '&nbsp;';
  makecell(row,rownum,4,passedB);
  var rewards = seq();
  compall('Z',seq('score','X',rownum.toString(),'Z'),seq(),seq(),nil,rewards,truify(state),library);
  makecell(row,rownum,5,rewards);

  return row}

function makecell (row,rownum,colnum,val)
 {var cell = row.insertCell(colnum);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  cell.setAttribute('style','font-family:helvetica;font-size:18pt');
  if (val) {cell.innerHTML = val} else {cell.innerHTML = '&nbsp;'}
  return cell}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
