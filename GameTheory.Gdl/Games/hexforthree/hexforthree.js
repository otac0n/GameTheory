//------------------------------------------------------------------------------
// Hex
//------------------------------------------------------------------------------

function renderstate (state)
 {var canvas = document.createElement('canvas');
  canvas.setAttribute('width','540px');
  canvas.setAttribute('height','441px');


  drawhex(240, 40,canvas);

  drawgreenish(210, 60,canvas);
  drawbluish(270, 60,canvas);

  drawgreenish(180, 80,canvas);
  drawhex(240, 80,canvas);
  drawbluish(300, 80,canvas);

  drawgreenish(150,100,canvas);
  drawhex(210,100,canvas);
  drawhex(270,100,canvas);
  drawbluish(330,100,canvas);

  drawhex(120,120,canvas);
  drawhex(180,120,canvas);
  drawhex(240,120,canvas);
  drawhex(300,120,canvas);
  drawhex(360,120,canvas);

  drawhex(150,140,canvas);
  drawhex(210,140,canvas);
  drawhex(270,140,canvas);
  drawhex(330,140,canvas);

  drawreddish(120,160,canvas);
  drawhex(180,160,canvas);
  drawhex(240,160,canvas);
  drawhex(300,160,canvas);
  drawreddish(360,160,canvas);

  drawhex(150,180,canvas);
  drawhex(210,180,canvas);
  drawhex(270,180,canvas);
  drawhex(330,180,canvas);

  drawreddish(120,200,canvas);
  drawhex(180,200,canvas);
  drawgrey(240,200,canvas);
  drawhex(300,200,canvas);
  drawreddish(360,200,canvas);

  drawhex(150,220,canvas);
  drawhex(210,220,canvas);
  drawhex(270,220,canvas);
  drawhex(330,220,canvas);

  drawreddish(120,240,canvas);
  drawhex(180,240,canvas);
  drawhex(240,240,canvas);
  drawhex(300,240,canvas);
  drawreddish(360,240,canvas);

  drawhex(150,260,canvas);
  drawhex(210,260,canvas);
  drawhex(270,260,canvas);
  drawhex(330,260,canvas);

  drawhex(120,280,canvas);
  drawhex(180,280,canvas);
  drawhex(240,280,canvas);
  drawhex(300,280,canvas);
  drawhex(360,280,canvas);

  drawbluish(150,300,canvas);
  drawhex(210,300,canvas);
  drawhex(270,300,canvas);
  drawgreenish(330,300,canvas);

  drawbluish(180,320,canvas);
  drawhex(240,320,canvas);
  drawgreenish(300,320,canvas);

  drawbluish(210,340,canvas);
  drawgreenish(270,340,canvas);

  drawhex(240,360,canvas);

  for (var i=0; i<state.length; i++)
      {if (state[i][0]=='cell')
          {var x = xpos(state[i][1],state[i][2]);
           var y = ypos(state[i][1],state[i][2]);
           if (state[i][3]=='red') {drawred(x,y,canvas)};
           if (state[i][3]=='green') {drawgreen(x,y,canvas)};
           if (state[i][3]=='blue') {drawblue(x,y,canvas)}}};

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
  ctx.fillStyle = "#ffe0e0";
  ctx.fill();
  ctx.stroke()}

function drawgreenish (x,y,w)
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
  ctx.fillStyle = "#e0ffe0";
  ctx.fill();
  ctx.stroke()}

function drawbluish (x,y,w)
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
  ctx.fillStyle = "#e0e0ff";
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
  ctx.fillStyle = "#888888";
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
  ctx.fillStyle = "#ff2222";
  ctx.fill();}

function drawgreen (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.arc(x+20,y+20,12,0,2*Math.PI,false);
  ctx.stroke();
  ctx.fillStyle = "#22ff22";
  ctx.fill();}

function drawblue (x,y,w)
 {var ctx = w.getContext('2d');
  ctx.beginPath();
  ctx.lineWidth=2;
  ctx.arc(x+20,y+20,12,0,2*Math.PI,false);
  ctx.stroke();
  ctx.fillStyle = "#2222ff";
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

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
