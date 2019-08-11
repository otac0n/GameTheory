//------------------------------------------------------------------------------
// Hex
//------------------------------------------------------------------------------

function renderstate (state)
 {var canvas = document.createElement('canvas');
  canvas.setAttribute('width','540px');
  canvas.setAttribute('height','441px');

  //drawhex(300,  0,canvas);

  //drawreddish(270, 20,canvas);
  //drawgrey(330, 20,canvas);

  drawhex(240, 40,canvas);
  //drawhex(300, 40,canvas);
  //drawgrey(360, 40,canvas);

  drawreddish(210, 60,canvas);
  drawgrey(270, 60,canvas);
  //drawhex(330, 60,canvas);
  //drawgrey(390, 60,canvas);

  drawreddish(180, 80,canvas);
  drawhex(240, 80,canvas);
  drawgrey(300, 80,canvas);
  //drawhex(360, 80,canvas);
  //drawgrey(420, 80,canvas);

  drawreddish(150,100,canvas);
  drawhex(210,100,canvas);
  drawhex(270,100,canvas);
  drawgrey(330,100,canvas);
  //drawhex(390,100,canvas);
  //drawgrey(450,100,canvas);

  drawreddish(120,120,canvas);
  drawhex(180,120,canvas);
  drawhex(240,120,canvas);
  drawhex(300,120,canvas);
  drawgrey(360,120,canvas);
  //drawhex(420,120,canvas);
  //drawgrey(480,120,canvas);

  drawreddish( 90,140,canvas);
  drawhex(150,140,canvas);
  drawhex(210,140,canvas);
  drawhex(270,140,canvas);
  drawhex(330,140,canvas);
  drawgrey(390,140,canvas);
  //drawhex(450,140,canvas);
  //drawgrey(510,140,canvas);

  drawreddish( 60,160,canvas);
  drawhex(120,160,canvas);
  drawhex(180,160,canvas);
  drawhex(240,160,canvas);
  drawhex(300,160,canvas);
  drawhex(360,160,canvas);
  drawgrey(420,160,canvas);
  //drawhex(480,160,canvas);
  //drawgrey(540,160,canvas);

  drawreddish( 30,180,canvas);
  drawhex( 90,180,canvas);
  drawhex(150,180,canvas);
  drawhex(210,180,canvas);
  drawhex(270,180,canvas);
  drawhex(330,180,canvas);
  drawhex(390,180,canvas);
  drawgrey(450,180,canvas);
  //drawhex(510,180,canvas);
  //drawgrey(570,180,canvas);

  drawhex(  0,200,canvas);
  drawhex( 60,200,canvas);
  drawhex(120,200,canvas);
  drawhex(180,200,canvas);
  drawhex(240,200,canvas);
  drawhex(300,200,canvas);
  drawhex(360,200,canvas);
  drawhex(420,200,canvas);
  drawhex(480,200,canvas);
  //drawhex(540,200,canvas);
  //drawhex(600,200,canvas);

  drawgrey( 30,220,canvas);
  drawhex( 90,220,canvas);
  drawhex(150,220,canvas);
  drawhex(210,220,canvas);
  drawhex(270,220,canvas);
  drawhex(330,220,canvas);
  drawhex(390,220,canvas);
  drawreddish(450,220,canvas);
  //drawreddish(510,220,canvas);
  //drawreddish(570,220,canvas);

  drawgrey( 60,240,canvas);
  drawhex(120,240,canvas);
  drawhex(180,240,canvas);
  drawhex(240,240,canvas);
  drawhex(300,240,canvas);
  drawhex(360,240,canvas);
  drawreddish(420,240,canvas);
  //drawreddish(480,240,canvas);
  //drawreddish(540,240,canvas);

  drawgrey( 90,260,canvas);
  drawhex(150,260,canvas);
  drawhex(210,260,canvas);
  drawhex(270,260,canvas);
  drawhex(330,260,canvas);
  drawreddish(390,260,canvas);
  //drawhex(450,260,canvas);
  //drawreddish(510,260,canvas);

  drawgrey(120,280,canvas);
  drawhex(180,280,canvas);
  drawhex(240,280,canvas);
  drawhex(300,280,canvas);
  drawreddish(360,280,canvas);
  //drawhex(420,280,canvas);
  //drawreddish(480,280,canvas);

  drawgrey(150,300,canvas);
  drawhex(210,300,canvas);
  drawhex(270,300,canvas);
  drawreddish(330,300,canvas);
  //drawhex(390,300,canvas);
  //drawreddish(450,300,canvas);

  drawgrey(180,320,canvas);
  drawhex(240,320,canvas);
  drawreddish(300,320,canvas);
  //drawhex(360,320,canvas);
  //drawreddish(420,320,canvas);

  drawgrey(210,340,canvas);
  drawreddish(270,340,canvas);
  //drawhex(330,340,canvas);
  //drawreddish(390,340,canvas);

  drawhex(240,360,canvas);
  //drawhex(300,360,canvas);
  //drawreddish(360,360,canvas);

  //drawgrey(270,380,canvas);
  //drawreddish(330,380,canvas);

  //drawhex(300,400,canvas);

  for (var i=0; i<state.length; i++)
      {if (state[i][0]=='cell')
          {var x = xpos(state[i][1],state[i][2]);
           var y = ypos(state[i][1],state[i][2]);
           if (state[i][3]=='red') {drawred(x,y,canvas)};
           if (state[i][3]=='black') {drawblack(x,y,canvas)}}};

  return canvas}

