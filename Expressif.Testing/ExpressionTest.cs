using Expressif.Functions;
using Expressif.Values;
using Expressif.Values.Special;
using System.Data;
using System.Diagnostics;

namespace Expressif.Testing
{
    public class ExpressionTest
    {
        [SetUp]
        public void Setup()
        { }

        [Test]
        public void Evaluate_SingleFunctionWithoutParameter_Valid()
        {
            var expression = new Expression("lower");
            var result = expression.Evaluate("Nikola Tesla");
            Assert.That(result, Is.EqualTo("nikola tesla"));
        }

        [Test]
        public void Evaluate_SingleFunctionWithOneParameter_Valid()
        {
            var expression = new Expression("remove-chars(a)");
            var result = expression.Evaluate("Nikola Tesla");
            Assert.That(result, Is.EqualTo("Nikol Tesl"));
        }

        [Test]
        public void Evaluate_TwoFunctions_Valid()
        {
            var expression = new Expression("lower | remove-chars(a)");
            var result = expression.Evaluate("Nikola Tesla");
            Assert.That(result, Is.EqualTo("nikol tesl"));
        }

        [Test]
        public void Evaluate_VariableAsParameter_Valid()
        {
            var context = new Context();
            context.Variables.Add<char>("myChar", 'k');

            var expression = new Expression("lower | remove-chars(@myChar)", context);
            var result = expression.Evaluate("Nikola Tesla");
            Assert.That(result, Is.EqualTo("niola tesla"));
        }

        [Test]
        public void Evaluate_ObjectPropertyAsParameter_Valid()
        {
            var context = new Context();
            context.CurrentObject.Set(new { CharToBeRemoved = 't' });

            var expression = new Expression("lower | remove-chars([CharToBeRemoved])", context);
            var result = expression.Evaluate("Nikola Tesla");
            Assert.That(result, Is.EqualTo("nikola esla"));
        }

        [Test]
        public void Evaluate_ObjectIndexAsParameter_Valid()
        {
            var context = new Context();
            context.CurrentObject.Set(new List<char>() { 'e', 's' });

            var expression = new Expression("lower | remove-chars(#1)", context);
            var result = expression.Evaluate("Nikola Tesla");
            Assert.That(result, Is.EqualTo("nikola tela"));
        }

        [Test]
        public void Evaluate_Synonyms_Valid()
        {
            var context = new Context();
            context.CurrentObject.Set(new List<char>() { 'e', 's' });

            var expression = new Expression("text-to-lower | text-to-remove-chars(#1)", context);
            var result = expression.Evaluate("Nikola Tesla");
            Assert.That(result, Is.EqualTo("nikola tela"));
        }

        [Test]
        public void Evaluate_FunctionAsParameter_Valid()
        {
            var context = new Context();
            context.Variables.Add<int>("myVar", 6);
            context.CurrentObject.Set(new List<int>() { 15, 8, 3 });

            var expression = new Expression("lower | skip-last-chars( {@myVar | subtract(#2) })", context);
            var result = expression.Evaluate("Nikola Tesla");
            Assert.That(result, Is.EqualTo("nikola te"));
        }

    }
}