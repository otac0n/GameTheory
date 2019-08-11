//------------------------------------------------------------------------------
// Toplevel subroutines
//------------------------------------------------------------------------------

function findroles (rules)
 {return roles(seq())}

function findlegals (role,facts,rules)
 {return legals(role,facts).sort()}

function findnexts (facts,rules)
 {return nexts(facts).sort()}

function findreward (role,facts,rules)
 {return rewardbx(role,facts)}

function findterminalp (facts,rules)
 {return terminal(facts)}

//------------------------------------------------------------------------------
// GDL rules
//------------------------------------------------------------------------------

function roles (facts)
 {return seq('white','black')}

//------------------------------------------------------------------------------

function legals (role,facts)
 {var answers = seq();
  answers = answers.concat(legal1bs(role,facts));
  answers = answers.concat(legal2bs(role,facts));
  answers = answers.concat(legal3bs(role,facts));
  return vniquify(answers)}

function legal1bs (role,facts)
 {var answers = seq();
  if (controlb(role,facts))
     {var dum1 = objects('color',role,facts);
      for (var i=0; i<dum1.length; i++)
          {var dum2 = locationssb(dum1[i],facts);
           for (var j=0; j<dum2.length; j++)
               {var dum3 = validmovebbbss(dum1[i],dum2[j][0],dum2[j][1],facts);
                for (var k=0; k<dum3.length; k++)
                    {if (!celloccupiedbybbb(dum3[k][0],dum3[k][1],role,facts))
                        {answers.push(seq('move',dum2[j][0],dum2[j][1],dum3[k][0],dum3[k][1]))}}}}};
  return answers}

function legal2bs (role,facts)
 {var answers = seq();
  if (controlb(role,facts))
     {var dum1 = objects('color',role,facts);
      for (var i=0; i<dum1.length; i++)
          {if (!onboardb(dum1[i],facts))
              {var dum2 = cellemptyss(facts);
               for (var j=0; j<dum2.length; j++)
                   {answers.push(seq('place',dum1[i],dum2[j][0],dum2[j][1]))}}}};
  return answers}

function legal3bs (role,facts)
 {return seq('noop')}

//------------------------------------------------------------------------------

function nexts (facts)
 {var answers = seq();
  answers = answers.concat(next01s(facts));
  answers = answers.concat(next02s(facts));
  answers = answers.concat(next03s(facts));
  answers = answers.concat(next04s(facts));
  answers = answers.concat(next05s(facts));
  answers = answers.concat(next06s(facts));
  answers = answers.concat(next07s(facts));
  answers = answers.concat(next08s(facts));
  answers = answers.concat(next09s(facts));
  answers = answers.concat(next10s(facts));
  answers = answers.concat(next11s(facts));
  answers = answers.concat(next12s(facts));
  answers = answers.concat(next13s(facts));
  answers = answers.concat(next14s(facts));
  answers = answers.concat(next15s(facts));
  answers = answers.concat(next16s(facts));
  answers = answers.concat(next17s(facts));
  return answers}

function next01s (facts)
 {var answers = seq();
  var dum1 = doesmovesssss(facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = locationbbs(dum1[i][1],dum1[i][2],facts);
       for (var j=0; j<dum2.length; j++)
           {answers.push(seq('location',dum1[i][3],dum1[i][4],dum2[j]))}};
  return answers}

function next02s (facts)
 {var answers = seq();
  var dum1 = doesmovesssss(facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = locationsss(facts);
       for (var j=0; j<dum2.length; j++)
           {var fxy = seq('f',dum2[j][0],dum2[j][1]);
            var fx1y1 = seq('f',dum1[i][1],dum1[i][2]);
            var fx2y2 = seq('f',dum1[i][3],dum1[i][4]);
            if (!equalp(fxy,fx1y1) && !equalp(fxy,fx2y2))
               {answers.push(seq('location',dum2[j][0],dum2[j][1],dum2[j][2]))}}};
  return answers}

