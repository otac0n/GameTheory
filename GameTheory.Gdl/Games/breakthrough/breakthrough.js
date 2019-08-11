//------------------------------------------------------------------------------
// Breakthrough
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','2');
  table.setAttribute('bgcolor','#cccccc');
  table.setAttribute('border','4');
  for (var i=8; i>0; i--)
      {var row = table.insertRow(8-i);
       var rank = stringize(i);
       makecell(row,'1',rank,state);
       makecell(row,'2',rank,state);
       makecell(row,'3',rank,state);
       makecell(row,'4',rank,state);
       makecell(row,'5',rank,state);
       makecell(row,'6',rank,state);
       makecell(row,'7',rank,state);
       makecell(row,'8',rank,state)};
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
     {return 'http://games.ggp.org/base/resources/images/chess/White_Pawn.png'};
  if (image=='whiteking')
     {return 'http://games.ggp.org/base/resources/images/chess/White_King.png'};
  if (image=='whitequeen')
     {return 'http://games.ggp.org/base/resources/images/chess/White_Queen.png'};
  if (image=='whitebishop')
     {return 'http://games.ggp.org/base/resources/images/chess/White_Bishop.png'};
  if (image=='whiteknight')
     {return 'http://games.ggp.org/base/resources/images/chess/White_Knight.png'};
  if (image=='whiterook')
     {return 'http://games.ggp.org/base/resources/images/chess/White_Rook.png'};
  if (image=='black')
     {return 'http://games.ggp.org/base/resources/images/chess/Black_Pawn.png'};
  if (image=='blackking')
     {return 'http://games.ggp.org/base/resources/images/chess/Black_King.png'};
  if (image=='blackqueen')
     {return 'http://games.ggp.org/base/resources/images/chess/Black_Queen.png'};
  if (image=='blackbishop')
     {return 'http://games.ggp.org/base/resources/images/chess/Black_Bishop.png'};
  if (image=='blackknight')
     {return 'http://games.ggp.org/base/resources/images/chess/Black_Knight.png'};
  if (image=='blackrook')
     {return 'http://games.ggp.org/base/resources/images/chess/Black_Rook.png'};
  return false}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
