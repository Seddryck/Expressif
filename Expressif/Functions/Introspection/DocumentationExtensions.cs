﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Expressif.Functions.Introspection;

public record CtorInfo(ParamInfo[] Parameters);

public record ParamInfo(string Name, string Type, string Summary);

/// <summary>
/// Utility class to provide documentation for various types where available with the assembly.
/// </summary>
public static class DocumentationExtensions
{
    /// <summary>
    /// Provides the documentation comments for a specific method.
    /// </summary>
    /// <param name="methodInfo">The MethodInfo (reflection data ) of the member to find documentation for.</param>
    /// <returns>The XML fragment describing the method.</returns>
    public static XmlElement? GetDocumentation(this MethodInfo methodInfo)
    {
        var parametersString = string.Join(", ", methodInfo.GetParameters().Select(parameterInfo => parameterInfo.ParameterType.FullName));

        //AL: 15.04.2008 ==> BUG-FIX remove “()” if parametersString is empty
        if (parametersString.Length > 0)
            return XmlFromName(methodInfo.DeclaringType!, 'M', methodInfo.Name + "(" + parametersString + ")");
        else
            return XmlFromName(methodInfo.DeclaringType!, 'M', methodInfo.Name);
    }

    /// <summary>
    /// Provides the documentation comments for a specific member.
    /// </summary>
    /// <param name="memberInfo">The MemberInfo (reflection data) or the member to find documentation for.</param>
    /// <returns>The XML fragment describing the member.</returns>
    public static XmlElement? GetDocumentation(this MemberInfo memberInfo)
    {
        // First character [0] of member type is prefix character in the name in the XML
        return XmlFromName(memberInfo.DeclaringType!, memberInfo.MemberType.ToString()[0], memberInfo.Name);
    }

    /// <summary>
    /// Returns the Xml documenation summary comment for this member.
    /// </summary>
    /// <param name="memberInfo"></param>
    /// <returns></returns>
    public static string GetSummary(this MemberInfo memberInfo)
        => memberInfo.GetDocumentation()?.SelectSingleNode("summary")?.InnerText.Trim() ?? string.Empty;

    /// <summary>
    /// Provides the documentation comments for a specific type.
    /// </summary>
    /// <param name="type">Type to find the documentation for.</param>
    /// <returns>The XML fragment that describes the type.</returns>
    public static XmlElement? GetDocumentation(this Type type)
        => XmlFromName(type, 'T', "");

    /// <summary>
    /// Gets the summary portion of a type's documenation or returns an empty string if not available.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetSummary(this Type type)
        => type.GetDocumentation()?.SelectSingleNode("summary")?.InnerText.Trim() ?? string.Empty;

    /// <summary>
    /// Gets the summary portion of a type's documenation or returns an empty string if not available.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static CtorInfo[] GetInfoConstructors(this Type type)
    {
        var ctorNodes = XmlFromPattern(type, 'M', "#ctor");

        if (ctorNodes == null || ctorNodes.Length==0)
            return [];

        var ctorInfos = new List<CtorInfo>();
        foreach (var ctorNode in ctorNodes)
        {
            var paramInfos = new List<ParamInfo>();
            var paramNodes = ctorNode.SelectNodes("param");
            foreach (XmlElement paramNode in paramNodes!)
                paramInfos.Add(new ParamInfo(paramNode.GetAttribute("name"), "Undefined", paramNode.InnerText.Trim()));
            ctorInfos.Add(new CtorInfo(paramInfos.ToArray()));
        }
        return ctorInfos.ToArray();
    }      

    /// <summary>
    /// Obtains the XML Element that describes a reflection element by searching the 
    /// members for a member that has a name that describes the element.
    /// </summary>
    /// <param name="type">The type or parent type, used to fetch the assembly.</param>
    /// <param name="prefix">The prefix as seen in the name attribute in the documentation XML.</param>
    /// <param name="name">Where relevant, the full name qualifier for the element.</param>
    /// <returns>The member that has a name that describes the specified reflection element.</returns>
    private static XmlElement? XmlFromName(this Type type, char prefix, string name)
    {
        string fullName = string.IsNullOrEmpty(name)
            ? $"{prefix}:{type.FullName}"
            : $"{prefix}:{type.FullName}.{name}";

        var xmlDocument = XmlFromAssembly(type.Assembly);

        var matchedElement = xmlDocument["doc"]!["members"]!.SelectSingleNode($"member[@name='{fullName}']") as XmlElement;

        return matchedElement;
    }

    /// <summary>
    /// Obtains the XML Element that describes a reflection element by searching the members for a member that is starting by .
    /// </summary>
    /// <param name="type">The type or parent type, used to fetch the assembly.</param>
    /// <param name="prefix">The prefix as seen in the name attribute in the documentation XML.</param>
    /// <param name="pattern">Where relevant, the full name qualifier for the element.</param>
    /// <returns>The member that has a name that describes the specified reflection element.</returns>
    private static XmlElement[] XmlFromPattern(this Type type, char prefix, string pattern)
    {
        string fullName = string.IsNullOrEmpty(pattern)
            ? $"{prefix}:{type.FullName}"
            : $"{prefix}:{type.FullName}.{pattern}";

        var xmlDocument = XmlFromAssembly(type.Assembly);

        var matchedElements = xmlDocument["doc"]!["members"]!.SelectNodes($"member[@name[starts-with(., '{fullName}')]]")!.Cast<XmlElement>();

        return matchedElements.ToArray();
    }

    /// <summary>
    /// A cache used to remember Xml documentation for assemblies.
    /// </summary>
    private static readonly Dictionary<Assembly, XmlDocument> cache = [];

    /// <summary>
    /// A cache used to store failure exceptions for assembly lookups.
    /// </summary>
    private static readonly Dictionary<Assembly, Exception> failCache = [];

    /// <summary>
    /// Obtains the documentation file for the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to find the XML document for.</param>
    /// <returns>The XML document.</returns>
    /// <remarks>This version uses a cache to preserve the assemblies, so that 
    /// the XML file is not loaded and parsed on every single lookup.</remarks>
    public static XmlDocument XmlFromAssembly(this Assembly assembly)
    {
        if (failCache.TryGetValue(assembly, out var value))
            throw value;

        try
        {
            if (!cache.ContainsKey(assembly))
                cache[assembly] = XmlFromAssemblyNonCached(assembly);

            return cache[assembly];
        }
        catch (Exception exception)
        {
            failCache[assembly] = exception;
            throw;
        }
    }

    /// <summary>
    /// Loads and parses the documentation file for the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to find the XML document for.</param>
    /// <returns>The XML document.</returns>
    private static XmlDocument XmlFromAssemblyNonCached(Assembly assembly)
    {
        var assemblyFilename = assembly.Location;

        if (!string.IsNullOrEmpty(assemblyFilename))
        {
            StreamReader streamReader;

            try
            {
                streamReader = new StreamReader(Path.ChangeExtension(assemblyFilename, ".xml"));
            }
            catch (FileNotFoundException exception)
            {
                throw new Exception("XML documentation not present (make sure it is turned on in project properties when building)", exception);
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(streamReader);
            return xmlDocument;
        }
        else
        {
            throw new Exception("Could not ascertain assembly filename", null);
        }
    }
}