function next03s (facts)
 {var answers = seq();
  var dum = doesplacessss(facts);
  for (var i=0; i<dum.length; i++)
      {answers.push(seq('location',dum[i][2],dum[i][3],dum[i][1]))};
  return answers}

function next04s (facts)
 {var answers = seq();
  var dum1 = doesplacessss(facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = locationsss(facts);
       for (var j=0; j<dum2.length; j++)
           {answers.push(seq('location',dum2[j][0],dum2[j][1],dum2[j][2]))}};
  return answers}

function next05s (facts)
 {var answers = seq();
  var dum1 = controls(facts);
  for (var i=0; i<dum1.length; i++)
      {if (doesnoopb(dum1[0],facts))
          {var dum2 = locationsss(facts);
           for (var j=0; j<dum2.length; j++)
               {answers.push(seq('location',dum2[j][0],dum2[j][1],dum2[j][2]))}}};
  return answers}

function next06s (facts)
 {var answers = seq();
  var dum1 = doesmovesssss(facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = locationbbs(dum1[i][3],dum1[i][4],facts);
       for (var j=0; j<dum2.length; j++)
           {answers.push(seq('color',dum2[j],dum1[i][0]))}};
  return answers}

function next07s (facts)
 {var answers = seq();
  var dum1 = doesmovesssss(facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = colorss(facts);
       for (var j=0; j<dum2.length; j++)
           {if (!locationbbb(dum1[i][3],dum1[i][4],dum2[j][0],facts))
               {answers.push(seq('color',dum2[j][0],dum2[j][1]))}}};
  return answers}

function next08s (facts)
 {var answers = seq();
  var dum1 = doesplacessss(facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = colorss(facts);
       for (var j=0; j<dum2.length; j++)
           {answers.push(seq('color',dum2[j][0],dum2[j][1]))}};
  return answers}

function next09s (facts)
 {var answers = seq();
  var dum1 = controls(facts);
  for (var i=0; i<dum1.length; i++)
      {if (doesnoopb(dum1[0],facts))
          {var dum2 = colorss(facts);
           for (var j=0; j<dum2.length; j++)
               {answers.push(seq('color',dum2[j][0],dum2[j][1]))}}};
  return answers}

function next10s (facts)
 {var answers = seq();
  var dum1 = doesmovesssss(facts);
  for (var i=0; i<dum1.length; i++)
      {if (celloccupiedbb(dum1[i][3],dum1[i][4],facts))
          {var dum2 = strengthbs(dum1[i][0],facts);
           for (var j=0; j<dum2.length; j++)
               {var dum3 = ppbs(dum2[j],facts);
                for (var k=0; k<dum3.length; k++)
                    {answers.push(seq('strength',dum1[i][0],dum3[k]))}}}};
  return answers}

function next11s (facts)
 {var answers = seq();
  var dum1 = doesmovesssss(facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = celloccupiedbybbs(dum1[i][3],dum1[i][4],facts);
       for (var j=0; j<dum2.length; j++)
           {var dum3 = strengthbs(dum2[j],facts);
            for (var k=0; k<dum3.length; k++)
               {var dum4 = ppsb(dum3[k],facts);
                for (var l=0; l<dum4.length; l++)
                    {answers.push(seq('strength',dum3[k],dum4[l]))}}}};
  return answers}

function next12s (facts)
 {var answers = seq();
  var dum1 = doesmovesssss(facts);
  for (var i=0; i<dum1.length; i++)
      {if (!celloccupiedbb(dum1[i][3],dum1[i][4],facts))
          {var dum2 = strengthss(facts);
           for (var j=0; j<dum2.length; j++)
               {answers.push(seq('strength',dum2[j][0],dum2[j][1]))}}};
  return answers}

function next13s (facts)
 {var answers = seq();
  var dum1 = doesplacessss(facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = strengthss(facts);
       for (var j=0; j<dum2.length; j++)
           {answers.push(seq('strength',dum2[j][0],dum2[j][1]))}};
  return answers}

