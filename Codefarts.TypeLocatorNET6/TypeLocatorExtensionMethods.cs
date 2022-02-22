using System.Runtime.Loader;

namespace Codefarts.TypeLocator;

public static class TypeLocatorExtensionMethods
{
    public static IEnumerable<Type> FindTypesByName(this TypeLocator locator, string typeName,
                                                    IEnumerable<string>? assemblyFiles = null,
                                                    AssemblyLoadContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException(nameof(typeName));
        }

        return locator.FindTypes(x => x.Name.Equals(typeName, StringComparison.Ordinal), assemblyFiles, context);
    }

    public static IEnumerable<Type> FindTypesByFullName(this TypeLocator locator, string typeName,
                                                        IEnumerable<string>? assemblyFiles = null,
                                                        AssemblyLoadContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException(nameof(typeName));
        }

        return locator.FindTypes(x => x.FullName.Equals(typeName, StringComparison.Ordinal), assemblyFiles, context);
    }
}