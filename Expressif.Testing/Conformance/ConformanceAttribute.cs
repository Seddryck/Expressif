using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace Expressif.Testing.Conformance;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ConformanceAttribute : NUnitAttribute, ITestBuilder
{
    private const string CategoryName = "conformance";

    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite)
    {
        ArgumentNullException.ThrowIfNull(method);

        var type = method.TypeInfo?.Type
            ?? throw new InvalidOperationException("ConformanceAttribute can only be applied to methods declared on a concrete type.");
        var testName = method.Name;

        var builder = new NUnitTestCaseBuilder();
        foreach (var testCaseData in ConformanceTestCaseDataSource.GetCases(type, testName))
        {
            testCaseData.SetCategory(CategoryName);
            yield return builder.BuildTestMethod(method, suite, testCaseData);
        }
    }
}
