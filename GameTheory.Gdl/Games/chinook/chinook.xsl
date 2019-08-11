<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:template match="/">
<table width='400' height='400' cellspacing='0' cellpadding='0' border='1'>
<tr>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='a' and argument[2]='8']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='b' and argument[2]='8']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='c' and argument[2]='8']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='d' and argument[2]='8']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='e' and argument[2]='8']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='f' and argument[2]='8']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='g' and argument[2]='8']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='h' and argument[2]='8']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
</tr>
<tr>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='a' and argument[2]='7']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='b' and argument[2]='7']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='c' and argument[2]='7']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='d' and argument[2]='7']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='e' and argument[2]='7']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='f' and argument[2]='7']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='g' and argument[2]='7']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='h' and argument[2]='7']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
</tr>
<tr>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='a' and argument[2]='6']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='b' and argument[2]='6']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='c' and argument[2]='6']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='d' and argument[2]='6']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='e' and argument[2]='6']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='f' and argument[2]='6']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='g' and argument[2]='6']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='h' and argument[2]='6']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
</tr>
<tr>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='a' and argument[2]='5']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='b' and argument[2]='5']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='c' and argument[2]='5']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='d' and argument[2]='5']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='e' and argument[2]='5']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='f' and argument[2]='5']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='g' and argument[2]='5']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='h' and argument[2]='5']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
</tr>
<tr>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='a' and argument[2]='4']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='b' and argument[2]='4']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='c' and argument[2]='4']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='d' and argument[2]='4']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='e' and argument[2]='4']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='f' and argument[2]='4']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='g' and argument[2]='4']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='h' and argument[2]='4']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
</tr>
<tr>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='a' and argument[2]='3']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='b' and argument[2]='3']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='c' and argument[2]='3']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='d' and argument[2]='3']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='e' and argument[2]='3']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='f' and argument[2]='3']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='g' and argument[2]='3']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='h' and argument[2]='3']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
</tr>
<tr>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='a' and argument[2]='2']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='b' and argument[2]='2']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='c' and argument[2]='2']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='d' and argument[2]='2']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='e' and argument[2]='2']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='f' and argument[2]='2']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='g' and argument[2]='2']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='h' and argument[2]='2']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
</tr>
<tr>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='a' and argument[2]='1']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='b' and argument[2]='1']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='c' and argument[2]='1']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='d' and argument[2]='1']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='e' and argument[2]='1']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='f' and argument[2]='1']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#aaaaaa' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='oddcell' and argument[1]='g' and argument[2]='1']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
<td width='50' height='50' bgcolor='#cccccc' align='center' valign='center'>
  <xsl:for-each select="/state/fact[relation='evencell' and argument[1]='h' and argument[2]='1']">
    <xsl:call-template name="square"/>
  </xsl:for-each>
</td>
</tr>
</table>
</xsl:template>

<xsl:template name="square">
  <xsl:if test="argument[3]='white'"><img src="/ggp/games/chinook/wpawn.gif"/></xsl:if>
  <xsl:if test="argument[3]='black'"><img src="/ggp/games/chinook/bpawn.gif"/></xsl:if>
</xsl:template>

</xsl:stylesheet>
