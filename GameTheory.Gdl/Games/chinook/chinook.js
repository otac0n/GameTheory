//------------------------------------------------------------------------------
// Chinook
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','2');
  table.setAttribute('bgcolor','#cccccc');
  table.setAttribute('border','4');
  for (var i=8; i>0; i=i-2)
      {var row = table.insertRow(8-i);
       var rank = stringize(i);
       addevencell(row,'a',rank,state);
       addoddcell(row,'b',rank,state);
       addevencell(row,'c',rank,state);
       addoddcell(row,'d',rank,state);
       addevencell(row,'e',rank,state);
       addoddcell(row,'f',rank,state);
       addevencell(row,'g',rank,state);
       addoddcell(row,'h',rank,state);
       row = table.insertRow(8-i+1);
       var rank = stringize(i-1);
       addoddcell(row,'a',rank,state);
       addevencell(row,'b',rank,state);
       addoddcell(row,'c',rank,state);
       addevencell(row,'d',rank,state);
       addoddcell(row,'e',rank,state);
       addevencell(row,'f',rank,state);
       addoddcell(row,'g',rank,state);
       addevencell(row,'h',rank,state)};
  return table}

function addoddcell (row,file,rank,state)
 {var cell = row.insertCell(row.cells.length);
  cell.setAttribute('bgcolor','#cccccc');
  cell.setAttribute('width','50');
  cell.setAttribute('height','50');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  var image = getoddimage(file,rank,state);
  if (image)
     {var widget = document.createElement('img');
      widget.setAttribute('width','40');
      widget.setAttribute('height','40');
      widget.setAttribute('src',image);
      cell.appendChild(widget)}
     else {cell.innerHTML = '&nbsp;'};
  return true}

function addevencell (row,file,rank,state)
 {var cell = row.insertCell(row.cells.length);
  cell.setAttribute('bgcolor','#aaaaaa');
  cell.setAttribute('width','50');
  cell.setAttribute('height','50');
  cell.setAttribute('align','center');
  cell.setAttribute('valign','center');
  var image = getevenimage(file,rank,state);
  if (image)
     {var widget = document.createElement('img');
      widget.setAttribute('width','40');
      widget.setAttribute('height','40');
      widget.setAttribute('src',image);
      cell.appendChild(widget)}
     else {cell.innerHTML = '&nbsp;'};
  return true}

function getoddimage (file,rank,state)
 {var image = compfindx('Piece',seq('oddcell',file,rank,'Piece'),state,library);
  if (image=='red') {return '/ggp/games/chinook/red.png'};
  if (image=='blue') {return '/ggp/games/chinook/blue.png'};
  return false}

function getevenimage (file,rank,state)
 {var image = compfindx('Piece',seq('evencell',file,rank,'Piece'),state,library);
  if (image=='red') {return '/ggp/games/chinook/red.png'};
  if (image=='blue') {return '/ggp/games/chinook/blue.png'};
  return false}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
