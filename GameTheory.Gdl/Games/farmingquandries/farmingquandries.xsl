<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:param name="width" select="300"/>
  <xsl:param name="height" select="300"/>

  <xsl:template name="main" match="/">
    <div> <!-- Set Style -->
    <style type="text/css" media="all"> 
    td.season { text-align: center } font.season { font-size: <xsl:value-of select="$width*0.10"/>px; } font.red_tile { font-size: <xsl:value-of select="$width * 0.13"/>px; color: red } font.blue_tile { font-size: <xsl:value-of select="$width * 0.13"/>px; color: blue } td.cell {        width:  <xsl:value-of select="$width * 0.17"/>px; height: <xsl:value-of select="$height * 0.17"/>px;        border: 2px solid #000;        background-color: #B6AFA9;    text-align: center;    align: center;  valign: middle;    }      
    table.board {background-color: #000000;}
    </style>
    <!-- Draw Board --> 
    <xsl:call-template name="board">      
      <xsl:with-param name="cols" select="4"/>   
      <xsl:with-param name="rows" select="4"/>   
    </xsl:call-template>     
    <xsl:call-template name='season'/>
    </div>   
  </xsl:template>

  <xsl:template name="board"> 
    <xsl:param name="cols" select="1"/>
    <xsl:param name="rows" select="1"/>
    <table class="board">
    <xsl:call-template name="board_rows">
      <xsl:with-param name="cols" select="$cols"/>
      <xsl:with-param name="rows" select="$rows"/>
    </xsl:call-template>
    </table>
  </xsl:template>

  <xsl:template name="board_rows">  
    <xsl:param name="cols" select="1"/>
    <xsl:param name="rows" select="1"/>  
    <xsl:param name="row" select="1"/>  
    <tr>   
      <xsl:call-template name="board_row"> 
        <xsl:with-param name="cols" select="$cols"/> 
        <xsl:with-param name="rows" select="$rows"/>  
        <xsl:with-param name="row" select="$row"/>  
      </xsl:call-template>  
    </tr> 
    <xsl:if test="$row &lt; $rows"> 
      <xsl:call-template name="board_rows">  
        <xsl:with-param name="cols" select="$cols"/>   
        <xsl:with-param name="rows" select="$rows"/>  
        <xsl:with-param name="row" select="$row + 1"/>   
      </xsl:call-template>  
     </xsl:if>
  </xsl:template>

  <xsl:template name="board_row">  
    <xsl:param name="cols" select="1"/>   <xsl:param name="rows" select="1"/>
    <xsl:param name="row"  select="1"/>  <xsl:param name="col" select="1"/>
    <xsl:call-template name="cell">    
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

  <xsl:template name="cell" match="state/fact"> 
    <xsl:param name="row" select="1"/>
    <xsl:param name="col" select="1"/> 
    <td class="cell">  
    <xsl:attribute name="id">  
      <xsl:value-of select="'at_'"/>   
      <xsl:value-of select="$row"/>
      <xsl:value-of select="$col"/>   
    </xsl:attribute>    
  
  <xsl:choose>   
    <xsl:when test="//fact[relation='ripe' and argument[2]=$row and argument[3] = $col]"> 
      <xsl:attribute name="style">background-color: #663366</xsl:attribute> 
    </xsl:when>   
    <xsl:when test="//fact[relation='plowed' and argument[2]=$row and argument[3] = $col]"> 
      <xsl:attribute name="style">background-color: #CC9966</xsl:attribute> 
    </xsl:when>   
    <xsl:when test="//fact[relation='sown' and argument[2]=$row and argument[3] = $col]"> 
      <xsl:attribute name="style">background-color: #99CC33</xsl:attribute> 
    </xsl:when>   
  </xsl:choose>  

  <xsl:choose>   
    <xsl:when test="//fact[relation='plowed' and argument[1]='red' and argument[2]=$row and argument[3] = $col]">
      <font class="red_tile">O</font>
    </xsl:when>   
    <xsl:when test="//fact[relation='sown' and argument[1]='red' and argument[2]=$row and argument[3] = $col]">
      <font class="red_tile">O</font>
    </xsl:when>   
    <xsl:when test="//fact[relation='ripe' and argument[1]='red' and argument[2]=$row and argument[3] = $col]">
      <font class="red_tile">O</font>
    </xsl:when>   
    <xsl:when test="//fact[relation='plowed' and argument[1]='blue' and argument[2]=$row and argument[3] = $col]">
      <font class="blue_tile">O</font>
    </xsl:when>   
    <xsl:when test="//fact[relation='sown' and argument[1]='blue' and argument[2]=$row and argument[3] = $col]"> 
      <font class="blue_tile">O</font>
    </xsl:when>   
    <xsl:when test="//fact[relation='ripe' and argument[1]='blue' and argument[2]=$row and argument[3] = $col]"> 
      <font class="blue_tile">O</font>
    </xsl:when>   
  </xsl:choose>

    </td>  
  </xsl:template>
  
  <xsl:template name="season" match="state/fact">
    <xsl:for-each select="/match/herstory/state/fact[relation='step']">
      <xsl:value-of select="argument[1]"/><br/>
    </xsl:for-each>
    <table> <tr> <td class="season">
      <xsl:choose>
        <xsl:when test="//fact[relation='season' and argument[1]='spring']"> 
          <xsl:attribute name="style">background-color: #83F52C</xsl:attribute> 
          <font class="season">Springtime</font>
        </xsl:when>   
        <xsl:when test="//fact[relation='season' and argument[1]='summer']"> 
          <xsl:attribute name="style">background-color: #324F17</xsl:attribute> 
          <font class="season">Summer</font>
        </xsl:when>   
        <xsl:when test="//fact[relation='season' and argument[1]='fall']"> 
          <xsl:attribute name="style">background-color: #FF6600</xsl:attribute> 
          <font class="season">Fall</font>
        </xsl:when>   
        <xsl:when test="//fact[relation='season' and argument[1]='winter']"> 
          <xsl:attribute name="style">background-color: #EEE5DE</xsl:attribute> 
          <font class="season">Winter</font>
        </xsl:when>   
      </xsl:choose>  
    </td> </tr> </table>
  </xsl:template>

</xsl:stylesheet> 








