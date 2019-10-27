<?xml version="1.0" encoding="ISO-8859-1"?>

<!--
	Bomberman, Mummymaze, Pacman, Ghostmaze, Wargame, etc.
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:import href="../generic/template.xsl"/>
	<xsl:import href="../generic/chess_board.xsl"/>
	<xsl:import href="../generic/state.xsl"/>
	<xsl:import href="../generic/sitespecific.xsl"/>

	<xsl:template name="print_state">
		
		<style type="text/css" media="all">
			div.maze {
				position: relative;
			}
		</style>
		
		<div class="maze">
			
			<!-- Draw board -->
			<xsl:call-template name="chess_board">
				<xsl:with-param name="checkered">alldark</xsl:with-param>
				<xsl:with-param name="CellFluentName">CELL</xsl:with-param>
				<xsl:with-param name="contentArgIdx">3</xsl:with-param>
				<xsl:with-param name="xArgIdx">1</xsl:with-param>
				<xsl:with-param name="yArgIdx">2</xsl:with-param>
				<xsl:with-param name="CellWidth">44</xsl:with-param>
				<xsl:with-param name="CellHeight">44</xsl:with-param>
				<xsl:with-param name="DefaultCellContent">no</xsl:with-param>
				<xsl:with-param name="DefaultCell">no</xsl:with-param>
			</xsl:call-template>

			<xsl:call-template name="chess_board">
				<xsl:with-param name="checkered">alldark</xsl:with-param>
				<xsl:with-param name="Width">-8</xsl:with-param>
				<xsl:with-param name="Height">4</xsl:with-param>
				<xsl:with-param name="CellFluentName">AT</xsl:with-param>
				<xsl:with-param name="contentArgIdx">1</xsl:with-param>
				<xsl:with-param name="xArgIdx">2</xsl:with-param>
				<xsl:with-param name="yArgIdx">3</xsl:with-param>
				<xsl:with-param name="CellWidth">44</xsl:with-param>
				<xsl:with-param name="CellHeight">44</xsl:with-param>
				<xsl:with-param name="DefaultCellContent">no</xsl:with-param>
				<xsl:with-param name="DefaultCell">no</xsl:with-param>
			</xsl:call-template>
		</div>
		
		<!-- show remaining fluents -->
		<xsl:call-template name="state">
			<xsl:with-param name="excludeFluent">CELL</xsl:with-param>
			<xsl:with-param name="excludeFluent2">AT</xsl:with-param>
		</xsl:call-template>
		
	</xsl:template>


	<xsl:template name="make_cell">
		<xsl:param name="col"/>
		<xsl:param name="row"/>
		<xsl:param name="defaultClass"/>
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
				<xsl:when test="$content='REDBRUSH'">redbrush</xsl:when>
				<xsl:when test="$content='GREENBRUSH'">greenbrush</xsl:when>
				<xsl:when test="$content='BLUEBRUSH'">bluebrush</xsl:when>
				<xsl:when test="$content='YELLOWBRUSH'">yellowbrush</xsl:when>
				<xsl:when test="$content='RED'">red</xsl:when>
				<xsl:when test="$content='GREEN'">green</xsl:when>
				<xsl:when test="$content='BLUE'">blue</xsl:when>
				<xsl:when test="$content='YELLOW'">yellow</xsl:when>
				<xsl:when test="$content='B'">blank</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<img>
			<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/battlebrushes/', $imgName, '.png')"/></xsl:attribute>
			<xsl:attribute name="alt"><xsl:value-of select="$alt"/></xsl:attribute>
			<xsl:attribute name="title"><xsl:value-of select="$alt"/></xsl:attribute>
		</img>
		
	</xsl:template>

</xsl:stylesheet>
