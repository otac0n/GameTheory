using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTheory.Games.Nessos
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an Amphora card in Nessos.
    /// </summary>
    public class Card : IComparable<Card>, ITokenFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Card"/> class.
        /// </summary>
        /// <param name="name">The name of the Amphora card.</param>
        /// <param name="value">The number of points granted by this card.</param>
        public Card(string name, int value = 0)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets the name of the Amphora card.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the value of this card.
        /// </summary>
        public int Value { get; }

        public IList<object> FormatTokens => throw new NotImplementedException();

        /// <inheritdoc/>
        public int CompareTo(Card other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }
            else if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            int comp;

            if ((comp = this.Name.CompareTo(other.Name)) != 0 ||
                (comp = this.Value.CompareTo(other.Value)) != 0)
            {
                return comp;
            }

            return 0;
        }
    }
}
