using Expressif.Predicates;
using Expressif.Parsers;
using Expressif.Values.Special;
using Expressif.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Expressif.Functions;
using System.Data.Common;

namespace Expressif;

public interface IPredicationApplicableBuilder : IPredicationBuilder
{
    IParameter? Argument { get; }
}

public class PredicationApplicableBuilder : AbstractPredicationBuilder, IPredicationApplicableBuilder
{
    public IParameter? Argument { get; private set; }

    public PredicationApplicableBuilder(IContext? context = null, PredicationFactory? factory = null, PredicationSerializer? serializer = null)
        : base(context, factory, serializer ?? new PredicationApplicableSerializer()) { }

    protected virtual FirstPredicationApplicableBuilder CreateNext(IPredicationBuilder builder)
        => new ((IPredicationApplicableBuilder)builder);

    public FirstPredicationApplicableBuilder WithArgument(object? argument)
    {
        Argument = GetArgument(argument);
        return CreateNext(this);
    }

    public FirstPredicationApplicableBuilder WithArgument(IParameter argument)
    {
        Argument = GetArgument(argument);
        return CreateNext(this);
    }

    public FirstPredicationApplicableBuilder WithArgument(Expression<Func<IContext, object?>> expression)
    {
        Argument = GetArgument(expression);
        return CreateNext(this);
    }

    protected virtual IParameter GetArgument(object? argument)
        => argument switch
        {
            IParameter p => p,
            Expression<Func<IContext, object?>> expression => new ContextParameter(expression.Compile()),
            _ => new LiteralParameter(argument?.ToString() ?? new Null().Keyword)
        };
}

public class FirstPredicationApplicableBuilder : BaseFirstPredicationBuilder<NextPredicationApplicableBuilder>, IPredicationApplicableBuilder
{
    public IParameter? Argument { get; private set; }

    public FirstPredicationApplicableBuilder(IPredicationApplicableBuilder builder)
        : base(builder.Context, builder.Factory, builder.Serializer)
    {
        Argument = builder.Argument;
    }

    protected override NextPredicationApplicableBuilder CreateNext(IPredicationBuilder builder)
        => new((IPredicationApplicableBuilder)builder);

    public new PredicationApplicable Build()
        => new PredicationApplicableBuildStrategy().Execute(this);
}


public class NextPredicationApplicableBuilder : BaseNextPredicationBuilder<NextPredicationApplicableBuilder>, IPredicationApplicableBuilder
{
    public IParameter? Argument { get; private set; }

    public NextPredicationApplicableBuilder(IPredicationApplicableBuilder builder)
        : base(builder)
    {
        Argument = builder.Argument;
    }

    protected override NextPredicationApplicableBuilder CreateNext(IPredicationBuilder builder)
        => new((IPredicationApplicableBuilder)builder);

    public new PredicationApplicable Build()
        => new PredicationApplicableBuildStrategy().Execute(this);
}

internal interface IPredicationApplicableBuildStrategy
{
    PredicationApplicable Execute(IPredicationApplicableBuilder builder);
}

internal class PredicationApplicableBuildStrategy : IPredicationApplicableBuildStrategy
{
    public PredicationApplicable Execute(IPredicationApplicableBuilder builder)
    {
        if (builder.Pile is null || builder.Argument is null)
            throw new InvalidOperationException();
        var predicate = builder.Factory.Instantiate(builder.Pile, builder.Context);
        return new PredicationApplicable(builder.Argument, predicate, builder.Context);
    }
}
