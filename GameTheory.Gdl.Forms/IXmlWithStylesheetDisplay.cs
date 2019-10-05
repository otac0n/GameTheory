// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Displays
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml;
    using GameTheory.Gdl.Shared;
    using TheArtOfDev.HtmlRenderer.WinForms;
    using static Controls;

    public class IXmlWithStylesheetDisplay : Display
    {
        private static readonly XmlWriterSettings Settings = new XmlWriterSettings
        {
            Async = true,
            OmitXmlDeclaration = true,
        };

        private IXmlWithStylesheetDisplay()
        {
        }

        public static IXmlWithStylesheetDisplay Instance { get; } = new IXmlWithStylesheetDisplay();

        public override bool CanDisplay(Scope scope, Type type, object value) => typeof(IXml).IsAssignableFrom(type); // TODO: Only game state? Infer sub templates from XLS in compiler?

        protected override Control Update(Control originalDisplay, Scope scope, Type type, object value, IReadOnlyList<Display> displays)
        {
            string html;
            var xml = ((IXml)value);
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream, Settings))
                {
                    xml.ToXml(writer);
                }

                stream.Seek(0, SeekOrigin.Begin);

                // TODO: Find XSLT.
                using (var reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
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
                    Text = html,
                    Tag = this,
                };
            }
        }
    }
}
