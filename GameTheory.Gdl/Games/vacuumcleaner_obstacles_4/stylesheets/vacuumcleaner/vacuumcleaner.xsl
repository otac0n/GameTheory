<?xml version="1.0" encoding="ISO-8859-1"?>

<!--
	for the vacuum cleaner worlds
	
	works with the following sees_xml rules:

(<= (sees_xml random ?t) (sees_xml1 ?t))
(<= (sees_xml ?p ?t) (role ?p) (distinct ?p random) (sees_xml1 ?t))

(<= (sees_xml1 (cell ?x ?y b))
    (xcoordinate ?x)
    (ycoordinate ?y)
    (not (true (at ?what ?x ?y)))
)
(<= (sees_xml1 (cell ?x ?y x))
    (true (at obstacle ?x ?y))
)
(<= (sees_xml1 (cell ?x ?y whitepawn))
    (true (at agent ?x ?y))
    (not (true (at dirt ?x ?y)))
)
(<= (sees_xml1 (cell ?x ?y blackpawn))
    (true (at dirt ?x ?y))
    (true (at agent ?x ?y))
)
(<= (sees_xml1 (cell ?x ?y brown_disc))
    (true (at dirt ?x ?y))
    (not (true (at agent ?x ?y)))
)
(<= (sees_xml1 stopped)
    (true stopped)
)
(<= (sees_xml1 (orientation ?x))
    (true (orientation ?x))
)
(<= (sees_xml1 (cell ?x ?y x))
    (true (at wall ?x ?y))
)
(<= (sees_xml1 (points ?x))
    (true (points ?x))
)
(<= (sees_xml1 (step ?x))
    (true (step ?x))
)
(<= (sees_xml1 (size ?x ?y))
    (size ?x ?y)
)
-->


<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:import href="../generic/template.xsl"/>
	<xsl:import href="../generic/chess_board.xsl"/>
	
	<xsl:template name="print_state">
		<xsl:variable name="width">
			<xsl:value-of select="fact[prop-f='SIZE']/arg[1]"/>
		</xsl:variable>
		<xsl:variable name="height">
			<xsl:value-of select="fact[prop-f='SIZE']/arg[2]"/>
		</xsl:variable>
		<xsl:call-template name="print_chess_state">
			<xsl:with-param name="Width" select="$width"/>
			<xsl:with-param name="Height" select="$height"/>
			<xsl:with-param name="checkered">alllight</xsl:with-param>
			<xsl:with-param name="CellWidth">24</xsl:with-param>
			<xsl:with-param name="CellHeight">24</xsl:with-param>
			<xsl:with-param name="BorderWidth">1</xsl:with-param>
		</xsl:call-template>
	</xsl:template>
	
</xsl:stylesheet>