function next14s (facts)
 {var answers = seq();
  var dum1 = controls(facts);
  for (var i=0; i<dum1.length; i++)
      {if (doesnoopb(dum1[0],facts))
          {var dum2 = strengthss(facts);
           for (var j=0; j<dum2.length; j++)
               {answers.push(seq('strength',dum2[j][0],dum2[j][1]))}}};
  return answers}

function next15s (facts)
 {if (controlb('black',facts)) {return seq(seq('control','white'))};
  return seq()}

function next16s (facts)
 {if (controlb('white',facts)) {return seq(seq('control','black'))};
  return seq()}

function next17s (facts)
 {var answers = seq();
  var dum1 = steps(facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = ppbs(dum1[i],facts);
       for (var j=0; j<dum2.length; j++)
           {answers.push(seq('step',dum2[j]))}};
  return answers}

//------------------------------------------------------------------------------

function rewardbx (role,facts)
 {var count = colorbx(role,facts);
  return scoremapbx(count,facts)}

//------------------------------------------------------------------------------

function terminal (facts)
 {if (strengthxb(32,facts)) {return true};
  if (stepb(100,facts)) {return true};
  return false}

//------------------------------------------------------------------------------
// views
//------------------------------------------------------------------------------

function validmovebbbss (piece,x,y,facts)
 {var answers = seq();
  answers = answers.concat(validmove1bbbss(piece,x,y,facts));
  answers = answers.concat(validmove2bbbss(piece,x,y,facts));
  answers = answers.concat(validmove3bbbss(piece,x,y,facts));
  answers = answers.concat(validmove5bbbss(piece,x,y,facts));
  answers = answers.concat(validmove6bbbss(piece,x,y,facts));
  return answers}

function validmove1bbbss (piece,x,y,facts)
 {if (typebb(piece,'pawn',facts) && colorbb(piece,'white',facts))
     {return whitepawnmovebbss(x,y,facts)};
  return seq()}

function validmove2bbbss (piece,x,y,facts)
 {if (typebb(piece,'pawn',facts) && colorbb(piece,'black',facts))
     {return blackpawnmovebbss(x,y,facts)};
  return seq()}
 
function validmove3bbbss (piece,x,y,facts)
 {if (typebb(piece,'rook',facts))
     {return rookmovebbss(x,y,facts)};
  return seq()}
 
function validmove5bbbss (piece,x,y,facts)
 {if (typebb(piece,'bishop',facts))
     {return bishopmovebbss(x,y,facts)};
  return seq()}
 
function validmove6bbbss (piece,x,y,facts)
 {if (typebb(piece,'queen',facts))
     {return queenmovebbss(x,y,facts)};
  return seq()}
 
//------------------------------------------------------------------------------

function whitepawnmovebbss (x,y,facts)
 {var answers = seq();
  answers = answers.concat(whitepawnmove1bbss(x,y,facts));
  answers = answers.concat(whitepawnmove2bbss(x,y,facts));
  answers = answers.concat(whitepawnmove3bbss(x,y,facts));
  answers = answers.concat(whitepawnmove4bbss(x,y,facts));
  return answers}

function whitepawnmove1bbss (x,y,facts)
 {var answers = seq();
  if (y==='2' && cellemptybb(x,'3',facts) && cellemptybb(x,'4',facts))
     {answers = seq(seq(x,'4'))};
  return answers}

function whitepawnmove2bbss (x,y,facts)
 {var answers = seq();
  var dum = nextrankbs(y,facts);
  for (var i=0; i<dum.length; i++)
      {if (cellemptybb(x,dum[i],facts)) {answers.push(seq(x,dum[i]))}};
  return answers}

function whitepawnmove3bbss (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilebs(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextrankbs(y,facts);
       for (var j=0; j<dum2.length; j++)
           {if (celloccupiedbybbb(dum1[i],dum2[j],'black',facts))
               {answers.push(seq(dum1[i],dum2[j]))}}};
  return answers}
  
