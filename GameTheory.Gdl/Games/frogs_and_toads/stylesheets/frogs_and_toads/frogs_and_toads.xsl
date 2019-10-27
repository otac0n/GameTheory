<?xml version="1.0" encoding="ISO-8859-1"?>

<!--
	similar to mazegames but uses frogs and toads as pieces and floor as empty cells
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:import href="../generic/template.xsl"/>
	<xsl:import href="../generic/chess_board.xsl"/>
	
	<xsl:template name="print_state">
		<xsl:call-template name="print_chess_state">
			<xsl:with-param name="checkered" select="'invisible'"/>
			<xsl:with-param name="BorderWidth" select="0"/>
			<xsl:with-param name="CellWidth" select="36"/>
			<xsl:with-param name="CellHeight" select="36"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="make_cell_content">
		<xsl:param name="xArg"/>
		<xsl:param name="yArg"/>
		<xsl:param name="content"/>
		<xsl:param name="piece"/>
		<xsl:param name="background"/>
		<xsl:param name="alt"/>

		<xsl:variable name="imgName">
			<xsl:choose>
				<xsl:when test="$content='FROG'">frog</xsl:when>
				<xsl:when test="$content='TOAD'">toad</xsl:when>
				<xsl:when test="$content='FLOOR'">floor</xsl:when>
				<xsl:otherwise><xsl:value-of select="$piece"/></xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<img>
			<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/frogs_and_toads/', $imgName, '.png')"/></xsl:attribute>
			<xsl:attribute name="alt"><xsl:value-of select="$alt"/></xsl:attribute>
			<xsl:attribute name="title"><xsl:value-of select="$alt"/></xsl:attribute>
		</img>
	</xsl:template>
	
</xsl:stylesheet>
