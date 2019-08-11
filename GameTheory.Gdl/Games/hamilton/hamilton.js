//------------------------------------------------------------------------------
// Hamilton
//------------------------------------------------------------------------------

function renderstate (state)
 {var canvas = document.createElement('canvas');
  canvas.setAttribute('width','390px');
  canvas.setAttribute('height','390px');
  drawline(195, 15,375,105,canvas);
  drawline(375,105,375,375,canvas);
  drawline(375,375, 15,375,canvas);
  drawline( 15,375, 15,105,canvas);
  drawline( 15,105,195, 15,canvas);

  drawline( 15,105, 75,135,canvas);
  drawline(375,105,315,135,canvas);
  drawline(195, 15,195, 75,canvas);
  drawline(375,375,315,315,canvas);
  drawline( 15,375, 75,315,canvas);

  drawline(195, 75,315,135,canvas);
  drawline(315,135,315,315,canvas);
  drawline(315,315, 75,315,canvas);
  drawline( 75,315, 75,135,canvas);
  drawline( 75,135,195, 75,canvas);

  drawline(195,270,195,315,canvas);
  drawline(135,105,135,165,canvas);
  drawline( 75,225,135,225,canvas);

  drawline(135,165,135,225,canvas);
  drawline(135,165,255,165,canvas);
  drawline(255,165,255,225,canvas);
  drawline(255,105,255,165,canvas);
  drawline(255,225,315,225,canvas);
  drawline(255,225,195,270,canvas);
  drawline(195,270,135,225,canvas);

  drawnode('a',180,  0,state,canvas);
  drawnode('b',360, 90,state,canvas);
  drawnode('c',360,360,state,canvas);
  drawnode('d',  0,360,state,canvas);
  drawnode('e',  0, 90,state,canvas);

  drawnode('f', 60,120,state,canvas);
  drawnode('g',120, 90,state,canvas);
  drawnode('h',180, 60,state,canvas);
  drawnode('i',240, 90,state,canvas);
  drawnode('j',300,120,state,canvas);
  drawnode('k',300,210,state,canvas);
  drawnode('l',300,300,state,canvas);
  drawnode('m',180,300,state,canvas);
  drawnode('n', 60,300,state,canvas);
  drawnode('o', 60,210,state,canvas);

  drawnode('p',120,210,state,canvas);
  drawnode('q',120,150,state,canvas);
  drawnode('r',240,150,state,canvas);
  drawnode('s',240,210,state,canvas);
  drawnode('t',180,255,state,canvas);

  drawscore(compfindx('X',seq('score','X'),state,library),170,195,canvas);

  return canvas}

//------------------------------------------------------------------------------
// Drawing subroutines
//------------------------------------------------------------------------------

function drawproposition (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.arc(x+15,y+15,12,0,2*Math.PI,false);
  ctx.stroke();
  ctx.fillStyle = "#dcccb3";
  ctx.fill();}

function drawvisited (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.arc(x+15,y+15,12,0,2*Math.PI,false);
  ctx.stroke();
  ctx.fillStyle = "#bb77b3";
  ctx.fill();}

function drawcurrent (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.arc(x+15,y+15,12,0,2*Math.PI,false);
  ctx.stroke();
  ctx.fillStyle = "#bb7700";
  ctx.fill();}

function drawtext (text,x,y,w)
 {var ctx = w.getContext('2d');
  ctx.fillStyle = "#000000";
  ctx.font="italic 14px Times"
  ctx.fillText(text,x+12,y+18);
  return true}

function drawline(u,v,x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.moveTo(u,v);
  ctx.lineTo(x,y);
  ctx.stroke()}

function drawnode (name,x,y,state,canvas)
 {var current = compfindp(seq('location',name),state,library);
  if (current) {drawcurrent(x,y,canvas)}
     else {var visited = compfindp(seq('visited',name),state,library);
           if (visited) {drawvisited(x,y,canvas)}
           else {drawproposition(x,y,canvas)}};
  drawtext(name,x,y,canvas)}

function drawscore (text,x,y,w)
 {var ctx = w.getContext('2d');
  ctx.fillStyle = "#000000";
  ctx.font="28px Times"
  ctx.fillText(text,x+12,y+18);
  return true}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
