// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    public class BooleanType : ObjectType
    {
        public static readonly BooleanType Instance = new BooleanType();

        private BooleanType()
            : base("bool", null, typeof(bool))
        {
        }
    }
}
