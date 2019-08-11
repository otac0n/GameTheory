//------------------------------------------------------------------------------
// connectfour
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('cellpadding','0');
  table.setAttribute('border','10');
  for (var i=0; i<6; i++)
      {var row = table.insertRow(i);
       var rownum = String(6-i);
       for (var j=0; j<7; j++)
           {var cell = row.insertCell(j);
            var colnum = String(j+1);
            var filler = compfindx('Z',seq('cell',colnum,rownum,'Z'),state,seq());
            cell.height='60';
            cell.width='60';
            cell.align='center';
            cell.valign='center';
            if (filler=='red')
               {cell.innerHTML = '<img src="/ggp/games/connectfoursuicide/red.png" height="50"/>'};
            if (filler=='blue')
               {cell.innerHTML = '<img src="/ggp/games/connectfoursuicide/blue.png" height="50"/>'};
            if (filler==false)
               {cell.innerHTML = '&nbsp;'}}};
  return table}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
