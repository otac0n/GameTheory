//------------------------------------------------------------------------------
// Albuquerque
//------------------------------------------------------------------------------

function renderstate (state)
 {var canvas = document.createElement('canvas');
  canvas.setAttribute('width','270px');
  canvas.setAttribute('height','270px');

  drawline( 15, 15,255, 15,canvas);
  drawline( 15, 75,255, 75,canvas);
  drawline( 15,135,255,135,canvas);
  drawline( 15,195,255,195,canvas);
  drawline( 15,255,255,255,canvas);

  drawline( 15, 15, 15,255,canvas);
  drawline( 75, 15, 75,255,canvas);
  drawline(135, 15,135,255,canvas);
  drawline(195, 15,195,255,canvas);
  drawline(255, 15,255,255,canvas);

  drawline( 15, 15,255,255,canvas);
  drawline( 15,255,255, 15,canvas);
  drawline(135, 15, 15,135,canvas);
  drawline( 15,135,135,255,canvas);
  drawline(135,255,255,135,canvas);
  drawline(255,135,135, 15,canvas);

  drawnode('1','1',  0,  0,state,canvas);
  drawnode('1','2', 60,  0,state,canvas);
  drawnode('1','3',120,  0,state,canvas);
  drawnode('1','4',180,  0,state,canvas);
  drawnode('1','5',240,  0,state,canvas);

  drawnode('2','1',  0, 60,state,canvas);
  drawnode('2','2', 60, 60,state,canvas);
  drawnode('2','3',120, 60,state,canvas);
  drawnode('2','4',180, 60,state,canvas);
  drawnode('2','5',240, 60,state,canvas);

  drawnode('3','1',  0,120,state,canvas);
  drawnode('3','2', 60,120,state,canvas);
  drawnode('3','3',120,120,state,canvas);
  drawnode('3','4',180,120,state,canvas);
  drawnode('3','5',240,120,state,canvas);

  drawnode('4','1',  0,180,state,canvas);
  drawnode('4','2', 60,180,state,canvas);
  drawnode('4','3',120,180,state,canvas);
  drawnode('4','4',180,180,state,canvas);
  drawnode('4','5',240,180,state,canvas);

  drawnode('5','1',  0,240,state,canvas);
  drawnode('5','2', 60,240,state,canvas);
  drawnode('5','3',120,240,state,canvas);
  drawnode('5','4',180,240,state,canvas);
  drawnode('5','5',240,240,state,canvas);

  drawscore("bla", 100, 300, canvas);

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

function drawnode (m,n,x,y,state,canvas)
 {var mark = compfindx('X',seq('cell',m,n,'X'),state,library);
  if (mark=='red') {drawred(x,y,canvas); return true};
  if (mark=='black') {drawblack(x,y,canvas); return true};
  drawblank(x,y,canvas);
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