function xpos (m,n)
 {return xm(m)+xn(n)}

function ypos (m,n)
 {return 200+ym(m)-yn(n)}

function xm (m)
 {if (m=='a') {return 0};
  if (m=='b') {return 30};
  if (m=='c') {return 60};
  if (m=='d') {return 90};
  if (m=='e') {return 120};
  if (m=='f') {return 150};
  if (m=='g') {return 180};
  if (m=='h') {return 210};
  if (m=='i') {return 240};
  if (m=='j') {return 270};
  if (m=='k') {return 300};
  return 330}

function ym (m)
 {if (m=='a') {return 0};
  if (m=='b') {return 20};
  if (m=='c') {return 40};
  if (m=='d') {return 60};
  if (m=='e') {return 80};
  if (m=='f') {return 100};
  if (m=='g') {return 120};
  if (m=='h') {return 140};
  if (m=='i') {return 160};
  if (m=='j') {return 180};
  if (m=='k') {return 200};
  return 330}

function yn (n)
 {if (n=='1') {return 0};
  if (n=='2') {return 20};
  if (n=='3') {return 40};
  if (n=='4') {return 60};
  if (n=='5') {return 80};
  if (n=='6') {return 100};
  if (n=='7') {return 120};
  if (n=='8') {return 140};
  if (n=='9') {return 160};
  if (n=='10') {return 180};
  if (n=='11') {return 200};
  return 330}

function xn (n)
 {if (n=='1') {return 0};
  if (n=='2') {return 30};
  if (n=='3') {return 60};
  if (n=='4') {return 90};
  if (n=='5') {return 120};
  if (n=='6') {return 150};
  if (n=='7') {return 180};
  if (n=='8') {return 210};
  if (n=='9') {return 240};
  if (n=='10') {return 270};
  if (n=='11') {return 300};
  return 330}

//------------------------------------------------------------------------------
// Drawing subroutines
//------------------------------------------------------------------------------

function drawreddish (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=1;
  ctx.moveTo(x+10,y+ 0);
  ctx.lineTo(x+30,y+ 0);
  ctx.lineTo(x+40,y+20);
  ctx.lineTo(x+30,y+40);
  ctx.lineTo(x+10,y+40);
  ctx.lineTo( x+0,y+20);
  ctx.closePath();
  ctx.fillStyle = "#fff0f0";
  ctx.fill();
  ctx.stroke()}

function drawgrey (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=1;
  ctx.moveTo(x+10,y+ 0);
  ctx.lineTo(x+30,y+ 0);
  ctx.lineTo(x+40,y+20);
  ctx.lineTo(x+30,y+40);
  ctx.lineTo(x+10,y+40);
  ctx.lineTo( x+0,y+20);
  ctx.closePath();
  ctx.fillStyle = "#dddddd";
  ctx.fill();
  ctx.stroke()}

function drawhex (x,y,canvas)
 {drawline(x+ 0,y+20,x+10,y+ 0,canvas);
  drawline(x+10,y+ 0,x+30,y+ 0,canvas);
  drawline(x+30,y+ 0,x+40,y+20,canvas);
  drawline(x+40,y+20,x+30,y+40,canvas);
  drawline(x+30,y+40,x+10,y+40,canvas);
  drawline(x+10,y+40, x+0,y+20,canvas)}

function drawred (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.arc(x+20,y+20,12,0,2*Math.PI,false);
  ctx.stroke();
  ctx.fillStyle = "#ff8888";
  ctx.fill();}

function drawblack (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.arc(x+20,y+20,12,0,2*Math.PI,false);
  ctx.stroke();
  ctx.fillStyle = "#888888";
  ctx.fill();}

function drawblank (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.arc(x+20,y+20,12,0,2*Math.PI,false);
  ctx.stroke();
  ctx.fillStyle = "#ffffff";
  ctx.fill();}

function drawline(u,v,x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=1;
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
