//------------------------------------------------------------------------------
// connectfour
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('cellpadding','0');
  table.setAttribute('border','1');
  for (var i=0; i<6; i++)
      {var row = table.insertRow(i);
       var rownum = String(6-i);
       for (var j=0; j<8; j++)
           {var cell = row.insertCell(j);
            var colnum = String(j+1);
            var filler = compfindx('Z',seq('cell',colnum,rownum,'Z'),state,seq());
            cell.height='30';
            cell.width='30';
            cell.align='center';
            cell.valign='center';
            if (filler=='red')
               {cell.innerHTML = '<img src="/ggp/games/connectfour3p/red.jpg"/>'};
            if (filler=='blue')
               {cell.innerHTML = '<img src="/ggp/games/connectfour3p/blue.jpg"/>'};
            if (filler=='yellow')
               {cell.innerHTML = '<img src="/ggp/games/connectfour3p/yellow.png"/>'};
            if (filler==false)
               {cell.innerHTML = '&nbsp;'}}};
  return table}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
