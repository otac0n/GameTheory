//------------------------------------------------------------------------------
// Albuquerque
//------------------------------------------------------------------------------

function renderstate (state)
 {var canvas = document.createElement('canvas');
  canvas.setAttribute('width','150px');
  canvas.setAttribute('height','150px');

  drawline( 15,135,135,135,canvas);
  drawline( 15, 15, 15,135,canvas);
  drawline(135, 15,135,135,canvas);
  drawline( 15, 15,135,135,canvas);
  drawline( 15,135,135, 15,canvas);

  drawnode('a',  0,  0,canvas); drawtext('a',  0,  0,canvas);
  drawnode('b',  0,120,canvas); drawtext('b',  0,120,canvas);
  drawnode('c', 60, 60,canvas); drawtext('c', 60, 60,canvas);
  drawnode('d',120,  0,canvas); drawtext('d',120,  0,canvas);
  drawnode('e',120,120,canvas); drawtext('e',120,120,canvas);

  return canvas}

//------------------------------------------------------------------------------
// Drawing subroutines
//------------------------------------------------------------------------------

function drawred (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.arc(x+15,y+15,12,0,2*Math.PI,false);
  ctx.stroke();
  ctx.fillStyle = "#ff8888";
  ctx.fill();}

function drawblack (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.arc(x+15,y+15,12,0,2*Math.PI,false);
  ctx.stroke();
  ctx.fillStyle = "#888888";
  ctx.fill();}

function drawblank (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.arc(x+15,y+15,12,0,2*Math.PI,false);
  ctx.stroke();
  ctx.fillStyle = "#ffffff";
  ctx.fill();}

function drawline(u,v,x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.moveTo(u,v);
  ctx.lineTo(x,y);
  ctx.stroke()}

function drawnode (node,x,y,canvas)
 {var mark = compfindx('X',seq('cell',node,'X'),state,library);
  if (mark=='red') {drawred(x,y,canvas); return true};
  if (mark=='black') {drawblack(x,y,canvas); return true};
  drawblank(x,y,canvas);
  return true}

function drawtext (text,x,y,w)
 {var ctx = w.getContext('2d');
  ctx.fillStyle = "#000000";
  ctx.font="italic 14px Times"
  ctx.fillText(text,x+12,y+18);
  return true}

function drawscore (text,x,y,w)
 {var ctx = w.getContext('2d');
  ctx.fillStyle = "#000000";
  ctx.font="28px Times"
  ctx.fillText(text,x+12,y+18);
  return true}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
