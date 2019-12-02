// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Shared
{
    using System.Threading.Tasks;
    using System.Xml;

    /// <summary>
    /// An object that can be written to an xml writer.
    /// </summary>
    public interface IXml
    {
        /// <summary>
        /// Writes the object to the specified writer.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to write to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ToXml(XmlWriter writer);
    }
}
