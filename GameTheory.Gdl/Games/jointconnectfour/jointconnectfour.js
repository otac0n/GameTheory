//------------------------------------------------------------------------------
// jointconnectfour
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','0');
  table.setAttribute('cellpadding','0');
  table.setAttribute('border','10');
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  cell.appendChild(rendernormal(state));
  var row = table.insertRow(1);
  var cell = row.insertCell(0);
  cell.appendChild(rendersuicide(state));
  return table}

function rendernormal (state)
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
            var filler = compfindx('Z',seq('cell_normal',colnum,rownum,'Z'),state,seq());
            cell.height='40';
            cell.width='40';
            cell.align='center';
            cell.valign='center';
            if (filler=='red')
               {cell.innerHTML = '<img src="/ggp/games/jointconnectfour/red.png" height="30"/>'};
            if (filler=='black')
               {cell.innerHTML = '<img src="/ggp/games/jointconnectfour/blue.png" height="30"/>'};
            if (filler==false)
               {cell.innerHTML = '&nbsp;'}}};
  return table}

function rendersuicide (state)
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
            var filler = compfindx('Z',seq('cell_suicide',colnum,rownum,'Z'),state,seq());
            cell.height='40';
            cell.width='40';
            cell.align='center';
            cell.valign='center';
            if (filler=='red')
               {cell.innerHTML = '<img src="/ggp/games/jointconnectfour/red.png" height="30"/>'};
            if (filler=='black')
               {cell.innerHTML = '<img src="/ggp/games/jointconnectfour/blue.png" height="30"/>'};
            if (filler==false)
               {cell.innerHTML = '&nbsp;'}}};
  return table}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
