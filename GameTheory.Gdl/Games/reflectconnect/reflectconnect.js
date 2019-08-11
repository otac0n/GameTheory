//------------------------------------------------------------------------------
// reflectconnect
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('cellpadding','0');
  table.setAttribute('border','1');
  for (var i=0; i<11; i++)
      {var row = table.insertRow(i);
       var rownum = String(11-i);
       for (var j=0; j<11; j++)
           {var cell = row.insertCell(j);
               cell.height=40;
               cell.width=40;
               cell.align='center';
               cell.valign='center';
               if ((i<5 && j<5) || (i>5 && j>5))
                  {cell.style.backgroundColor="black"; continue}
                  else if (i==5 && j==5)
                          {cell.style.backgroundColor="green"; continue}
            var colnum = String(j+1);
            var filler = compfindx('Z',seq('cell',colnum,rownum,'Z'),state,seq());
            if (filler=='red')
               {cell.innerHTML = '<img src="/ggp/games/reflectconnect6/red.png" height="30"/>';};
            if (filler=='blue')
               {cell.innerHTML = '<img src="/ggp/games/reflectconnect6/blue.png" height="30"/>'};
            if (filler=='blank')
               {cell.innerHTML = '&nbsp;'}}};
			
  return table}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
