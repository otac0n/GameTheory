// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class EnumCollectionTests
    {
        public enum V
        {
            A = 1,
            B = 3,
            C,
        }

        [Test]
        public void Combinations_WhenCollectionIsEmpty_YieldsNone()
        {
            var subject = EnumCollection<V>.Empty;

            var actual = subject.Combinations(1);

            Assert.That(actual, Is.Empty);
        }

        [TestCase("A:1,B:1,C:1", 2, "A:1,B:1;A:1,C:1;B:1,C:1")]
        [TestCase("A:1,B:1,C:1", 3, "A:1,B:1,C:1")]
        [TestCase("A:3,B:1", 2, "A:1,B:1;A:2")]
        [TestCase("A:2,B:1,C:1", 2, "A:1,B:1;A:1,C:1;A:2;B:1,C:1")]
        public void Combinations_WhenCountIsGreaterThanOne_YieldsDistinctCombinations(string input, int count, string output)
        {
            var subject = ParseEnumCollection(input);
            var expected = ParseArray(output, ParseEnumCollection);

            var actual = subject.Combinations(count);

            Assert.That(Serialize(actual), Is.EqualTo(Serialize(expected)));
        }

        [Test]
        public void Combinations_WhenCountIsOne_YieldsEachKey()
        {
            var subject = EnumCollection<V>.Empty
                .Add(V.A, 1)
                .Add(V.B, 1)
                .Add(V.C, 1);

            var actual = subject.Combinations(1);

            Assert.That(actual.Select(v => v.Single()), Is.EquivalentTo(subject.Keys));
        }

        [Test]
        public void Combinations_WhenCountIsZero_YieldsNone()
        {
            var subject = EnumCollection<V>.Empty
                .Add(V.A, 1)
                .Add(V.B, 1)
                .Add(V.C, 1);

            var actual = subject.Combinations(0);

            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void Permutations_WhenCollectionIsEmpty_YieldsNone()
        {
            var subject = EnumCollection<V>.Empty;

            var actual = subject.Permutations(1);

            Assert.That(actual, Is.Empty);
        }

        [TestCase("A:1,B:1,C:1", 2, "A,B;A,C;B,A;B,C;C,A;C,B")]
        [TestCase("A:1,B:1,C:1", 3, "A,B,C;A,C,B;B,A,C;B,C,A;C,A,B;C,B,A")]
        [TestCase("A:3,B:1", 2, "A,A;A,B;B,A")]
        [TestCase("A:2,B:1,C:1", 2, "A,A;A,B;A,C;B,A;B,C;C,A;C,B")]
        public void Permutations_WhenCountIsGreaterThanOne_YieldsDistinctPermutations(string input, int count, string output)
        {
            var subject = ParseEnumCollection(input);
            var expected = ParseArray(output, p => ParseArray(p, ParseValue, ','));

            var actual = subject.Permutations(count);

            Assert.That(Serialize(actual), Is.EqualTo(Serialize(expected)));
        }

        [Test]
        public void Permutations_WhenCountIsOne_YieldsEachKey()
        {
            var subject = EnumCollection<V>.Empty
                .Add(V.A, 1)
                .Add(V.B, 1)
                .Add(V.C, 1);

            var actual = subject.Permutations(1);

            Assert.That(actual.Select(v => v.Single()), Is.EquivalentTo(subject.Keys));
        }

        [Test]
        public void Permutations_WhenCountIsZero_YieldsNone()
        {
            var subject = EnumCollection<V>.Empty
                .Add(V.A, 1)
                .Add(V.B, 1)
                .Add(V.C, 1);

            var actual = subject.Permutations(0);

            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void WeightedCombinations_WhenCollectionIsEmpty_YieldsNone()
        {
            var subject = EnumCollection<V>.Empty;

            var actual = subject.WeightedCombinations(1);

            Assert.That(actual, Is.Empty);
        }

        [TestCase("A:1,B:1,C:1", 2, "A:1,B:1@1;A:1,C:1@1;B:1,C:1@1")]
        [TestCase("A:1,B:1,C:1", 3, "A:1,B:1,C:1@1")]
        [TestCase("A:3,B:1", 2, "A:1,B:1@3;A:2@3")]
        [TestCase("A:2,B:1,C:1", 2, "A:1,B:1@2;A:1,C:1@2;A:2@1;B:1,C:1@1")]
        public void WeightedCombinations_WhenCountIsGreaterThanOne_YieldsDistinctCombinations(string input, int count, string output)
        {
            var subject = ParseEnumCollection(input);
            var expected = ParseArray(output, w => ParseWeighted(w, ParseEnumCollection));

            var actual = subject.WeightedCombinations(count);

            Assert.That(Serialize(actual), Is.EqualTo(Serialize(expected)));
        }

        [Test]
        public void WeightedCombinations_WhenCountIsOne_YieldsEachKey()
        {
            var subject = EnumCollection<V>.Empty
                .Add(V.A, 1)
                .Add(V.B, 1)
                .Add(V.C, 1);

            var actual = subject.WeightedCombinations(1);

            Assert.That(actual.Select(v => v.Value.Single()), Is.EquivalentTo(subject.Keys));
        }

        [Test]
        public void WeightedCombinations_WhenCountIsOne_YieldsWeightsEqualToKeyCount()
        {
            var subject = EnumCollection<V>.Empty
                .Add(V.A, 3)
                .Add(V.B, 2)
                .Add(V.C, 5);

            var actual = subject.WeightedCombinations(1);

            Assert.That(actual.ToDictionary(a => a.Value.Single(), a => a.Weight), Is.EquivalentTo(subject.Keys.ToDictionary(s => s, s => subject[s])));
        }

        [Test]
        public void WeightedCombinations_WhenCountIsZero_YieldsNone()
        {
            var subject = EnumCollection<V>.Empty
                .Add(V.A, 1)
                .Add(V.B, 1)
                .Add(V.C, 1);

            var actual = subject.WeightedCombinations(0);

            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void WeightedPermutations_WhenCollectionIsEmpty_YieldsNone()
        {
            var subject = EnumCollection<V>.Empty;

            var actual = subject.WeightedPermutations(1);

            Assert.That(actual, Is.Empty);
        }

        [TestCase("A:1,B:1,C:1", 2, "A,B@1;A,C@1;B,A@1;B,C@1;C,A@1;C,B@1")]
        [TestCase("A:1,B:1,C:1", 3, "A,B,C@1;A,C,B@1;B,A,C@1;B,C,A@1;C,A,B@1;C,B,A@1")]
        [TestCase("A:3,B:1", 2, "A,A@6;A,B@3;B,A@3")]
        [TestCase("A:2,B:1,C:1", 2, "A,A@2;A,B@2;A,C@2;B,A@2;C,A@2;B,C@1;C,B@1")]
        public void WeightedPermutations_WhenCountIsGreaterThanOne_YieldsDistinctPermutations(string input, int count, string output)
        {
            var subject = ParseEnumCollection(input);
            var expected = ParseArray(output, w => ParseWeighted(w, a => ParseArray(a, ParseValue, ',')));
            Assert.That(Serialize(subject), Is.EqualTo(input));
            Assert.That(Serialize(expected), Is.EqualTo(output));

            var actual = subject.WeightedPermutations(count);

            Assert.That(Serialize(actual), Is.EqualTo(Serialize(expected)));
        }

        [Test]
        public void WeightedPermutations_WhenCountIsOne_YieldsEachKey()
        {
            var subject = EnumCollection<V>.Empty
                .Add(V.A, 1)
                .Add(V.B, 1)
                .Add(V.C, 1);

            var actual = subject.WeightedPermutations(1);

            Assert.That(actual.Select(v => v.Value.Single()), Is.EquivalentTo(subject.Keys));
        }

        [Test]
        public void WeightedPermutations_WhenCountIsOne_YieldsWeightsEqualToKeyCount()
        {
            var subject = EnumCollection<V>.Empty
                .Add(V.A, 3)
                .Add(V.B, 2)
                .Add(V.C, 5);

            var actual = subject.WeightedPermutations(1);

            Assert.That(actual.ToDictionary(a => a.Value.Single(), a => a.Weight), Is.EquivalentTo(subject.Keys.ToDictionary(s => s, s => subject[s])));
        }

        [Test]
        public void WeightedPermutations_WhenCountIsZero_YieldsNone()
        {
            var subject = EnumCollection<V>.Empty
                .Add(V.A, 1)
                .Add(V.B, 1)
                .Add(V.C, 1);

            var actual = subject.WeightedPermutations(0);

            Assert.That(actual, Is.Empty);
        }

        private static T[] ParseArray<T>(string input, Func<string, T> innerParse, char separator = ';') => input.Split(separator).Select(innerParse).ToArray();

        private static EnumCollection<V> ParseEnumCollection(string input)
        {
            return input.Split(',').Aggregate(EnumCollection<V>.Empty, (c, v) =>
            {
                var parts = v.Split(new[] { ':' }, 2);
                return c.Add(ParseValue(parts[0]), int.Parse(parts[1]));
            });
        }

        private static V ParseValue(string input) => (V)Enum.Parse(typeof(V), input);

        private static Weighted<T> ParseWeighted<T>(string input, Func<string, T> innerParse)
        {
            var ix = input.LastIndexOf('@');
            return Weighted.Create(innerParse(input.Substring(0, ix)), double.Parse(input.Substring(ix + 1)));
        }

        private static string Serialize(IEnumerable<EnumCollection<V>> actual) => Serialize(actual, Serialize);

        private static string Serialize(IEnumerable<Weighted<EnumCollection<V>>> actual) => Serialize(actual.OrderByDescending(w => w.Weight).ThenBy(Serialize), Serialize, sort: false);

        private static string Serialize(IEnumerable<Weighted<V[]>> actual) => Serialize(actual.OrderByDescending(w => w.Weight).ThenBy(Serialize), Serialize, sort: false);

        private static string Serialize(Weighted<EnumCollection<V>> actual) => Serialize(actual, Serialize);

        private static string Serialize(IEnumerable<V[]> actual) => Serialize(actual, Serialize);

        private static string Serialize(Weighted<V[]> actual) => Serialize(actual, Serialize);

        private static string Serialize(V[] actual) => Serialize(actual, Serialize, ',', sort: false);

        private static string Serialize<T>(IEnumerable<T> actual, Func<T, string> innerSerialize, char separator = ';', bool sort = true) => string.Join(separator.ToString(), sort ? actual.Select(innerSerialize).OrderBy(x => x) : actual.Select(innerSerialize));

        private static string Serialize<T>(Weighted<T> actual, Func<T, string> innerSerialize) => string.Concat(innerSerialize(actual.Value), '@', actual.Weight);

        private static string Serialize(EnumCollection<V> actual) => string.Join(",", actual.Keys.Select(k => string.Concat(k, ':', actual[k])));

        private static string Serialize(V actual) => actual.ToString();
    }
}
