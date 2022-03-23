// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Displays
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Xsl;
    using GameTheory.Catalogs;
    using GameTheory.Gdl.Catalogs;
    using GameTheory.Gdl.Shared;
    using HtmlAgilityPack;
    using Newtonsoft.Json.Linq;
    using TheArtOfDev.HtmlRenderer.WinForms;
    using HtmlDocument = HtmlAgilityPack.HtmlDocument;

    public class IXmlWithStylesheetDisplay : Display
    {
        private static readonly XmlWriterSettings Settings = new XmlWriterSettings
        {
            Async = true,
            OmitXmlDeclaration = true,
            Indent = true,
        };

        private static readonly XNamespace xsl = "http://www.w3.org/1999/XSL/Transform";
        private GdlGame game;
        private XslCompiledTransform transform;

        public IXmlWithStylesheetDisplay(ICatalogGame game)
        {
            this.game = game as GdlGame;

            if (this.game != null)
            {
                var metadataStylesheet = this.game.Metadata.StyleSheet;
                if (!string.IsNullOrEmpty(metadataStylesheet) && Path.GetExtension(metadataStylesheet).ToUpperInvariant() == ".XSL")
                {
                    var path = Path.Combine(Path.GetDirectoryName(this.game.MetadataPath), metadataStylesheet);
                    var doc = XDocument.Load(path, LoadOptions.SetBaseUri);

                    while (true)
                    {
                        try
                        {
                            using (var reader = doc.CreateReader())
                            {
                                var transform = new XslCompiledTransform();
                                transform.Load(reader);
                                this.transform = transform;
                                break;
                            }
                        }
                        catch (XsltException ex)
                        {
                            // TODO: I need to find a future-proof and locale-proff mechanism to detect this error.
                            var messageMatch = Regex.Match(ex.Message, "The named template '(?<name>[_A-Za-z0-9]+)' does not exist.");
                            if (messageMatch.Success)
                            {
                                // Add a runtime termination, rather than failing the transform comilation.
                                doc.Root.Add(
                                    new XElement(
                                        xsl + "template",
                                        new XAttribute("name", messageMatch.Groups["name"].Value),
                                        new XElement(
                                            xsl + "message",
                                            new XAttribute("terminate", "yes"),
                                            ex.ToString())));
                                continue;
                            }

                            throw;
                        }
                    }
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
                    }

                    var doc = new HtmlDocument();
                    doc.LoadHtml(sw.ToString());
                    var head = doc.DocumentNode.SelectSingleNode("//head");
                    if (head == null)
                    {
                        doc.DocumentNode.PrependChild(head = doc.CreateElement("head"));
                    }

                    var @base = head.SelectSingleNode("base");
                    if (@base == null)
                    {
                        head.PrependChild(@base = doc.CreateElement("base"));
                    }

                    @base.Attributes.Add("href", HtmlEntity.Entitize(new Uri(Path.Combine(Path.GetDirectoryName(this.game.MetadataPath), "dummy/")).ToString()));

                    ////var metadataStylesheet = this.game.Metadata.StyleSheet;
                    ////var sources = (JObject)this.game.Metadata.Extensions["sources"];
                    ////var sourceLookup = sources.Properties().ToLookup(p => (string)p.Value, p => p.Name, StringComparer.InvariantCultureIgnoreCase);
                    ////var sourceRoot = new Uri((string)sources[metadataStylesheet]);
                    ////var stylesheet = new Uri(Path.Combine(Path.GetDirectoryName(this.game.MetadataPath), metadataStylesheet), UriKind.Absolute);
                    ////
                    ////var images = doc.DocumentNode.SelectNodes("//img[@src]");
                    ////var anchors = doc.DocumentNode.SelectNodes("//a[@href]");
                    ////var links = doc.DocumentNode.SelectNodes("//link[@href]");
                    ////var all = Enumerable.Concat(
                    ////    Enumerable.Concat(
                    ////        (images ?? Enumerable.Empty<HtmlNode>()).Select(image => image.Attributes["src"]),
                    ////        (anchors ?? Enumerable.Empty<HtmlNode>()).Select(anchor => anchor.Attributes["href"])),
                    ////    (links ?? Enumerable.Empty<HtmlNode>()).Select(link => link.Attributes["href"]));
                    ////foreach (var attr in all)
                    ////{
                    ////    var href = attr.DeEntitizeValue;
                    ////    var url = new UriBuilder(new Uri(sourceRoot, href));
                    ////    url.Path = url.Path.Replace(string.Format("{0}{0}", Path.AltDirectorySeparatorChar), $"{Path.AltDirectorySeparatorChar}");
                    ////    var source = sourceLookup[url.ToString()].FirstOrDefault();
                    ////    attr.Value = HtmlEntity.Entitize((source == null ? new Uri(stylesheet, href) : new Uri(stylesheet, source)).ToString());
                    ////}

                    html = doc.DocumentNode.OuterHtml;
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