function whitepawnmove4bbss (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilesb(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextrankbs(y,facts);
       for (var j=0; j<dum2.length; j++)
           {if (celloccupiedbybbb(dum1[i],dum2[j],'black',facts))
               {answers.push(seq(dum1[i],dum2[j]))}}};
  return answers}

//------------------------------------------------------------------------------

function blackpawnmovebbss (x,y,facts)
 {var answers = seq();
  answers = answers.concat(blackpawnmove1bbss(x,y,facts));
  answers = answers.concat(blackpawnmove2bbss(x,y,facts));
  answers = answers.concat(blackpawnmove3bbss(x,y,facts));
  answers = answers.concat(blackpawnmove4bbss(x,y,facts));
  return answers}

function blackpawnmove1bbss (x,y,facts)
 {var answers = seq();
  if (y==='7' && cellemptybb(x,'6',facts) && cellemptybb(x,'5',facts))
     {answers = seq(seq(x,'5'))};
  return answers}

function blackpawnmove2bbss (x,y,facts)
 {var answers = seq();
  var dum = nextranksb(y,facts);
  for (var i=0; i<dum.length; i++)
      {if (cellemptybb(x,dum[i],facts)) {answers.push(seq(x,dum[i]))}};
  return answers}

function blackpawnmove3bbss (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilebs(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextranksb(y,facts);
       for (var j=0; j<dum2.length; j++)
           {if (celloccupiedbybbb(dum1[i],dum2[j],'white',facts))
               {answers.push(seq(dum1[i],dum2[j]))}}};
  return answers}

function blackpawnmove4bbss (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilesb(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextranksb(y,facts);
       for (var j=0; j<dum2.length; j++)
           {if (celloccupiedbybbb(dum1[i],dum2[j],'white',facts))
               {answers.push(seq(dum1[i],dum2[j]))}}};
  return answers}

//------------------------------------------------------------------------------

function rookmovebbss (x,y,facts)
 {var answers = seq();
  answers = answers.concat(horizontalmovebbss(x,y,facts));
  answers = answers.concat(horizontalmovessbb(x,y,facts));
  answers = answers.concat(verticalmovebbss(x,y,facts));
  answers = answers.concat(verticalmovessbb(x,y,facts));
  return answers}

//------------------------------------------------------------------------------

function bishopmovebbss (x,y,facts)
 {var answers = seq();
  answers = answers.concat(northeastmovebbss(x,y,facts));
  answers = answers.concat(northeastmovessbb(x,y,facts));
  answers = answers.concat(southeastmovebbss(x,y,facts));
  answers = answers.concat(southeastmovessbb(x,y,facts));
  return answers}

//------------------------------------------------------------------------------

function queenmovebbss (x,y,facts)
 {var answers = seq();
  answers = answers.concat(rookmovebbss(x,y,facts));
  answers = answers.concat(bishopmovebbss(x,y,facts));
  return answers}

//------------------------------------------------------------------------------

function horizontalmovebbss (x,y,facts)
 {var answers = seq();
  answers = answers.concat(horizontalmove1bbss(x,y,facts));
  answers = answers.concat(horizontalmove2bbss(x,y,facts));
  return answers}

function horizontalmove1bbss (x,y,facts)
 {var answers = seq();
  var dum = nextfilebs(x,facts);
  for (var i=0; i<dum.length; i++)
      {answers.push(seq(dum[i],y))};
  return answers}

function horizontalmove2bbss (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilebs(x,facts);
  for (var i=0; i<dum1.length; i++)
      {if (cellemptybb(dum1[i],y,facts))
          {var dum2 = horizontalmovebbss(dum1[i],y,facts);
           for (var j=0; j<dum2.length; j++)
               {answers.push(dum2[j])}}};
  return answers}

function horizontalmovessbb (x,y,facts)
 {var answers = seq();
  answers = answers.concat(horizontalmove1ssbb(x,y,facts));
  answers = answers.concat(horizontalmove2ssbb(x,y,facts));
  return answers}

