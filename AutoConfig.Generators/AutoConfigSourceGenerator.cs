using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace ToolBX.AutoConfig.Generators;

[Generator]
public class AutoConfigSourceGenerator : IIncrementalGenerator
{
    private const string AutoConfigAttributeName = "ToolBX.AutoConfig.AutoConfigAttribute";
    private const string AutoConfigGenericAttributeName = "ToolBX.AutoConfig.AutoConfigAttribute<T>";

    private static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var fromClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                // Matches both `class` and `record class` declarations (config types are idiomatically records).
                predicate: static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax
                    && ((TypeDeclarationSyntax)node).AttributeLists.Count > 0,
                transform: static (ctx, _) => GetTypeRegistrations(ctx))
            .Where(static registrations => registrations is { Count: > 0 })
            .SelectMany(static (registrations, _) => registrations!);

        var fromAssembly = context.CompilationProvider
            .SelectMany(static (compilation, _) => GetAssemblyRegistrations(compilation));

        var collected = fromClasses.Collect().Combine(fromAssembly.Collect());

        context.RegisterSourceOutput(collected, static (spc, pair) =>
            Execute(spc, pair.Left.AddRange(pair.Right)));
    }

    private static List<RegistrationEntry>? GetTypeRegistrations(GeneratorSyntaxContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol)
            return null;

        List<RegistrationEntry>? registrations = null;

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (TryCreateRegistration(attribute, typeSymbol) is not { } registration)
                continue;

            registrations ??= [];
            registrations.Add(registration);
        }

        return registrations;
    }

    private static IEnumerable<RegistrationEntry> GetAssemblyRegistrations(Compilation compilation)
    {
        foreach (var attribute in compilation.Assembly.GetAttributes())
        {
            // Assembly-level attributes can only target an external type, so there is no fallback symbol.
            if (TryCreateRegistration(attribute, fallbackTarget: null) is { } registration)
                yield return registration;
        }
    }

    private static RegistrationEntry? TryCreateRegistration(AttributeData attribute, INamedTypeSymbol? fallbackTarget)
    {
        var attributeClass = attribute.AttributeClass;
        if (attributeClass is null)
            return null;

        var attributeName = attributeClass.ConstructedFrom.ToDisplayString();
        var isGeneric = attributeName == AutoConfigGenericAttributeName;
        if (!isGeneric && attributeName != AutoConfigAttributeName)
            return null;

        INamedTypeSymbol? target;
        if (isGeneric)
        {
            target = attributeClass.TypeArguments.Length == 1
                ? attributeClass.TypeArguments[0] as INamedTypeSymbol
                : null;
        }
        else
        {
            // The non-generic attribute binds the type it is placed on.
            target = fallbackTarget;
            if (target is { } self &&
                (self.IsAbstract || !self.IsReferenceType || self.IsGenericType))
            {
                return null;
            }
        }

        if (target is null)
            return null;

        if (attribute.ConstructorArguments.Length == 0 ||
            attribute.ConstructorArguments[0].Value is not string name)
        {
            return null;
        }

        var validateDataAnnotations = false;
        var validateOnStart = false;
        foreach (var namedArgument in attribute.NamedArguments)
        {
            switch (namedArgument.Key)
            {
                case "ValidateDataAnnotations" when namedArgument.Value.Value is bool vda:
                    validateDataAnnotations = vda;
                    break;
                case "ValidateOnStart" when namedArgument.Value.Value is bool vos:
                    validateOnStart = vos;
                    break;
            }
        }

        return new RegistrationEntry(
            target.ToDisplayString(FullyQualifiedFormat),
            name,
            validateDataAnnotations,
            validateOnStart);
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<RegistrationEntry> registrations)
    {
        if (registrations.IsDefaultOrEmpty)
            return;

        var distinct = registrations.Distinct().ToList();

        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine("#pragma warning disable");
        builder.AppendLine();
        builder.AppendLine("using Microsoft.Extensions.Configuration;");
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine("using Microsoft.Extensions.Options;");
        builder.AppendLine();
        builder.AppendLine("[assembly: global::ToolBX.AutoConfig.HasAutoConfigServices]");
        builder.AppendLine();
        builder.AppendLine("namespace ToolBX.AutoConfig.Generated");
        builder.AppendLine("{");
        builder.AppendLine("    internal static class AutoConfigRegistrar");
        builder.AppendLine("    {");
        builder.AppendLine("        [global::System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(\"AutoConfig binds configuration sections through reflection-based configuration binding.\")]");
        builder.AppendLine("        [global::System.Diagnostics.CodeAnalysis.RequiresDynamicCode(\"AutoConfig binds configuration sections through reflection-based configuration binding.\")]");
        builder.AppendLine("        public static void Register(");
        builder.AppendLine("            IServiceCollection services,");
        builder.AppendLine("            IConfiguration configuration,");
        builder.AppendLine("            global::ToolBX.AutoConfig.AutoConfigOptions options)");
        builder.AppendLine("        {");

        foreach (var registration in distinct)
        {
            builder.AppendLine("            {");
            builder.AppendLine(
                $"                var builder = services.AddOptions<{registration.TargetType}>().Bind(configuration.GetSection({ToLiteral(registration.SectionName)}));");

            builder.AppendLine(registration.ValidateDataAnnotations
                ? "                builder.ValidateDataAnnotations();"
                : "                if (options.ValidateDataAnnotations) builder.ValidateDataAnnotations();");

            builder.AppendLine(registration.ValidateOnStart
                ? "                builder.ValidateOnStart();"
                : "                if (options.ValidateOnStart) builder.ValidateOnStart();");

            builder.AppendLine("            }");
        }

        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        public static void CollectOptions(");
        builder.AppendLine("            global::System.IServiceProvider serviceProvider,");
        builder.AppendLine("            global::System.Collections.Generic.List<object> destination)");
        builder.AppendLine("        {");

        foreach (var targetType in distinct.Select(x => x.TargetType).Distinct())
        {
            var id = StableId(targetType);
            builder.AppendLine(
                $"            if (serviceProvider.GetService<IOptions<{targetType}>>() is {{ }} options_{id})");
            builder.AppendLine($"                destination.Add(options_{id}.Value);");
        }

        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource("AutoConfigRegistrar.g.cs", builder.ToString());
    }

    private static string ToLiteral(string value) => "@\"" + value.Replace("\"", "\"\"") + "\"";

    private static string StableId(string fullyQualifiedName)
    {
        var chars = fullyQualifiedName.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray();
        return new string(chars);
    }

    private sealed record RegistrationEntry(
        string TargetType,
        string SectionName,
        bool ValidateDataAnnotations,
        bool ValidateOnStart);
}
