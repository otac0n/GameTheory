<?xml version="1.0" encoding="ISO-8859-1"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:import href="../generic/template.xsl"/>
	<xsl:import href="../generic/state.xsl"/>
	<xsl:import href="../generic/sitespecific.xsl"/>
	
	<xsl:template name="print_state">
		<style type="text/css" media="all">
			div.map {
				position:relative;
				width: 716px;
				height: 702px;
				padding: 0px;
			}
			div.truck {
				position: absolute;
				padding:  0px;
			}
			div.pos {
				position: absolute;
				padding: 0px;
			}
			div.destination {
				position: absolute;
				padding: 0px;
			}
			div.debug {
				position: absolute;
				padding: 0px;
			}
		</style>
		<!-- Draw Board -->
		<div class="map">
			<img>
				<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL,'/logistics/map_roads.png')"/></xsl:attribute>
			</img>
			
			<xsl:for-each select="fact[prop-f='MOVE']">
				<xsl:variable name="owner" select="./arg[1]"/>
				<xsl:variable name="value" select="arg[2]"/>
				
				<xsl:variable name="posy">
					<xsl:choose>
						<xsl:when test="$owner='TRUCK1'">20</xsl:when>
						<xsl:otherwise>140</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				
		    	<div class="truck">
					<xsl:attribute name="style">
						left: 20px;
						top: <xsl:value-of select="$posy"/>px;
					</xsl:attribute>
					<img>
						<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/logistics/', translate($owner, 'TRUCK12', 'truck12'), '_', translate($value, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.gif')"/></xsl:attribute>
						<xsl:attribute name="alt"><xsl:value-of select="$owner"/></xsl:attribute>
						<xsl:attribute name="title"><xsl:value-of select="$owner"/></xsl:attribute>
					</img>
				</div>
			</xsl:for-each>
			
			<xsl:for-each select="fact[prop-f='AT']">
				<xsl:variable name="owner" select="./arg[1]"/>
				<xsl:variable name="value" select="arg[2]"/>
				
				<xsl:variable name="posx">
					<xsl:choose>
						<xsl:when test="$value='MADRID'">92</xsl:when>
						<xsl:when test="$value='BARCELONA'">147</xsl:when>
						<xsl:when test="$value='CADIZ'">62</xsl:when>
						<xsl:when test="$value='LISSABON'">36</xsl:when>
						<xsl:when test="$value='BORDEAUX'">139</xsl:when>
						<xsl:when test="$value='DIJON'">192</xsl:when>
						<xsl:when test="$value='PARIS'">175</xsl:when>
						<xsl:when test="$value='LYON'">205</xsl:when>
						<xsl:when test="$value='VERONA'">261</xsl:when>
						<xsl:when test="$value='ROM'">288</xsl:when>
						<xsl:when test="$value='NAPOLI'">309</xsl:when>
						<xsl:when test="$value='WIEN'">313</xsl:when>
						<xsl:when test="$value='LUXEMBURG'">215</xsl:when>
						<xsl:when test="$value='AMSTERDAM'">200</xsl:when>
						<xsl:when test="$value='MUENCHEN'">274</xsl:when>
						<xsl:when test="$value='HAMBURG'">252</xsl:when>
						<xsl:when test="$value='BERLIN'">291</xsl:when>
						<xsl:when test="$value='WARSCHAU'">365</xsl:when>
						<xsl:when test="$value='PRAG'">304</xsl:when>
						<xsl:when test="$value='BUDAPEST'">349</xsl:when>
						<xsl:when test="$value='MINSK'">429</xsl:when>
						<xsl:when test="$value='BELGRAD'">370</xsl:when>
						<xsl:when test="$value='BUKAREST'">438</xsl:when>
						<xsl:when test="$value='ISTANBUL'">461</xsl:when>
						<xsl:when test="$value='ATHEN'">405</xsl:when>
						<xsl:when test="$value='ANKARA'">516</xsl:when>
						<xsl:when test="$value='KIEW'">471</xsl:when>
						<xsl:when test="$value='MOSKAU'">561</xsl:when>
						<xsl:when test="$value='PETERSBURG'">480</xsl:when>
						<xsl:when test="$value='HELSINKI'">409</xsl:when>
						
						<xsl:when test="$value='TRUCK1'">85</xsl:when>
						<xsl:when test="$value='TRUCK2'">85</xsl:when>
						
						<xsl:otherwise>0</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				
				<xsl:variable name="posy">
					<xsl:choose>
						<xsl:when test="$value='MADRID'">607</xsl:when>
						<xsl:when test="$value='BARCELONA'">594</xsl:when>
						<xsl:when test="$value='CADIZ'">650</xsl:when>
						<xsl:when test="$value='LISSABON'">619</xsl:when>
						<xsl:when test="$value='BORDEAUX'">528</xsl:when>
						<xsl:when test="$value='DIJON'">513</xsl:when>
						<xsl:when test="$value='PARIS'">462</xsl:when>
						<xsl:when test="$value='LYON'">483</xsl:when>
						<xsl:when test="$value='VERONA'">528</xsl:when>
						<xsl:when test="$value='ROM'">578</xsl:when>
						<xsl:when test="$value='NAPOLI'">597</xsl:when>
						<xsl:when test="$value='WIEN'">490</xsl:when>
						<xsl:when test="$value='LUXEMBURG'">463</xsl:when>
						<xsl:when test="$value='AMSTERDAM'">396</xsl:when>
						<xsl:when test="$value='MUENCHEN'">492</xsl:when>
						<xsl:when test="$value='HAMBURG'">381</xsl:when>
						<xsl:when test="$value='BERLIN'">410</xsl:when>
						<xsl:when test="$value='WARSCHAU'">421</xsl:when>
						<xsl:when test="$value='PRAG'">447</xsl:when>
						<xsl:when test="$value='BUDAPEST'">496</xsl:when>
						<xsl:when test="$value='MINSK'">395</xsl:when>
						<xsl:when test="$value='BELGRAD'">531</xsl:when>
						<xsl:when test="$value='BUKAREST'">541</xsl:when>
						<xsl:when test="$value='ISTANBUL'">589</xsl:when>
						<xsl:when test="$value='ATHEN'">639</xsl:when>
						<xsl:when test="$value='ANKARA'">608</xsl:when>
						<xsl:when test="$value='KIEW'">442</xsl:when>
						<xsl:when test="$value='MOSKAU'">331</xsl:when>
						<xsl:when test="$value='PETERSBURG'">249</xsl:when>
						<xsl:when test="$value='HELSINKI'">241</xsl:when>
						
						<xsl:when test="$value='TRUCK1'">55</xsl:when>
						<xsl:when test="$value='TRUCK2'">185</xsl:when>
						
						<xsl:otherwise>0</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				
		    	<div class="pos">
					<xsl:attribute name="style">
						left: <xsl:value-of select="$posx - 20"/>px;
						top: <xsl:value-of select="$posy - 15"/>px;
					</xsl:attribute>
					<img>
						<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/logistics/', translate($owner, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '.png')"/></xsl:attribute>
						<xsl:attribute name="alt"><xsl:value-of select="$owner"/> at <xsl:value-of select="$value"/> (<xsl:value-of select="$posx"/>, <xsl:value-of select="$posy"/>)</xsl:attribute>
						<xsl:attribute name="title"><xsl:value-of select="$owner"/> at <xsl:value-of select="$value"/> (<xsl:value-of select="$posx"/>, <xsl:value-of select="$posy"/>)</xsl:attribute>
					</img>
				</div>
				
				<!--<div class="debug">
					<xsl:attribute name="style">
						left: <xsl:value-of select="$posx"/>px;
						top: 150px;
					</xsl:attribute>
					<xsl:value-of select="$posx"/><br/>
					<xsl:value-of select="$posy"/>
				</div>-->
			</xsl:for-each>
			
			<xsl:for-each select="fact[prop-f='DESTINATION']">
				<xsl:variable name="owner" select="./arg[1]"/>
				<xsl:variable name="value" select="arg[2]"/>
				
				<xsl:variable name="posx">
					<xsl:choose>
						<xsl:when test="$value='MADRID'">92</xsl:when>
						<xsl:when test="$value='BARCELONA'">147</xsl:when>
						<xsl:when test="$value='CADIZ'">62</xsl:when>
						<xsl:when test="$value='LISSABON'">36</xsl:when>
						<xsl:when test="$value='BORDEAUX'">139</xsl:when>
						<xsl:when test="$value='DIJON'">192</xsl:when>
						<xsl:when test="$value='PARIS'">175</xsl:when>
						<xsl:when test="$value='LYON'">205</xsl:when>
						<xsl:when test="$value='VERONA'">261</xsl:when>
						<xsl:when test="$value='ROM'">288</xsl:when>
						<xsl:when test="$value='NAPOLI'">309</xsl:when>
						<xsl:when test="$value='WIEN'">313</xsl:when>
						<xsl:when test="$value='LUXEMBURG'">215</xsl:when>
						<xsl:when test="$value='AMSTERDAM'">200</xsl:when>
						<xsl:when test="$value='MUENCHEN'">274</xsl:when>
						<xsl:when test="$value='HAMBURG'">252</xsl:when>
						<xsl:when test="$value='BERLIN'">291</xsl:when>
						<xsl:when test="$value='WARSCHAU'">365</xsl:when>
						<xsl:when test="$value='PRAG'">304</xsl:when>
						<xsl:when test="$value='BUDAPEST'">349</xsl:when>
						<xsl:when test="$value='MINSK'">429</xsl:when>
						<xsl:when test="$value='BELGRAD'">370</xsl:when>
						<xsl:when test="$value='BUKAREST'">438</xsl:when>
						<xsl:when test="$value='ISTANBUL'">461</xsl:when>
						<xsl:when test="$value='ATHEN'">405</xsl:when>
						<xsl:when test="$value='ANKARA'">516</xsl:when>
						<xsl:when test="$value='KIEW'">471</xsl:when>
						<xsl:when test="$value='MOSKAU'">561</xsl:when>
						<xsl:when test="$value='PETERSBURG'">480</xsl:when>
						<xsl:when test="$value='HELSINKI'">409</xsl:when>
						
						<xsl:when test="$value='TRUCK1'">85</xsl:when>
						<xsl:when test="$value='TRUCK2'">85</xsl:when>
						
						<xsl:otherwise>0</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				
				<xsl:variable name="posy">
					<xsl:choose>
						<xsl:when test="$value='MADRID'">607</xsl:when>
						<xsl:when test="$value='BARCELONA'">594</xsl:when>
						<xsl:when test="$value='CADIZ'">650</xsl:when>
						<xsl:when test="$value='LISSABON'">619</xsl:when>
						<xsl:when test="$value='BORDEAUX'">528</xsl:when>
						<xsl:when test="$value='DIJON'">513</xsl:when>
						<xsl:when test="$value='PARIS'">462</xsl:when>
						<xsl:when test="$value='LYON'">483</xsl:when>
						<xsl:when test="$value='VERONA'">528</xsl:when>
						<xsl:when test="$value='ROM'">578</xsl:when>
						<xsl:when test="$value='NAPOLI'">597</xsl:when>
						<xsl:when test="$value='WIEN'">490</xsl:when>
						<xsl:when test="$value='LUXEMBURG'">463</xsl:when>
						<xsl:when test="$value='AMSTERDAM'">396</xsl:when>
						<xsl:when test="$value='MUENCHEN'">492</xsl:when>
						<xsl:when test="$value='HAMBURG'">381</xsl:when>
						<xsl:when test="$value='BERLIN'">410</xsl:when>
						<xsl:when test="$value='WARSCHAU'">421</xsl:when>
						<xsl:when test="$value='PRAG'">447</xsl:when>
						<xsl:when test="$value='BUDAPEST'">496</xsl:when>
						<xsl:when test="$value='MINSK'">395</xsl:when>
						<xsl:when test="$value='BELGRAD'">531</xsl:when>
						<xsl:when test="$value='BUKAREST'">541</xsl:when>
						<xsl:when test="$value='ISTANBUL'">589</xsl:when>
						<xsl:when test="$value='ATHEN'">639</xsl:when>
						<xsl:when test="$value='ANKARA'">608</xsl:when>
						<xsl:when test="$value='KIEW'">442</xsl:when>
						<xsl:when test="$value='MOSKAU'">331</xsl:when>
						<xsl:when test="$value='PETERSBURG'">249</xsl:when>
						<xsl:when test="$value='HELSINKI'">241</xsl:when>
						
						<xsl:when test="$value='TRUCK1'">55</xsl:when>
						<xsl:when test="$value='TRUCK2'">185</xsl:when>
						
						<xsl:otherwise>0</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				
		    	<div class="destination">
					<xsl:attribute name="style">
						left: <xsl:value-of select="$posx - 20"/>px;
						top: <xsl:value-of select="$posy - 15"/>px;
					</xsl:attribute>
					<img>
						<xsl:attribute name="src"><xsl:value-of select="concat($stylesheetURL, '/logistics/', translate($owner, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '_destination.png')"/></xsl:attribute>
						<xsl:attribute name="alt"><xsl:value-of select="$owner"/> at <xsl:value-of select="$value"/> (<xsl:value-of select="$posx"/>, <xsl:value-of select="$posy"/>)</xsl:attribute>
						<xsl:attribute name="title"><xsl:value-of select="$owner"/> at <xsl:value-of select="$value"/> (<xsl:value-of select="$posx"/>, <xsl:value-of select="$posy"/>)</xsl:attribute>
					</img>
				</div>
			</xsl:for-each>
			
		</div>
		
		<!-- show remaining fluents -->
		<xsl:call-template name="state">
			<xsl:with-param name="excludeFluent" select="'MOVE'"/>
			<xsl:with-param name="excludeFluent2" select="'AT'"/>
			<xsl:with-param name="excludeFluent3" select="'DESTINATION'"/>
		</xsl:call-template>
	</xsl:template>
</xsl:stylesheet>
