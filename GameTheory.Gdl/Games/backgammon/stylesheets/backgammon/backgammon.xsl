<?xml version="1.0" encoding="ISO-8859-1"?>

<!--
	works for Backgammon
	
	pictures are modified screenshots from GNU Backgammon (http://gnubg.org/)
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:import href="../generic/template.xsl"/>
	<xsl:import href="../generic/state.xsl"/>
	<xsl:import href="../generic/dice.xsl"/>
	
	<xsl:template name="print_state">
		<xsl:call-template name="print_backgammon_board"/>
		
		<xsl:call-template name="state">
			<xsl:with-param name="excludeFluent" select="'BOARD_POINT'"/>
			<xsl:with-param name="excludeFluent2" select="'DIE'"/>
		</xsl:call-template>
	</xsl:template>
	
	<xsl:template name="print_backgammon_board">
		<style type="text/css" media="all">
			div.backgammonboard
			{
				position:   relative;
				width:      542px;
				height:     412px;
				padding:    0px;
				border:     0px;
				background: transparent url(
				<xsl:value-of select="$stylesheetURL"/>/backgammon/board.png
				) repeat top left;
			}
			div.backgammon_piece_caption{
				height: 30px;
				width: 30px;
				line-height: 30px;
				vertical-align: middle;
				text-align: center;
				font-weight: bold;
				font-size: 20px;
				color: #FFF5EE;
				}
			div.backgammonboard div.die{
				position: absolute;
				height: 56px;
				width: 56px;
			}
			#die1 {
				top: 170px;
				left: 80px;
			}
			#die2 {
				top: 185px;
				left: 150px;
			}
		</style>

		<xsl:variable name="up" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>
		<xsl:variable name="lo" select="'abcdefghijklmnopqrstuvwxyz'"/>
		<div class="backgammonboard">
			<xsl:for-each select="fact[prop-f='BOARD_POINT']">
				<xsl:variable name="alt"><xsl:call-template name="fluent2text"/></xsl:variable>
				<xsl:variable name="pos" select="arg[1]"/>
				<xsl:variable name="number" select="arg[2]"/>
				<xsl:variable name="color" select="translate(arg[3],$up,$lo)"/>
				<xsl:if test="$number>=1">
					<xsl:variable name="xpos">
						<xsl:choose>
							<xsl:when test="$pos='BAR'"><xsl:value-of select="271-15"/></xsl:when>
							<xsl:when test="$pos='OUT'"><xsl:value-of select="496"/></xsl:when>
							<xsl:when test="number($pos)>=1 and number($pos)&lt;=6"><xsl:value-of select="302+30*(6-number($pos))"/></xsl:when>
							<xsl:when test="number($pos)>=7 and number($pos)&lt;=12"><xsl:value-of select="61+30*(12-number($pos))"/></xsl:when>
							<xsl:when test="number($pos)>=13 and number($pos)&lt;=18"><xsl:value-of select="61+30*(number($pos)-13)"/></xsl:when>
							<xsl:when test="number($pos)>=19 and number($pos)&lt;=24"><xsl:value-of select="302+30*(number($pos)-19)"/></xsl:when>
							<xsl:otherwise>0</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
					<xsl:variable name="ypos_base">
						<xsl:choose>
							<xsl:when test="$pos='BAR' and $color='red'"><xsl:value-of select="366-5*30"/></xsl:when>
							<xsl:when test="$pos='BAR' and $color='black'"><xsl:value-of select="16+5*30"/></xsl:when>
							<xsl:when test="($pos='OUT' and $color='red') or number($pos)&lt;=12"><xsl:value-of select="16"/></xsl:when>
							<xsl:when test="($pos='OUT' and $color='black') or number($pos)>=13"><xsl:value-of select="366"/></xsl:when>
							<xsl:otherwise>0</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
					<xsl:variable name="ypos_offset_direction">
						<xsl:choose>
							<xsl:when test="$pos='BAR' and $color='red'"><xsl:value-of select="1"/></xsl:when>
							<xsl:when test="$pos='BAR' and $color='black'"><xsl:value-of select="-1"/></xsl:when>
							<xsl:when test="($pos='OUT' and $color='red') or number($pos)&lt;=12"><xsl:value-of select="1"/></xsl:when>
							<xsl:when test="($pos='OUT' and $color='black') or number($pos)>=13"><xsl:value-of select="-1"/></xsl:when>
							<xsl:otherwise>0</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
					<xsl:if test="$number>=1">
						<xsl:call-template name="print_backgammon_piece">
							<xsl:with-param name="color" select="$color"/>
							<xsl:with-param name="xpos" select="$xpos"/>
							<xsl:with-param name="ypos" select="$ypos_base"/>
							<xsl:with-param name="alt" select="$alt"/>
						</xsl:call-template>
					</xsl:if>
 					<xsl:if test="$number>=2">
						<xsl:call-template name="print_backgammon_piece">
							<xsl:with-param name="color" select="$color"/>
							<xsl:with-param name="xpos" select="$xpos"/>
							<xsl:with-param name="ypos" select="$ypos_base+$ypos_offset_direction*30*1"/>
							<xsl:with-param name="alt" select="$alt"/>
						</xsl:call-template>
					</xsl:if>
					<xsl:if test="$number>=3">
						<xsl:call-template name="print_backgammon_piece">
							<xsl:with-param name="color" select="$color"/>
							<xsl:with-param name="xpos" select="$xpos"/>
							<xsl:with-param name="ypos" select="$ypos_base+$ypos_offset_direction*30*2"/>
							<xsl:with-param name="alt" select="$alt"/>
						</xsl:call-template>
					</xsl:if>
					<xsl:if test="$number>=4">
						<xsl:call-template name="print_backgammon_piece">
							<xsl:with-param name="color" select="$color"/>
							<xsl:with-param name="xpos" select="$xpos"/>
							<xsl:with-param name="ypos" select="$ypos_base+$ypos_offset_direction*30*3"/>
							<xsl:with-param name="alt" select="$alt"/>
						</xsl:call-template>
					</xsl:if>
					<xsl:if test="$number>=5">
						<xsl:call-template name="print_backgammon_piece">
							<xsl:with-param name="color" select="$color"/>
							<xsl:with-param name="xpos" select="$xpos"/>
							<xsl:with-param name="ypos" select="$ypos_base+$ypos_offset_direction*30*4"/>
							<xsl:with-param name="alt" select="$alt"/>
						</xsl:call-template>
					</xsl:if>
					<xsl:if test="$number>5">
						<div class="backgammon_piece_caption">
							<xsl:attribute name="style">
								position: absolute;
								left: <xsl:value-of select="$xpos"/>px;
								top: <xsl:value-of select="$ypos_base+$ypos_offset_direction*30*4"/>px;
							</xsl:attribute>
							<xsl:value-of select="$number"/>
						</div>
					</xsl:if>
				</xsl:if>
			</xsl:for-each>
 			<xsl:variable name="dieColor" select="translate(fact[prop-f='CONTROL']/arg[1],$up,$lo)"/>
			<xsl:variable name="dieNb1" select="number(fact[prop-f='DIE' and (arg[1]='1' or arg[1]='3' or arg[1]='4')]/arg[1])"/>
			<xsl:variable name="dieNb2" select="number(fact[prop-f='DIE' and $dieNb1!=number(arg[1])]/arg[1])"/>
			<xsl:variable name="pips1" select="fact[prop-f='DIE' and $dieNb1=number(arg[1])]/arg[2]"/>
			<xsl:variable name="pips2" select="fact[prop-f='DIE' and $dieNb2=number(arg[1])]/arg[2]"/> 
			<xsl:if test="$pips1 != ''">
				<div class="die" id="die1">
					<xsl:call-template name="print_dice">
						<xsl:with-param name="color" select="$dieColor"/>
						<xsl:with-param name="value" select="number($pips1)"/>
					</xsl:call-template>
				</div>
			</xsl:if>
			<xsl:if test="$pips2 != ''">
				<div class="die" id="die2">
					<xsl:call-template name="print_dice">
						<xsl:with-param name="color" select="$dieColor"/>
						<xsl:with-param name="value" select="number($pips2)"/>
					</xsl:call-template>
				</div>
			</xsl:if>
		</div>
	</xsl:template>
	
	<xsl:template name="print_backgammon_piece">
		<xsl:param name="color"/>				
		<xsl:param name="xpos"/>
		<xsl:param name="ypos"/>
		<xsl:param name="alt"/>
		<div>
			<xsl:attribute name="style">
				position: absolute;
				left: <xsl:value-of select="$xpos"/>px;
				top: <xsl:value-of select="$ypos"/>px;
				width: 30px;
				height: 30px;
			</xsl:attribute>
			<img>
				<xsl:attribute name="src"><xsl:value-of select="$stylesheetURL"/>backgammon/<xsl:value-of select="$color"/>.png</xsl:attribute>
				<xsl:attribute name="title"><xsl:value-of select="$alt"/></xsl:attribute>
				<xsl:attribute name="alt"><xsl:value-of select="$alt"/></xsl:attribute>
				<xsl:attribute name="width">30</xsl:attribute>
				<xsl:attribute name="height">30</xsl:attribute>
			</img>
		</div>
	</xsl:template>
</xsl:stylesheet>
