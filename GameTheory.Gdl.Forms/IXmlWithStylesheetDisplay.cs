// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Displays
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Xsl;
    using GameTheory.Catalogs;
    using GameTheory.Gdl.Catalogs;
    using GameTheory.Gdl.Shared;
    using TheArtOfDev.HtmlRenderer.WinForms;
    using static Controls;

    public class IXmlWithStylesheetDisplay : Display
    {
        private static readonly XmlWriterSettings Settings = new XmlWriterSettings
        {
            Async = true,
            OmitXmlDeclaration = true,
            Indent = true,
        };

        private GdlGame game;
        private XslCompiledTransform transform;

        public IXmlWithStylesheetDisplay(ICatalogGame game)
        {
            this.game = game as GdlGame;

            if (this.game != null)
            {
                var metadataStylesheet = this.game.Metadata.StyleSheet;
                if (!string.IsNullOrEmpty(metadataStylesheet) && Path.GetExtension(metadataStylesheet).ToLowerInvariant() == ".xsl")
                {
                    // TODO: Use a prioritized list of xsl stylesheets.
                }

                var xsl = Directory.EnumerateFiles(Path.GetDirectoryName(this.game.MetadataPath), "*.xsl").FirstOrDefault();
                if (xsl != null)
                {
                    var transform = new XslCompiledTransform();
                    transform.Load(xsl);
                    this.transform = transform;
                }
            }
        }

        public override bool CanDisplay(Scope scope, Type type, object value) => typeof(IXml).IsAssignableFrom(type); // TODO: Only game state? Infer sub templates from XLS in compiler?

        protected override Control Update(Control originalDisplay, Scope scope, Type type, object value, IReadOnlyList<Display> displays)
        {
            string html;
            var xml = (IXml)value;
            using (var stream = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stream, Settings))
                {
                    xml.ToXml(writer);
                }

                var xmlValue = stream.ToString();
                using (var reader = XmlReader.Create(new StringReader(xmlValue)))
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("<!--");
                    sw.WriteLine(xmlValue);
                    sw.WriteLine("-->");

                    using (var writer = XmlWriter.Create(sw, Settings))
                    {
                        this.transform.Transform(reader, new XsltArgumentList(), writer);
                        html = sw.ToString();
                    }
                }
            }

            if (originalDisplay is HtmlPanel htmlPanel && htmlPanel.Tag == this)
            {
                htmlPanel.Text = html;
                return htmlPanel;
            }
            else
            {
                return new HtmlPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    Text = html,
                    Tag = this,
                };
            }
        }
    }
}
