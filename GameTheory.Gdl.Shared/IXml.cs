using System.Threading.Tasks;
using System.Xml;

namespace GameTheory.Gdl.Shared
{
    /// <summary>
    /// An object that can be written to an xml writer.
    /// </summary>
    public interface IXml
    {
        /// <summary>
        /// Writes the object to the
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        Task ToXml(XmlWriter writer);
    }
}
