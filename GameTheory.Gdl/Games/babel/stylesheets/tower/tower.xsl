<?xml version="1.0" encoding="ISO-8859-1"?>

<!-- for game "babel" -->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:import href="../generic/template.xsl"/>
	<xsl:import href="../generic/state.xsl"/>
	<xsl:import href="../generic/sitespecific.xsl"/>

	<xsl:template name="print_state">
		<style type="text/css" media="all">
			div.world {
				position:relative;
				width: 900px;
				height: 500px;
				padding: 0px;
			}
			div.tile {
				position: absolute;
				padding:  0px;
			}
		</style>
		<!-- Draw Board -->
		<div class="world" style="background: lightgray">
		
			<img>
				<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL,'/tower/background.png')"/></xsl:attribute>
			</img>
		
			<xsl:for-each select="fact[prop-f='BUILT']">
				<xsl:variable name="lvl" select="arg[1]"/>
				<xsl:variable name="value" select="arg[2]"/>
				
				<xsl:variable name="alt">(BUILT<xsl:text> </xsl:text><xsl:value-of select="$lvl"/><xsl:text> </xsl:text><xsl:value-of select="$value"/>)</xsl:variable>
				
				<xsl:variable name="posy" select="25 + (($lvl - 1) * 50)"/>
				<xsl:variable name="posx" select="25 + (($lvl - 1) * 25)"/>
				
				<div class="tile">
					<xsl:attribute name="style">
						left: <xsl:value-of select="$posx"/>px;
						bottom: <xsl:value-of select="$posy"/>px;
					</xsl:attribute>
					<img>
						<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/tower/tile_', $value, '.png')"/></xsl:attribute>
						<xsl:attribute name="alt"><xsl:value-of select="$alt"/></xsl:attribute>
						<xsl:attribute name="title"><xsl:value-of select="$alt"/></xsl:attribute>
					</img>
				</div>
		    	
			</xsl:for-each>
		</div>
		
		<!-- show remaining fluents -->
		<xsl:call-template name="state">
			<xsl:with-param name="excludeFluent" select="'BUILT'"/>
		</xsl:call-template>
	</xsl:template>
</xsl:stylesheet>
