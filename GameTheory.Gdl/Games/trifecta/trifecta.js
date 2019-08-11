//------------------------------------------------------------------------------
// tictactoe
//------------------------------------------------------------------------------
init();
function renderstate (state)
{
	var table = document.createElement('table');
	table.setAttribute('cellspacing','0');
	var names = {'b': "Deck", "x": "White", "o": "Black"};
	makerow(table,'b',state,names);
	makerow(table,'x',state,names);
	makerow(table,'o',state,names);
	return table;
}

function getCards(type,state)
{
	return compfinds(seq('X','Y'),seq('cell','X','Y', type),state,seq());
}

function mapRC(rc)
{
	return (JSON.parse(rc[0]) - 1) * 3 + JSON.parse(rc[1]);
}

function makerow (table,player,state,names)
{
	var row = table.insertRow();
	makecell(row, names[player]);
	var _cards = getCards(player,state);//.join(',');
	var t = document.createElement('table');
	var r = t.insertRow();
	if (!_cards.length)
	{
		var ct = document.createElement('table');
		var ctr1 = ct.insertRow();
		makecell(ctr1,"&nbsp;",75,120);
		makecell(r,ct.outerHTML);
	}
	else
	{
		for (var i = 0; i < _cards.length; i++)
		{
			var c  = renderCard(mapRC(_cards[i]), 70);
			var ct = document.createElement('table');
			var ctr1 = ct.insertRow();
			makecell(ctr1,c.outerHTML,75,75);
			var ctr2 = ct.insertRow();
			makecell(ctr2,_cards[i].join(","));
			makecell(r,ct.outerHTML);
		}
	}
	makecell(row,t.outerHTML);
	//makecell(row, r);
  	return row;
}

function makecell (row,html,w,h)
{
	var cell = row.insertCell();
	if (typeof w != 'undefined')
	{
		cell.setAttribute('width',w);
	}
	if (typeof h != 'undefined')
	{
		cell.setAttribute('height',h);
	}
	cell.setAttribute('align','center');
	cell.setAttribute('valign','center');
	cell.setAttribute('style','font-family:HelveticaNeue-Light;font-size:12pt');
	cell.innerHTML = html;
	return cell;
}

/*CARD GENERATION PROCESS*/
function shuffle(array)
{
	var choice = [], output = [];
	for (var i = 0; i < array.length; i++)
	{
		choice.push(i);
	}
	for (var i = 0; i < array.length; i++)
	{
		var chosen = Math.floor(Math.random() * choice.length);
		output.push(array[choice[chosen]]), choice.splice(chosen, 1); 
	}
	return output;
}

function init()
{
	var concept = {};
	concept['rows'] = [[1,2,3], [4,5,6], [7,8,9]], concept['cols'] = [[1,4,7], [2,5,8], [3,6,9]], concept['dia1'] = [[1,5,9]], concept['dia2'] = [[3,5,7]];
	var resDia1 = [], resDia2 = [];
	for (var i = 1; i < 10; i++)
	{
		if (concept['dia1'][0].indexOf(i) == -1)
		{
			resDia1.push(i);
		}
		if (concept['dia2'][0].indexOf(i) == -1)
		{
			resDia2.push(i);
		}
	}
	resDia1 = shuffle(resDia1), resDia2 = shuffle(resDia2);
	for (var i = 0; i < 3; i++)
	{
		concept['dia1'].push([resDia1[i], resDia1[i + 3]]);
		concept['dia2'].push([resDia2[i], resDia2[i + 3]]);
	}
	var props = shuffle(['background', 'foreground', 'shape', 'number']), concepts = Object.keys(concept), value = {};
	palette = ['#F9A727', '#DD3C26', '#5138BD', '#F4F4F4', '#000000', '#27AE60'];
	value['background'] = shuffle(palette);
	value['foreground'] = shuffle(shuffle(palette));
	value['shape'] = shuffle(['&hearts;', '&diams;', '&clubs;', '&spades;']);
	value['number'] = shuffle([1, 2, 3, 4, 5]);
	_card = {};
	for (var i = 1; i <= 9; i++)
	{
		_card[i] = {};
	}
	for (var i = 0; i < concepts.length; i++)
	{
		var prop = props[i], c = concept[concepts[i]];
		for (var j = 0; j < c.length; j++)
		{
			var d = c[j];
			if ('foreground' in _card[d[0]] && prop == 'background')
			{
				var l = -1;
				var flag;
				do
				{
					l++; flag = false;
					for(var w = 0; w < d.length; w++)
					{
						if (value[prop][l] == _card[d[w]]['foreground'])
						{
							flag = true;
						}
					}
				} while (flag);
				for (var k = 0; k < d.length; k++)
				{
					var x = d[k];
					_card[x][prop] = value[prop][l];
				}
				value[prop].splice(l, 1);
			}
			else if ('background' in _card[d[0]] && prop == 'foreground')
			{
				var l = -1;
				var flag;
				do
				{
					l++; flag = false;
					for (var w = 0; w < d.length; w++)
					{
						if (value[prop][l] == _card[d[w]]['background'])
						{
							flag = true;
						}
					}
				} while (flag);
				for (var k = 0; k < d.length; k++)
				{
					var x = d[k];
					_card[x][prop] = value[prop][l];
				}
				value[prop].splice(l, 1);
			}
			else
			{
				for (var k = 0; k < d.length; k++)
				{
					var x = d[k];
					_card[x][prop] = value[prop][0];
				}
				value[prop].splice(0, 1);
			}
		}
	}
}

