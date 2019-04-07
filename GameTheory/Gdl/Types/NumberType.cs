// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    public class NumberType : ObjectType
    {
        public static readonly NumberType Instance = new NumberType();

        private NumberType()
            : base("int")
        {
        }
    }
}
