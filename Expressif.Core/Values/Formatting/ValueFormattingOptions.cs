using System;
using System.Collections.Generic;

namespace Expressif.Values.Formatting;

/// <summary>Controls how Expressif values are formatted for display.</summary>
public sealed class ValueFormattingOptions
{
    /// <summary>Gets or sets the presentation style.</summary>
    /// <value>The compact or pretty presentation style.</value>
    public ValueFormat Format { get; init; } = ValueFormat.Compact;

    /// <summary>Gets or sets the text repeated for each indentation level.</summary>
    /// <value>The indentation text. The default is two spaces.</value>
    public string Indentation { get; init; } = "  ";

    /// <summary>
    /// Gets or sets the exact runtime types whose values may be rendered as compact atomic subtrees in pretty output.
    /// </summary>
    /// <value>The inline-eligible runtime types. The default set is empty.</value>
    public IReadOnlySet<Type> InlineValueTypes { get; init; } = new HashSet<Type>();

    /// <summary>
    /// Gets or sets the preferred maximum line width used to decide whether an eligible value remains inline.
    /// </summary>
    /// <value>The preferred line width. The default is 80.</value>
    public int PreferredLineWidth { get; init; } = 80;
}