var gradient = {};
gradient['#5138BD'] = '#6F56DB';
gradient['#DD3C26'] = LightenDarkenColor('#DD3C26', -50);
gradient['#000000'] = '#444444'
gradient['#F4F4F4'] = '#CCCCCC';
gradient['#F9A727'] = '#EB8327';
gradient['#27AE60'] = '#40C779';
function LightenDarkenColor(col, amt) 
{
	var usePound = false;
  	if (col[0] == "#") 
	{
        col = col.slice(1);
        usePound = true;
    }
 	var num = parseInt(col,16), r = (num >> 16) + amt;
 
    if (r > 255)
	{
		r = 255;
    }
	else if (r < 0)
	{
		r = 0;
 	}
    var b = ((num >> 8) & 0x00FF) + amt;
    if (b > 255)
	{
		b = 255;
    }
	else if (b < 0) 
	{
		b = 0;
 	}
    var g = (num & 0x0000FF) + amt;
    if (g > 255)
	{
		g = 255;
    }
	else if (g < 0) 
	{
		g = 0;
 	}
    return (usePound?"#":"") + (g | (b << 8) | (r << 16)).toString(16);
}

function getCssValuePrefix(name, value) 
{
    var prefixes = ['', '-o-', '-ms-', '-moz-', '-webkit-'];
    var dom = document.createElement('div');
    for (var i = 0; i < prefixes.length; i++) 
	{
        dom.style[name] = prefixes[i] + value;
        if (dom.style[name]) 
		{
            return prefixes[i];
        }
        dom.style[name] = '';
    }
}

function renderCard(id, width)
{
	if (!id)
	{
		return;
	}
	sym = _card[id].shape, bkg = _card[id].background, fg = _card[id].foreground, n = _card[id].number;
	var div = [];
	var elem = document.createElement("div");
	var w = width, h = 1.4* w, sh = h / 3, sw = w / 2;
	//elem.style.width = w;
	//elem.style.height = h;
	var c3 = n % 2 ? sym : "&nbsp;", c1 = n > 1 ? sym : "&nbsp;", c2 = n > 3 ? sym : "&nbsp;"; 
	var gP = getCssValuePrefix('backgroundImage', 'linear-gradient(left, #fff, #fff)');
	var table = '<table class="card" cellspacing="0" width="' + w + '" height="' + h + '" cellpadding="0" style="text-align:center;font-size:' + (sh * 0.65) + 'px;color:' + fg + ';font-family:HelveticaNeue-Light;border-radius:5px;background:' + gP + "linear-gradient(" + ["left", gradient[bkg], bkg, gradient[bkg]].join(', ') + ');"><tr><td>' + c1 + '</td><td>' + c2 + '</td></tr><tr><td valign="middle" colspan="2">' + c3 + '</td></tr><tr><td>' + c2 + '</td><td>' + c1 + '</td></tr></table>';
	elem.innerHTML = table;
	return elem.firstChild  ;
}