function horizontalmove1ssbb (x,y,facts)
 {var answers = seq();
  var dum = nextfilesb(x,facts);
  for (var i=0; i<dum.length; i++)
      {answers.push(seq(dum[i],y))};
  return answers}

function horizontalmove2ssbb (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilesb(x,facts);
  for (var i=0; i<dum1.length; i++)
      {if (cellemptybb(dum1[i],y,facts))
          {var dum2 = horizontalmovessbb(dum1[i],y,facts);
           for (var j=0; j<dum2.length; j++)
               {answers.push(dum2[j])}}};
  return answers}

//------------------------------------------------------------------------------

function verticalmovebbss (x,y,facts)
 {var answers = seq();
  answers = answers.concat(verticalmove1bbss(x,y,facts));
  answers = answers.concat(verticalmove2bbss(x,y,facts));
  return answers}

function verticalmove1bbss (x,y,facts)
 {var answers = seq();
  var dum = nextrankbs(y,facts);
  for (var i=0; i<dum.length; i++)
      {answers.push(seq(x,dum[i]))};
  return answers}

function verticalmove2bbss (x,y,facts)
 {var answers = seq();
  var dum1 = nextrankbs(y,facts);
  for (var i=0; i<dum1.length; i++)
      {if (cellemptybb(x,dum1[i],facts))
          {var dum2 = verticalmovebbss(x,dum1[i],facts);
           for (var j=0; j<dum2.length; j++)
               {answers.push(dum2[j])}}};
  return answers}

function verticalmovessbb (x,y,facts)
 {var answers = seq();
  answers = answers.concat(verticalmove1ssbb(x,y,facts));
  answers = answers.concat(verticalmove2ssbb(x,y,facts));
  return answers}

function verticalmove1ssbb (x,y,facts)
 {var answers = seq();
  var dum = nextranksb(y,facts);
  for (var i=0; i<dum.length; i++)
      {answers.push(seq(x,dum[i]))};
  return answers}

function verticalmove2ssbb (x,y,facts)
 {var answers = seq();
  var dum1 = nextranksb(y,facts);
  for (var i=0; i<dum1.length; i++)
      {if (cellemptybb(x,dum1[i],facts))
          {var dum2 = verticalmovessbb(x,dum1[i],facts);
           for (var j=0; j<dum2.length; j++)
               {answers.push(dum2[j])}}};
  return answers}

//------------------------------------------------------------------------------

function northeastmovebbss (x,y,facts)
 {var answers = seq();
  answers = answers.concat(northeastmove1bbss(x,y,facts));
  answers = answers.concat(northeastmove2bbss(x,y,facts));
  return answers}

function northeastmove1bbss (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilebs(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextrankbs(y,facts);
       for (var j=0; j<dum2.length; j++)
           {answers.push(seq(dum1[i],dum2[j]))}};
  return answers}

function northeastmove2bbss (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilebs(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextrankbs(y,facts);
       for (var j=0; j<dum2.length; j++)
           {if (cellemptybb(dum1[i],dum2[j],facts))
               {var dum3 = northeastmovebbss(dum1[i],dum2[j],facts);
                for (var k=0; k<dum3.length; k++)
                    {answers.push(dum3[k])}}}};
  return answers}

function northeastmovessbb (x,y,facts)
 {var answers = seq();
  answers = answers.concat(northeastmove1ssbb(x,y,facts));
  answers = answers.concat(northeastmove2ssbb(x,y,facts));
  return answers}

function northeastmove1ssbb (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilesb(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextranksb(y,facts);
       for (var j=0; j<dum2.length; j++)
           {answers.push(seq(dum1[i],dum2[j]))}};
  return answers}

function northeastmove2ssbb (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilesb(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextranksb(y,facts);
       for (var j=0; j<dum2.length; j++)
           {if (cellemptybb(dum1[i],dum2[j],facts))
               {var dum3 = northeastmovessbb(dum1[i],dum2[j],facts);
                for (var k=0; k<dum3.length; k++)
                    {answers.push(dum3[k])}}}};
  return answers}

