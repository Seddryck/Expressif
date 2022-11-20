﻿using Expressif.Values.Resolvers;
using Expressif.Predicates.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Text
{
    public class AnyOfTest
    {
        [Test]
        [TestCase("foo", new[] { "foo", "bar" }, true)]
        [TestCase("foo", new[] { "bar" }, false)]
        [TestCase("foo", new string[0], false)]
        [TestCase("(empty)", new[] { "foo", "bar", "(empty)" }, true)]
        [TestCase("(empty)", new[] { "foo", "bar", "" }, true)]
        [TestCase("(empty)", new[] { "foo", "bar" }, false)]
        [TestCase("", new[] { "foo", "bar", "(empty)" }, true)]
        [TestCase("", new[] { "foo", "bar", "" }, true)]
        [TestCase("", new[] { "foo", "bar" }, false)]
        public void EquivalentTo_Text_Success(object value, object[] references, bool expected)
        {
            var scalars = new List<LiteralScalarResolver<string>>();
            foreach (var reference in references)
                scalars.Add(new LiteralScalarResolver<string>(reference));

            var predicate = new AnyOf(scalars);
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        }
    }
}
