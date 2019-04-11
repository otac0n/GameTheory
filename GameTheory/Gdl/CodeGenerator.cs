// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using GameTheory.Gdl.Types;

    internal static partial class CodeGenerator
    {
        private static HashSet<string> keywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte",
            "case", "catch", "char", "checked", "class", "const",
            "continue", "decimal", "default", "delegate", "do", "double",
            "else", "enum", "event", "explicit", "extern", "false",
            "finally", "fixed", "float", "for", "foreach", "goto",
            "if", "implicit", "in", "int", "interface", "internal",
            "is", "lock", "long", "namespace", "new", "null",
            "object", "operator", "out", "override", "params", "private",
            "protected", "public", "readonly", "ref", "return", "sbyte",
            "sealed", "short", "sizeof", "stackalloc", "static", "string",
            "struct", "switch", "this", "throw", "true", "try",
            "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
            "using", "virtual", "void", "volatile", "while",
        };

        private static Dictionary<char, string> simpleEscapeChars = new Dictionary<char, string>()
        {
            { '\'', "\\'" }, { '\"', "\\\"" }, { '\\', "\\\\" }, { '\0', "\\0" },
            { '\a', "\\a" }, { '\b', "\\b" }, { '\f', "\\f" }, { '\n', "\\n" },
            { '\r', "\\r" }, { '\t', "\\t" }, { '\v', "\\v" },
        };

        private static string EscapeName(object name)
        {
            if (name is ExpressionType type)
            {
                if (type.BuiltInType != null)
                {
                    return type.BuiltInType.FullName;
                }
                else if (type is UnionType || type is IntersectionType || (type is ObjectType && !(type is FunctionType)))
                {
                    // Will be handled by run-time type checks.
                    return "object";
                }
            }

            var n = name.ToString();
            return keywords.Contains(n) ? "@" + n : n;
        }

        private static string ToLiteral(object item)
        {
            string input;
            if (item is null)
            {
                return "null";
            }
            else
            {
                input = item.ToString();
            }

            var sb = new StringBuilder(input.Length * 2);
            sb.Append("\"");
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (simpleEscapeChars.TryGetValue(c, out var literal))
                {
                    sb.Append(literal);
                }
                else if (c >= 32 && c <= 126)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                }
            }

            sb.Append("\"");
            return sb.ToString();
        }
    }
}
