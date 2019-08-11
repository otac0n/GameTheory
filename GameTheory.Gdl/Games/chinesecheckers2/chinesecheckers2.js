
function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('bgcolor','#dddddd');
  table.setAttribute('cellpadding','10');
  table.setAttribute('border','1');
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  cell.appendChild(renderboard(state));
  return table}

function renderboard (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('cellpadding','0');
  table.setAttribute('border','0');
  for (var i=0; i<9; i++)
      {var row = table.insertRow(i);
       for (var j=0; j<13; j++)
           {var cell = row.insertCell(j);
            cell.width='40';
            cell.height='60';
            cell.align='center';
            cell.valign='center';
            cell.innerHTML='&nbsp;'}};
  table.rows[0].cells[6].innerHTML = color('a1',state);
  table.rows[1].cells[5].innerHTML = color('b1',state);
  table.rows[1].cells[7].innerHTML = color('b2',state);
  table.rows[2].cells[0].innerHTML = color('c1',state);
  table.rows[2].cells[2].innerHTML = color('c2',state);
  table.rows[2].cells[4].innerHTML = color('c3',state);
  table.rows[2].cells[6].innerHTML = color('c4',state);
  table.rows[2].cells[8].innerHTML = color('c5',state);
  table.rows[2].cells[10].innerHTML = color('c6',state);
  table.rows[2].cells[12].innerHTML = color('c7',state);
  table.rows[3].cells[1].innerHTML = color('d1',state);
  table.rows[3].cells[3].innerHTML = color('d2',state);
  table.rows[3].cells[5].innerHTML = color('d3',state);
  table.rows[3].cells[7].innerHTML = color('d4',state);
  table.rows[3].cells[9].innerHTML = color('d5',state);
  table.rows[3].cells[11].innerHTML = color('d6',state);
  table.rows[4].cells[2].innerHTML = color('e1',state);
  table.rows[4].cells[4].innerHTML = color('e2',state);
  table.rows[4].cells[6].innerHTML = color('e3',state);
  table.rows[4].cells[8].innerHTML = color('e4',state);
  table.rows[4].cells[10].innerHTML = color('e5',state);
  table.rows[5].cells[1].innerHTML = color('f1',state);
  table.rows[5].cells[3].innerHTML = color('f2',state);
  table.rows[5].cells[5].innerHTML = color('f3',state);
  table.rows[5].cells[7].innerHTML = color('f4',state);
  table.rows[5].cells[9].innerHTML = color('f5',state);
  table.rows[5].cells[11].innerHTML = color('f6',state);
  table.rows[6].cells[0].innerHTML = color('g1',state);
  table.rows[6].cells[2].innerHTML = color('g2',state);
  table.rows[6].cells[4].innerHTML = color('g3',state);
  table.rows[6].cells[6].innerHTML = color('g4',state);
  table.rows[6].cells[8].innerHTML = color('g5',state);
  table.rows[6].cells[10].innerHTML = color('g6',state);
  table.rows[6].cells[12].innerHTML = color('g7',state);
  table.rows[7].cells[5].innerHTML = color('h1',state);
  table.rows[7].cells[7].innerHTML = color('h2',state);
  table.rows[8].cells[6].innerHTML = color('i1',state);
  return table}

function color (cell,state)
 {var color = compfindx('Z',seq('cell',cell,'Z'),state,seq());
  if (color=='yellow') {return '<img src="/ggp/games/chinesecheckers4/yellow.png" width="40" height="40"/>'};
  if (color=='magenta') {return '<img src="/ggp/games/chinesecheckers4/magenta.png" width="40" height="40"/>'};
  return '<img src="/ggp/games/chinesecheckers4/black.png" width="40" height="40"/>'}

