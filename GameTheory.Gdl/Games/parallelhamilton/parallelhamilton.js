//------------------------------------------------------------------------------
// Dual Hamilton
//------------------------------------------------------------------------------

function renderstate (state)
 {var table = document.createElement('table');
  table.setAttribute('cellspacing','10');
  table.setAttribute('cellpadding','0');
  table.setAttribute('border','0');
  var row = table.insertRow(0);
  var cell = row.insertCell(0);
  cell.appendChild(makegraph('left',state));
  cell = row.insertCell(1);
  cell.appendChild(makegraph('right',state));
  return table}

function makegraph (component,state)
 {var score = compfindx('X',seq('score',component,'X'),state,library);
  var a = getstatus('a',component,state);
  var b = getstatus('b',component,state);
  var c = getstatus('c',component,state);
  var d = getstatus('d',component,state);
  var e = getstatus('e',component,state);
  var f = getstatus('f',component,state);
  var g = getstatus('g',component,state);
  var h = getstatus('h',component,state);
  var i = getstatus('i',component,state);
  var j = getstatus('j',component,state);
  var k = getstatus('k',component,state);
  var l = getstatus('l',component,state);
  var m = getstatus('m',component,state);
  var n = getstatus('n',component,state);
  var o = getstatus('o',component,state);
  var p = getstatus('p',component,state);
  var q = getstatus('q',component,state);
  var r = getstatus('r',component,state);
  var s = getstatus('s',component,state);
  var t = getstatus('t',component,state);

  var canvas = document.createElement('canvas');
  canvas.setAttribute('width','312px');
  canvas.setAttribute('height','312px');
  var ctx = canvas.getContext('2d');
  ctx.scale(0.8,0.8);
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

  drawnode('a',a,180,  0,canvas);
  drawnode('b',b,360, 90,canvas);
  drawnode('c',c,360,360,canvas);
  drawnode('d',d,  0,360,canvas);
  drawnode('e',e,  0, 90,canvas);

  drawnode('f',f, 60,120,canvas);
  drawnode('g',g,120, 90,canvas);
  drawnode('h',h,180, 60,canvas);
  drawnode('i',i,240, 90,canvas);
  drawnode('j',j,300,120,canvas);
  drawnode('k',k,300,210,canvas);
  drawnode('l',l,300,300,canvas);
  drawnode('m',m,180,300,canvas);
  drawnode('n',n, 60,300,canvas);
  drawnode('o',o, 60,210,canvas);

  drawnode('p',p,120,210,canvas);
  drawnode('q',q,120,150,canvas);
  drawnode('r',r,240,150,canvas);
  drawnode('s',s,240,210,canvas);
  drawnode('t',t,180,255,canvas);

  drawscore(score,170,195,canvas);

  return canvas}

function getstatus (name,component,state)
 {var current = compfindp(seq('location',component,name),state,library);
  if (current) {return 'current'};
  var visited = compfindp(seq('visited',component,name),state,library);
  if (visited) {return 'visited'};
  return 'fresh'}

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

function drawnode (name,status,x,y,canvas)
 {if (status=='current') {drawcurrent(x,y,canvas)}
     else {if (status=='visited') {drawvisited(x,y,canvas)}
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