//------------------------------------------------------------------------------

function southeastmovebbss (x,y,facts)
 {var answers = seq();
  answers = answers.concat(southeastmove1bbss(x,y,facts));
  answers = answers.concat(southeastmove2bbss(x,y,facts));
  return answers}

function southeastmove1bbss (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilebs(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextranksb(y,facts);
       for (var j=0; j<dum2.length; j++)
           {answers.push(seq(dum1[i],dum2[j]))}};
  return answers}

function southeastmove2bbss (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilebs(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextranksb(y,facts);
       for (var j=0; j<dum2.length; j++)
           {if (cellemptybb(dum1[i],dum2[j],facts))
               {var dum3 = southeastmovebbss(dum1[i],dum2[j],facts);
                for (var k=0; k<dum3.length; k++)
                    {answers.push(dum3[k])}}}};
  return answers}

function southeastmovessbb (x,y,facts)
 {var answers = seq();
  answers = answers.concat(southeastmove1ssbb(x,y,facts));
  answers = answers.concat(southeastmove2ssbb(x,y,facts));
  return answers}

function southeastmove1ssbb (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilesb(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextrankbs(y,facts);
       for (var j=0; j<dum2.length; j++)
           {answers.push(seq(dum1[i],dum2[j]))}};
  return answers}

function southeastmove2ssbb (x,y,facts)
 {var answers = seq();
  var dum1 = nextfilesb(x,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = nextrankbs(y,facts);
       for (var j=0; j<dum2.length; j++)
           {if (cellemptybb(dum1[i],dum2[j],facts))
               {var dum3 = southeastmovessbb(dum1[i],dum2[j],facts);
                for (var k=0; k<dum3.length; k++)
                    {answers.push(dum3[k])}}}};
  return answers}

//------------------------------------------------------------------------------

function onboardb (piece,facts)
 {if (locationssb(piece,facts).length>0) {return true};
  return false}

//------------------------------------------------------------------------------

function celloccupiedbb(x,y,facts)
 {if (locationbbx(x,y,facts)) {return true};
  return false}

//------------------------------------------------------------------------------

function celloccupiedbybbb(x,y,role,facts)
 {if (colorbb(locationbbx(x,y,facts),role,facts)) {return true};
  return false}

function celloccupiedbybbs(x,y,facts)
 {var answers = seq();
  var dum1 = locationbbs(x,y,facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = colorbs(dum1[i],facts);
       for (var j=0; j<dum2.length; j++)
           {answers.push(dum2[j])}};
  return answers}

//------------------------------------------------------------------------------

function cellemptybb (x,y,facts)
 {return !locationbbx(x,y,facts)}

function cellemptyss (facts)
 {var answers = seq();
  var dum1 = files(facts);
  for (var i=0; i<dum1.length; i++)
      {var dum2 = ranks(facts);
       for (var j=0; j<dum2.length; j++)
           {if (!celloccupiedbb(dum1[i],dum2[j],facts))
               {answers.push(seq(dum1[i],dum2[j]))}}};
  return answers}

//------------------------------------------------------------------------------
// GDL data
//------------------------------------------------------------------------------

function files (facts)
 {return seq('a','b','c','d','e','f','g','h')}

function ranks (facts)
 {return seq('1','2','3','4','5','6','7','8')}

function nextfilebs (x,facts)
 {if (x==='a') {return seq('b')};
  if (x==='b') {return seq('c')};
  if (x==='c') {return seq('d')};
  if (x==='d') {return seq('e')};
  if (x==='e') {return seq('f')};
  if (x==='f') {return seq('g')};
  if (x==='g') {return seq('h')};
  return seq()}

function nextfilesb (x,facts)
 {if (x==='b') {return seq('a')};
  if (x==='c') {return seq('b')};
  if (x==='d') {return seq('c')};
  if (x==='e') {return seq('d')};
  if (x==='f') {return seq('e')};
  if (x==='g') {return seq('f')};
  if (x==='h') {return seq('g')};
  return seq()}

