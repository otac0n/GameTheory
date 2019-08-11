
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:param name="width" select="560"/>
<xsl:param name="height" select="600"/>
<xsl:template name="main" match="/">  
  <div> <!-- Set Style -->    
  <style type="text/css" media="all"> 
    td.at {        
      width:  <xsl:value-of select="$width * 0.09"/>px; 
      height: <xsl:value-of select="$height * 0.09"/>px;        
      border: 2px solid #000;        background-color: #FFFFCC;        
      align: center;  valign: middle;    
      background-size: "<xsl:value-of select="$width * 0.09"/>px <xsl:value-of select="$height * 0.09"/>px"; 
    }      
    table.board {   background-color: #000000;  }      
    img.piece {        
      width:   <xsl:value-of select="$width * 0.9 * 0.08"/>px;
      height:   <xsl:value-of select="$height * 0.9 * 0.08"/>px;
    }
    font.piece {
      font-size: <xsl:value-of select="$height * 0.9 * 0.08"/>px;
      color: purple;
    }
  </style>        
  <!-- Draw Board -->    
  <xsl:call-template   name="board">      
    <xsl:with-param name="cols" select="9"/>   
    <xsl:with-param name="rows" select="9"/>   
  </xsl:call-template>     
 </div>   
</xsl:template>
  <xsl:template name="sheep" match="state/fact">
    <xsl:param name="index" select="1"/>
    <xsl:choose>
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[3]='up']">
        <xsl:attribute   name="style">background-image: url(/ggp/games/fireSheep/sheep_up.png)</xsl:attribute> 
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[3]='down']">
        <xsl:attribute   name="style">background-image:  url(/ggp/games/fireSheep/sheep_down.png)</xsl:attribute> 
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[3]='left']">
        <xsl:attribute   name="style">background-image:  url(/ggp/games/fireSheep/sheep_left.png)</xsl:attribute> 
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[3]='right']">
        <xsl:attribute   name="style">background-image:  url(/ggp/games/fireSheep/sheep_right.png)</xsl:attribute> 
      </xsl:when>  
    </xsl:choose>  
    <xsl:choose>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[2]=0]">
        <font class="piece">0</font>
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[2]=1]">
        <font class="piece">1</font>
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[2]=2]">
        <font class="piece">2</font>
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[2]=3]">
        <font class="piece">3</font>
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[2]=4]">
        <font class="piece">4</font>
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[2]=5]">
        <font class="piece">5</font>
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[2]=6]">
        <font class="piece">6</font>
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[2]=7]">
        <font class="piece">7</font>
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[2]=8]">
        <font class="piece">8</font>
      </xsl:when>  
      <xsl:when test="//fact[relation='sheep' and argument[1]=$index and argument[2]=9]">
        <font class="piece">9</font>
      </xsl:when>  
    </xsl:choose>
    <xsl:value-of select="$index"/>
  </xsl:template>
  <xsl:template name="at" match="state/fact"> 
  <xsl:param name="row" select="1"/>  <xsl:param name="col"   select="1"/> 
  <td class="at">  
  <xsl:attribute name="id">  
    <xsl:value-of select="'at_'"/>   
      <xsl:value-of   select="$row"/>   <xsl:value-of select="$col"/>   
  </xsl:attribute>    
  
  <xsl:choose>   
    <xsl:when test="//fact[relation='grass' and argument[1]='red' and argument[2] = $row and argument[3]=$col]"> 
      <xsl:attribute   name="style">background-color: #FF0000</xsl:attribute> 
    </xsl:when>   
    <xsl:when test="//fact[relation='grass' and argument[1]='blue' and argument[2] = $row and argument[3]=$col]"> 
      <xsl:attribute   name="style">background-color: #0000FF</xsl:attribute> 
    </xsl:when>   
    <xsl:when test="//fact[relation='burning' and argument[2]=$row and argument[3]=$col]">  
      <xsl:attribute   name="style">background-color: #FF6600</xsl:attribute> 
    </xsl:when>  
    <xsl:when test="//fact[relation='frozen' and argument[2]=$row and argument[3]=$col]">  
      <xsl:attribute   name="style">background-color: #66CDAA</xsl:attribute> 
    </xsl:when>  
  </xsl:choose>  

  <center>
  <xsl:choose>
    <xsl:when test="//fact[relation='at' and argument[1]=1 and argument[2]=$row and argument[3]=$col]">
      <xsl:call-template name="sheep">
        <xsl:with-param name="index" select="1"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="//fact[relation='at' and argument[1]=2 and argument[2]=$row and argument[3]=$col]">  
      <xsl:call-template name="sheep">
        <xsl:with-param name="index" select="2"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="//fact[relation='at' and argument[1]=3 and argument[2]=$row and argument[3]=$col]">  
      <xsl:call-template name="sheep">
        <xsl:with-param name="index" select="3"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="//fact[relation='at' and argument[1]=4 and argument[2]=$row and argument[3]=$col]">  
      <xsl:call-template name="sheep">
        <xsl:with-param name="index" select="4"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="//fact[relation='at' and argument[1]=5 and argument[2]=$row and argument[3]=$col]">  
      <xsl:call-template name="sheep">
        <xsl:with-param name="index" select="5"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="//fact[relation='at' and argument[1]=6 and argument[2]=$row and argument[3]=$col]">  
      <xsl:call-template name="sheep">
        <xsl:with-param name="index" select="6"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="//fact[relation='at' and argument[1]=7 and argument[2]=$row and argument[3]=$col]">  
      <xsl:call-template name="sheep">
        <xsl:with-param name="index" select="7"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="//fact[relation='at' and argument[1]=8 and argument[2]=$row and argument[3]=$col]">  
      <xsl:call-template name="sheep">
        <xsl:with-param name="index" select="8"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="//fact[relation='at' and argument[1]=9 and argument[2]=$row and argument[3]=$col]">  
      <xsl:call-template name="sheep">
        <xsl:with-param name="index" select="9"/>
      </xsl:call-template>
    </xsl:when>
  </xsl:choose>
  </center>

  </td>  
  </xsl:template>
  
  <xsl:template   name="board_row">  
    <xsl:param name="cols" select="1"/>   <xsl:param name="rows" select="1"/>
    <xsl:param name="row"  select="1"/>  <xsl:param name="col" select="1"/>
    <xsl:call-template name="at">    
      <xsl:with-param name="row" select="$row"/>  
      <xsl:with-param name="col" select="$col"/>   
    </xsl:call-template>  
    <xsl:if test="$col &lt; $cols">
      <xsl:call-template name="board_row">     
        <xsl:with-param   name="cols" select="$cols"/>     
        <xsl:with-param name="rows"   select="$rows"/>  
        <xsl:with-param name="row"   select="$row"/> 
        <xsl:with-param name="col" select="$col + 1"/>  
      </xsl:call-template>  
    </xsl:if>
  </xsl:template>
  <xsl:template name="board_rows">  
    <xsl:param name="cols" select="1"/>  <xsl:param name="rows"   select="1"/>  
    <xsl:param name="row" select="1"/>  
    <tr>   
      <xsl:call-template name="board_row"> 
      <xsl:with-param   name="cols" select="$cols"/> 
      <xsl:with-param name="rows"   select="$rows"/>  
      <xsl:with-param name="row" select="$row"/>  
      </xsl:call-template>  
    </tr> 
    <xsl:if test="$row &lt; $rows"> 
      <xsl:call-template name="board_rows">  
        <xsl:with-param   name="cols" select="$cols"/>   
        <xsl:with-param name="rows"   select="$rows"/>  
        <xsl:with-param name="row" select="$row + 1"/>   
      </xsl:call-template>  
     </xsl:if>
  </xsl:template>
  <xsl:template name="board"> 
    <xsl:param name="cols" select="1"/>
    <xsl:param name="rows" select="1"/> 
    <table class="board"> 
    <xsl:call-template   name="board_rows">  
      <xsl:with-param name="cols"   select="$cols"/>  
      <xsl:with-param name="rows"   select="$rows"/> 
    </xsl:call-template> 
    </table>
  </xsl:template>
</xsl:stylesheet> 




