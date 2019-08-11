//------------------------------------------------------------------------------
// reversi
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('cellpadding','0');
  table.setAttribute('border','1');
  for (var i=0; i<8; i++)
      {var row = table.insertRow(i);
       var rownum = String(8-i);
       for (var j=0; j<8; j++)
           {var cell = row.insertCell(j);
            var colnum = String(j+1);
            var filler = compfindx('Z',seq('cell',colnum,rownum,'Z'),state,seq());
            cell.height='40';
            cell.width='40';
            cell.align='center';
            cell.valign='center';
            if (filler=='red')
               {cell.innerHTML = '<img src="/ggp/games/reversibig/red.png" height="30"/>'};
            if (filler=='blue')
               {cell.innerHTML = '<img src="/ggp/games/reversibig/blue.png" height="30"/>'};
            if (filler==false)
               {cell.innerHTML = '&nbsp;'}}};
  return table}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