function nextrankbs (y,facts)
 {if (y==='1') {return seq('2')};
  if (y==='2') {return seq('3')};
  if (y==='3') {return seq('4')};
  if (y==='4') {return seq('5')};
  if (y==='5') {return seq('6')};
  if (y==='6') {return seq('7')};
  if (y==='7') {return seq('8')};
  return seq()}

function nextranksb (y,facts)
 {if (y==='2') {return seq('1')};
  if (y==='3') {return seq('2')};
  if (y==='4') {return seq('3')};
  if (y==='5') {return seq('4')};
  if (y==='6') {return seq('5')};
  if (y==='7') {return seq('6')};
  if (y==='8') {return seq('7')};
  return seq()}

function typebb (piece,type,facts)
 {if (piece[0]==='k') {return (type==='king')};
  if (piece[0]==='q') {return (type==='queen')};
  if (piece[0]==='b') {return (type==='bishop')};
  if (piece[0]==='n') {return (type==='knight')};
  if (piece[0]==='r') {return (type==='rook')};
  if (piece[0]==='p') {return (type==='pawn')};
  return false}

function scoremapbx (count,facts)
 {if (count==='0') {return '0'};
  if (count==='1') {return '3'};
  if (count==='2') {return '6'};
  if (count==='3') {return '9'};
  if (count==='4') {return '12'};
  if (count==='5') {return '15'};
  if (count==='6') {return '18'};
  if (count==='7') {return '21'};
  if (count==='8') {return '25'};
  if (count==='9') {return '28'};
  if (count==='10') {return '31'};
  if (count==='11') {return '34'};
  if (count==='12') {return '37'};
  if (count==='13') {return '40'};
  if (count==='14') {return '43'};
  if (count==='15') {return '46'};
  if (count==='16') {return '50'};
  if (count==='17') {return '53'};
  if (count==='18') {return '56'};
  if (count==='19') {return '59'};
  if (count==='20') {return '62'};
  if (count==='21') {return '65'};
  if (count==='22') {return '68'};
  if (count==='23') {return '71'};
  if (count==='24') {return '75'};
  if (count==='25') {return '78'};
  if (count==='26') {return '81'};
  if (count==='27') {return '84'};
  if (count==='28') {return '87'};
  if (count==='29') {return '90'};
  if (count==='30') {return '93'};
  if (count==='31') {return '96'};
  if (count==='32') {return '100'};
  return false}

function ppbx (m,facts)
 {m = m*1;
  if (m>=0 && m<=99) {return stringize(m+1)};
  return false}

function ppbs (m,facts)
 {m = m*1;
  if (m>=0 && m<=99) {return seq(stringize(m+1))};
  return seq()}

function ppsb (n,facts)
 {n = n*1;
  if (n>=1 && n<=100) {return seq(stringize(n-1))};
  return seq()}

//------------------------------------------------------------------------------
// Input data
//------------------------------------------------------------------------------

function doesmovesssss (facts)
 {var answers = seq();
  for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='does')
          {var role = facts[i][1];
           var action = facts[i][2];
           if (action[0]==='move')
              {answers.push(seq(role,action[1],action[2],action[3],action[4]))}}};
  return answers}

function doesplacessss (facts)
 {var answers = seq();
  for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='does')
          {var role = facts[i][1];
           var action = facts[i][2];
           if (action[0]==='place')
              {answers.push(seq(role,action[1],action[2],action[3]))}}};
  return answers}

function doesnoopb (role,facts)
 {var answers = seq();
  for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='does')
          {if (facts[i][1]===role && facts[i][2]==='noop')
              {return true}}};
  return false}

//------------------------------------------------------------------------------
// State data
//------------------------------------------------------------------------------

function locationbbb (x,y,piece,facts)
 {for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]==='location' && fact[1]===x &&  fact[2]===y && fact[3]===piece)
              {return true}}};
  return false}

function locationbbx (x,y,facts)
 {for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]==='location' && fact[1]===x &&  fact[2]===y)
              {return fact[3]}}};
  return false}

