<?xml version="1.0" encoding="ISO-8859-1"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:import href="../generic/template.xsl"/>
	<xsl:import href="../generic/state.xsl"/>
	<xsl:import href="../generic/sitespecific.xsl"/>
	
	<xsl:template name="print_state">
		<style type="text/css" media="all">
			div.board {
				position:relative;
				width: 140px;
				height: 260px;
				padding: 0px;
			}
			div.piece {
				position: absolute;
				padding:  0px;
			}
			div.occu {
				position: absolute;
				padding: 0px;
			}
		</style>
		<!-- Draw Board -->
		<div class="board">
			<img>
				<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL,'/stratego/board.png')"/></xsl:attribute>
			</img>
			
			<xsl:for-each select="fact[prop-f='OCCUPIED']">
				<xsl:variable name="x" select="./arg[1]"/>
				<xsl:variable name="y" select="arg[2]"/>
				<xsl:variable name="r" select="arg[3]"/>
				
		    	<div class="occu">
					<xsl:attribute name="style">
						left: <xsl:value-of select="-30+($x * 40)"/>px;
						top: <xsl:value-of select="-30+($y * 40)"/>px;
					</xsl:attribute>
					<img>
						<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/stratego/', translate($r, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.png')"/></xsl:attribute>
						<xsl:attribute name="alt"><xsl:value-of select="$r"/></xsl:attribute>
						<xsl:attribute name="title"><xsl:value-of select="$r"/></xsl:attribute>
					</img>
				</div>
			</xsl:for-each>
			
			<xsl:for-each select="fact[prop-f='CELL']">
				<xsl:variable name="x" select="./arg[1]"/>
				<xsl:variable name="y" select="arg[2]"/>
				<xsl:variable name="p" select="arg[3]"/>
				
		    	<div class="piece">
					<xsl:attribute name="style">
						left: <xsl:value-of select="-30+($x * 40)"/>px;
						top: <xsl:value-of select="-30+($y * 40)"/>px;
					</xsl:attribute>
					<img>
						<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/stratego/', translate($p, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.png')"/></xsl:attribute>
						<xsl:attribute name="alt"><xsl:value-of select="$p"/></xsl:attribute>
						<xsl:attribute name="title"><xsl:value-of select="$p"/></xsl:attribute>
					</img>
				</div>
			</xsl:for-each>
		</div>
		
		<!-- show remaining fluents -->
		<xsl:call-template name="state">
			<xsl:with-param name="excludeFluent" select="'CELL'"/>
			<xsl:with-param name="excludeFluent2" select="'OCCUPIED'"/>
		</xsl:call-template>
	</xsl:template>
</xsl:stylesheet>
