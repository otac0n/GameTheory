<?xml version="1.0" encoding="ISO-8859-1"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:import href="../generic/template.xsl"/>
	<xsl:import href="../generic/state.xsl"/>
	<xsl:import href="../generic/sitespecific.xsl"/>

	<xsl:template name="print_state">
		<style type="text/css" media="all">
			div.street {
				position:relative;
				width: 800px;
				height: 500px;
				padding: 0px;
			}
			div.name1 {
				position: absolute;
				padding: 0px;
			}
			div.name2 {
				position: absolute;
				padding: 0px;
			}
			div.fighter {
				position: absolute;
				padding:  0px;
			}
			div.lifebar {
				position: absolute;
				padding:  0px;
			}
			div.energybar {
				position: absolute;
				padding:  0px;
			}
			div.clock {
				position: absolute;
				padding: 0px;
			}
		</style>
		<!-- Draw Board -->
		<div class="street">
			<img>
				<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL,'/streetfight/street.png')"/></xsl:attribute>
			</img>
			
			<div class="name1">
				<xsl:attribute name="style">
					left: 10px;
					top: 35px;
				</xsl:attribute>
				<img>
					<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/streetfight/ryutext.gif')"/></xsl:attribute>
					<xsl:attribute name="alt">ryu</xsl:attribute>
					<xsl:attribute name="title">ryu</xsl:attribute>
				</img>
			</div>
			<div class="name2">
				<xsl:attribute name="style">
					left: 690px;
					top: 35px;
				</xsl:attribute>
				<img>
					<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/streetfight/kentext.gif')"/></xsl:attribute>
					<xsl:attribute name="alt">ken</xsl:attribute>
					<xsl:attribute name="title">ken</xsl:attribute>
				</img>
			</div>
			
			<xsl:for-each select="fact[prop-f='LIFE']">
				<xsl:variable name="owner" select="./arg[1]"/>
				<xsl:variable name="value_help" select="arg[2]"/>
				
				<xsl:variable name="value">
					<xsl:choose>
						<xsl:when test="$value_help='10'">10</xsl:when>
						<xsl:otherwise>0<xsl:value-of select="$value_help"/></xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				
				<xsl:variable name="alt">(LIFE<xsl:text> </xsl:text><xsl:value-of select="$owner"/><xsl:text> </xsl:text><xsl:value-of select="$value"/>)</xsl:variable>
				
				<xsl:variable name="posx">
					<xsl:choose>
						<xsl:when test="$owner='RYU'">20</xsl:when>
						<xsl:otherwise>480</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
		    	
		    	<div class="lifebar">
					<xsl:attribute name="style">
						left: <xsl:value-of select="$posx"/>px;
						top: 20px;
					</xsl:attribute>
					<img>
						<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/streetfight/', translate($owner, 'RYUKEN', 'ryuken'), '_life_', $value, '.gif')"/></xsl:attribute>
						<xsl:attribute name="alt"><xsl:value-of select="$alt"/></xsl:attribute>
						<xsl:attribute name="title"><xsl:value-of select="$alt"/></xsl:attribute>
					</img>
				</div>
			</xsl:for-each>
			
			<xsl:for-each select="fact[prop-f='ENERGY']">
				<xsl:variable name="eowner" select="./arg[1]"/>
				<xsl:variable name="evalue" select="arg[2]"/>
				
				<xsl:variable name="ealt">(ENERGY<xsl:text> </xsl:text><xsl:value-of select="$eowner"/><xsl:text> </xsl:text><xsl:value-of select="$evalue"/>)</xsl:variable>
				
				<xsl:variable name="eposx">
					<xsl:choose>
						<xsl:when test="$eowner='RYU'">110</xsl:when>
						<xsl:otherwise>490</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
		    	
		    	<div class="energybar">
					<xsl:attribute name="style">
						left: <xsl:value-of select="$eposx"/>px;
						top: 50px;
					</xsl:attribute>
					<img>
						<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/streetfight/', translate($eowner, 'RYUKEN', 'ryuken'), '_energy_', $evalue, '.gif')"/></xsl:attribute>
						<xsl:attribute name="alt"><xsl:value-of select="$ealt"/></xsl:attribute>
						<xsl:attribute name="title"><xsl:value-of select="$ealt"/></xsl:attribute>
					</img>
				</div>
			</xsl:for-each>
			
			<xsl:for-each select="fact[prop-f='STEP']">
				<xsl:variable name="value" select="./arg[1]"/>
				
				<xsl:variable name="ealt">(STEP<xsl:text> </xsl:text><xsl:value-of select="$value"/>)</xsl:variable>
		    	
		    	<div class="clock">
					<xsl:attribute name="style">
						left: 355px;
						top: 20px;
					</xsl:attribute>
					<img>
						<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/streetfight/clock_', $value, '.gif')"/></xsl:attribute>
						<xsl:attribute name="alt"><xsl:value-of select="$ealt"/></xsl:attribute>
						<xsl:attribute name="title"><xsl:value-of select="$ealt"/></xsl:attribute>
					</img>
				</div>
			</xsl:for-each>
			
			<xsl:for-each select="fact[prop-f='COMBINEDACTION']">
				<xsl:variable name="owner" select="./arg[1]"/>
				<xsl:variable name="position" select="arg[2]"/>
				<xsl:variable name="move" select="arg[3]"/>
				<xsl:variable name="result" select="arg[4]"/>
				
				<xsl:variable name="posx">
					<xsl:choose>
						<xsl:when test="$position='1'">30</xsl:when>
						<xsl:when test="$position='2'">140</xsl:when>
						<xsl:when test="$position='3'">250</xsl:when>
						<xsl:when test="$position='4'">360</xsl:when>
						<xsl:when test="$position='5'">470</xsl:when>
						<xsl:otherwise>580</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
		    	
		    	<div class="fighter">
					<xsl:attribute name="style">
						left: <xsl:value-of select="$posx"/>px;
						top: 200px;
					</xsl:attribute>
					<img>
						<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/streetfight/', translate($owner, 'RYUKEN', 'ryuken'), '_', translate($move, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '_', translate($result, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.gif')"/></xsl:attribute>
						<xsl:attribute name="alt"><xsl:value-of select="$owner"/></xsl:attribute>
						<xsl:attribute name="title"><xsl:value-of select="$owner"/></xsl:attribute>
					</img>
				</div>
			</xsl:for-each>
			
		</div>
		
		<!-- show remaining fluents -->
		<xsl:call-template name="state">
			<xsl:with-param name="excludeFluent" select="'LIFE'"/>
			<xsl:with-param name="excludeFluent2" select="'ENERGY'"/>
			<xsl:with-param name="excludeFluent3" select="'STEP'"/>
			<!--<xsl:with-param name="excludeFluent4" select="'AT'"/>
			<xsl:with-param name="excludeFluent5" select="'POS'"/>-->
		</xsl:call-template>
	</xsl:template>
</xsl:stylesheet>
