// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GameTheory.Comparers;

    /// <summary>
    /// Provides utility methods for working with reflection.
    /// </summary>
    public static class ReflectionUtilities
    {
        public static List<Type[]> FixGenericTypeConstraints(Type[] typeParameters, Converter<Type, IEnumerable<Type>> getCandidates)
        {
            var constraints = Array.ConvertAll(typeParameters, p => p.GetTypeInfo().GetGenericParameterConstraints());
            var candidates = Array.ConvertAll(typeParameters, p => getCandidates(p).ToArray());

            var visited = new HashSet<IList<Type>>(new ListEqualityComparer<Type>());
            void FixTypes(Type[] node, List<Type[]> results)
            {
                for (var i = node.Length - 1; i >= 0; i--)
                {
                    if (node[i] != null)
                    {
                        continue;
                    }

                    var parameter = typeParameters[i];
                    foreach (var candidate in candidates[i])
                    {
                        if (MatchesConstraints(parameter, candidate, constraints[i]))
                        {
                            node[i] = candidate;

                            var nodeCopy = Array.ConvertAll(node, n => n);
                            if (visited.Add(nodeCopy))
                            {
                                if (nodeCopy.All(n => n != null))
                                {
                                    results.Add(nodeCopy);
                                }
                                else
                                {
                                    var oldConstraints = constraints;
                                    constraints = Array.ConvertAll(constraints, cs => Array.ConvertAll(cs, c => ReplaceTypeParameter(c, parameter, candidate)));
                                    FixTypes(node, results);
                                    constraints = oldConstraints;
                                }
                            }
                            else
                            {
                                // TODO: Duplicate reduction.
                            }

                            node[i] = null;
                        }
                    }
                }
            }

            var matches = new List<Type[]>();
            var root = new Type[typeParameters.Length];
            FixTypes(root, matches);

            return matches;
        }

        public static IEnumerable<MethodBase> GetPublicInitializers(this Type type)
        {
            var staticProperties = from staticProperty in type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                                   where staticProperty.PropertyType == type
                                   select staticProperty.GetGetMethod(false);

            var constructors = from constructor in type.GetConstructors()
                               let parameters = constructor.GetParameters()
                               select (MethodBase)constructor;

            return staticProperties.Concat(constructors);
        }

        public static object InvokeStatic(this MethodBase method, params object[] parameters)
        {
            return method is ConstructorInfo constructorInfo
                ? constructorInfo.Invoke(parameters)
                : method.Invoke(null, parameters);
        }

        public static bool MatchesConstraint(Type typeParameter, Type typeArgument, Type typeConstraint)
        {
            return typeConstraint.GetTypeInfo().IsAssignableFrom(typeArgument.GetTypeInfo());
        }

        public static bool MatchesConstraints(Type typeParameter, Type typeArgument, Type[] typeConstraints)
        {
            return typeConstraints.All(c => MatchesConstraint(typeParameter, typeArgument, c));
        }

        public static Type ReplaceTypeParameter(Type input, Type pattern, Type replacement)
        {
            if (input == pattern)
            {
                return replacement;
            }

            if (input.IsConstructedGenericType)
            {
                var replaced = false;

                var typeArguments = input.GenericTypeArguments;
                for (var i = 0; i < typeArguments.Length; i++)
                {
                    Type output;
                    var innerInput = typeArguments[0];
                    if ((output = ReplaceTypeParameter(innerInput, pattern, replacement)) != innerInput)
                    {
                        typeArguments[i] = output;
                        replaced = true;
                    }
                }

                if (replaced)
                {
                    return input.GetGenericTypeDefinition().MakeGenericType(typeArguments);
                }
            }

            return input;
        }

        public static Type TryMakeGenericType(Type genericTypeDefinition, params Type[] argument)
        {
            try
            {
                return genericTypeDefinition.MakeGenericType(argument);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