function locationbbs (x,y,facts)
 {var answers = seq();
  for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]==='location' && fact[1]===x &&  fact[2]===y)
              {answers.push(fact[3])}}};
  return answers}

function locationssb (piece,facts)
 {var answers = seq();
  for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]==='location' && fact[3]===piece)
              {answers.push(seq(fact[1],fact[2]))}}};
  return answers}

function locationsss (facts)
 {var answers = seq();
  for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]==='location')
              {answers.push(seq(fact[1],fact[2],fact[3]))}}};
  return answers}

function colorbb (piece,color,facts)
 {for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]==='color' && fact[1]===piece && fact[2]===color)
              {return true}}};
  return false}

function colorbs (piece,facts)
 {return results('color',piece,facts)}

function colorss (facts)
 {var answers = seq();
  for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]==='color')
              {answers.push(seq(fact[1],fact[2]))}}};
  return answers}

function colorbx (x,facts)
 {return result('color',x,facts)}

function controlb (x,facts)
 {return itemp('control',x,facts)}

function controls (facts)
 {return items('control',facts)}

function strengthxb (x,facts)
 {return object('strength',x,facts)}

function strengthbs (x,facts)
 {return results('strength',x,facts)}

function strengthss (facts)
 {var answers = seq();
  for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]==='strength')
              {answers.push(seq(fact[1],fact[2]))}}};
  return answers}

function stepb (x,facts)
 {return itemp('step',x,facts)}

function steps (facts)
 {return items('step',facts)}

//------------------------------------------------------------------------------
// general subroutines adapted from infoserver
//------------------------------------------------------------------------------

function itemp (predicate,object,facts)
 {for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]===predicate && fact[1]===object) {return true}}};
  return false}

function items (predicate,facts)
 {var answers = seq();
  for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]===predicate) {answers.push(fact[1])}}};
  return answers}

function object (slot,value,facts)
 {for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]===slot && fact[2]===value)
              {return fact[1]}}};
  return false}

function objects (slot,value,facts)
 {var answers = seq();
  for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]===slot && fact[2]===value)
              {answers.push(fact[1])}}};
  return answers}

function result (slot,object,facts)
 {for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]===slot && fact[1]===object)
              {return fact[2]}}};
  return false}

function results (slot,object,facts)
 {var answers = seq();
  for (var i=0; i<facts.length; i++)
      {if (facts[i][0]==='true')
          {var fact = facts[i][1];
           if (fact[0]===slot && fact[1]===object)
              {answers.push(fact[2])}}};
  return answers}

function runtime (form)
 {var beg = Date.now();
  var answer = eval(form);
  var end = Date.now();
  console.log((end-beg) + " milliseconds.");
  return answer}

//------------------------------------------------------------------------------
// compiler
//------------------------------------------------------------------------------

function compiletest (rel,rules)
 {var facts = sentences(rel,rules);
  var arity = getfactarity(rel,facts);
  return compile_ground (rel,arity,facts)}

function compile_ground (rel,arity,facts)
 {var namestring = rel + 'b';
  var argstring = '(x1';
  var args = seq('x1');
  for (var i=2; i<=arity; i++)
      {namestring = namestring + 'b';
       args.push('x' + i);
       argstring = argstring + ',x' + i};
  argstring = argstring + ')'
  var code = 'function ' + namestring + ' ' + argstring + '\r {';
  for (var i=0; i<facts.length; i++)
      {code = code + makeconditional(args,facts[i]) + '\r'};
  code = code + 'return false}\r\r';
  return code}

function makeconditional (args,fact)
 {return '  if (' + makematchtest(args,fact) + ') {return true};'}

function makematchtest (args,fact)
 {if (args.length===0) {return true};
  var code = args[0] + "===" + "'" + fact[1] + "'";
  for (var i=1; i<args.length; i++)
      {code = code + ' && ' + args[i] + "===" + "'" + fact[i+1] + "'"};
  return code}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
