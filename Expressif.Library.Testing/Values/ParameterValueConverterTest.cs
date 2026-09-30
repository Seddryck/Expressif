using Expressif.Bindings;
using Expressif.Values;

namespace Expressif.Testing.Values;

public class ParameterValueConverterTest
{
    [Test]
    public void Convert_UnsupportedDirectValue_RendersExpressifSource()
        => Assert.That(
            new ParameterValueConverter().Convert(new VariableParameter("threshold")),
            Is.EqualTo("@threshold"));
}
