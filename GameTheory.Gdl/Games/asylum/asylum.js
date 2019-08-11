//------------------------------------------------------------------------------
// Asylum
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  cell.height = '50';
  cell.align = 'center';
  cell.appendChild(renderreserve('black',state));
  row = table.insertRow(1);
  cell = row.insertCell(0);
  cell.align = 'center';
  cell.appendChild(renderboard(state));
  row = table.insertRow(2);
  cell = row.insertCell(0);
  cell.height = '50';
  cell.align = 'center';
  cell.appendChild(renderreserve('white',state));
  return table}

function renderreserve (role,state)
 {var reserve = getreserve(role,state);
  var table = document.createElement('table');
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  for (var i=0; i<reserve.length; i++)
      {var image = getimage(reserve[i]);
       var widget = document.createElement('img');
       widget.setAttribute('width','40');
       widget.setAttribute('height','40');
       if (image) {widget.setAttribute('src',image)};
       cell.appendChild(widget)};
  return table}
      
function renderboard (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','2');
  table.setAttribute('bgcolor','#cccccc');
  table.setAttribute('border','4');
  for (var i=8; i>0; i--)
      {var row = table.insertRow(8-i);
       var rank = stringize(i);
       makecell(row,'a',rank,state);
       makecell(row,'b',rank,state);
       makecell(row,'c',rank,state);
       makecell(row,'d',rank,state);
       makecell(row,'e',rank,state);
       makecell(row,'f',rank,state);
       makecell(row,'g',rank,state);
       makecell(row,'h',rank,state)};
  return table}

function makecell (row,file,rank,state)
 {var cell = row.insertCell(row.cells.length);
  cell.setAttribute('width','50');
  cell.setAttribute('height','50');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  var image = getimage(getpiece(file,rank,state));
  if (image)
     {var widget = document.createElement('img');
      widget.setAttribute('width','40');
      widget.setAttribute('height','40');
      widget.setAttribute('src',image);
      cell.appendChild(widget)}
     else {cell.innerHTML = '&nbsp;'};
  return cell}

function getimage (piece)
 {if (!piece) {return false};
  var role = getcolor(piece,state);
  if (piece[0]==='k')
     {if (role==='white')
         {return '/ggp/games/asylum/White_King.png'}
      return '/ggp/games/asylum/Black_King.png'};
  if (piece[0]==='q')
     {if (role==='white')
         {return '/ggp/games/asylum/White_Queen.png'}
      return '/ggp/games/asylum/Black_Queen.png'};
  if (piece[0]==='b')
     {if (role==='white')
         {return '/ggp/games/asylum/White_Bishop.png'}
      return '/ggp/games/asylum/Black_Bishop.png'};
  if (piece[0]==='n')
     {if (role==='white')
         {return '/ggp/games/asylum/White_Knight.png'}
      return '/ggp/games/asylum/Black_Knight.png'};
  if (piece[0]==='r')
     {if (role==='white')
         {return '/ggp/games/asylum/White_Rook.png'}
      return '/ggp/games/asylum/Black_Rook.png'};
  if (piece[0]==='p')
     {if (role==='white')
         {return '/ggp/games/asylum/White_Pawn.png'}
      return '/ggp/games/asylum/Black_Pawn.png'};
  return false}

function getreserve (role,state)
 {var reserve = seq();
  for (var i=0; i<state.length; i++)
      {var fact = state[i];
       if (fact[0]==='color' && fact[2]===role && !getboardp(fact[1],state))
          {reserve[reserve.length] = fact[1]}};
  return reserve}

function getboardp (piece,state)
 {for (var i=0; i<state.length; i++)
      {var fact = state[i];
       if (fact[0]==='location' && fact[3]===piece) {return true}};
  return false}

function getpiece (file,rank,state)
 {for (var i=0; i<state.length; i++)
      {var fact = state[i];
       if (fact[0]==='location' && fact[1]===file && fact[2]===rank)
          {return fact[3]}};
  return false}

function getcolor (piece,state)
 {for (var i=0; i<state.length; i++)
      {var fact = state[i];
       if (fact[0]==='color' && fact[1]===piece) {return fact[2]}};
  return false}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
