//------------------------------------------------------------------------------
// nqueens
//------------------------------------------------------------------------------

function renderstate (state)
 {

var table = document.createElement('table');
  table.setAttribute('cellspacing','2');
  table.setAttribute('bgcolor','#cccccc');
  table.setAttribute('border','4');
  var N = 16;
  for (var i=N; i>0; i--)
      {var row = table.insertRow(N-i);
       var rank = stringize(i);
       makecell(row,'a',rank,state);
       makecell(row,'b',rank,state);
       makecell(row,'c',rank,state);
       makecell(row,'d',rank,state);
       makecell(row,'e',rank,state);
       makecell(row,'f',rank,state);
       makecell(row,'g',rank,state);
       makecell(row,'h',rank,state);
       makecell(row,'i',rank,state);
       makecell(row,'j',rank,state);
       makecell(row,'k',rank,state);
       makecell(row,'l',rank,state);
       makecell(row,'m',rank,state);
       makecell(row,'n',rank,state);
       makecell(row,'o',rank,state);
       makecell(row,'p',rank,state)};
var div = document.createElement('div');
div.style.fontSize = 24;
div.append(table);
div.append( compfindx('Z',seq('step','Z'),state,seq()) );

  return div}

function makecell (row,file,rank,state)
 {var cell = row.insertCell(row.cells.length);
  cell.setAttribute('width','40');
  cell.setAttribute('height','40');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  var image = getimage(file,rank,state);
  if (image)
     {var widget = document.createElement('img');
      widget.setAttribute('width','30');
      widget.setAttribute('height','30');
      widget.setAttribute('src',image);
      cell.appendChild(widget)}
     else {cell.innerHTML = '&nbsp;'};
  return cell}

function getimage (file,rank,state)
 {var image = compfindx('Piece',seq('location',file,rank,'Piece'),state,library);
  if (image=='whitepawn')
     {return '/ggp/games/nqueens/White_Pawn.png'};
  if (image=='whiteking')
     {return '/ggp/games/nqueens/White_King.png'};
  if (image=='whitequeen')
     {return '/ggp/games/nqueens/White_Queen.png'};
  if (image=='whitebishop')
     {return '/ggp/games/nqueens/White_Bishop.png'};
  if (image=='whiteknight')
     {return '/ggp/games/nqueens/White_Knight.png'};
  if (image=='whiterook')
     {return '/ggp/games/nqueens/White_Rook.png'};
  if (image=='blackpawn')
     {return '/ggp/games/nqueens/Black_Pawn.png'};
  if (image=='blackking')
     {return '/ggp/games/nqueens/Black_King.png'};
  if (image=='blackqueen')
     {return '/ggp/games/nqueens/Black_Queen.png'};
  if (image=='blackbishop')
     {return '/ggp/games/nqueens/Black_Bishop.png'};
  if (image=='blackknight')
     {return '/ggp/games/nqueens/Black_Knight.png'};
  if (image=='blackrook')
     {return '/ggp/games/nqueens/Black_Rook.png'};
  return false}
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
