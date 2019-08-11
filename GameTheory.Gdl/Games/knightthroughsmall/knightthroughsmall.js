//------------------------------------------------------------------------------
// Knightthrough
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','2');
  table.setAttribute('bgcolor','#cccccc');
  table.setAttribute('border','4');
  for (var i=6; i>0; i--)
      {var row = table.insertRow(6-i);
       var rank = stringize(i);
       makecell(row,'1',rank,state);
       makecell(row,'2',rank,state);
       makecell(row,'3',rank,state);
       makecell(row,'4',rank,state);
       makecell(row,'5',rank,state);
       makecell(row,'6',rank,state);
      }
  return table}

function makecell (row,file,rank,state)
 {var cell = row.insertCell(row.cells.length);
  cell.setAttribute('width','50');
  cell.setAttribute('height','50');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  var image = getimage(file,rank,state);
  if (image)
     {var widget = document.createElement('img');
      widget.setAttribute('width','40');
      widget.setAttribute('height','40');
      widget.setAttribute('src',image);
      cell.appendChild(widget)}
     else {cell.innerHTML = '&nbsp;'};
  return cell}

function getimage (file,rank,state)
 {var image = compfindx('Piece',seq('cell',file,rank,'Piece'),state,library);
  if (image=='white')
     {return '/ggp/games/knightthrough/white.png'};
  if (image=='black')
     {return '/ggp/games/knightthrough/black.png'};
  return ''}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
