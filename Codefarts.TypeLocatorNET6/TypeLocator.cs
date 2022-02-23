// <copyright file="TypeLocator.cs" company="Codefarts">
// Copyright (c) Codefarts
// contact@codefarts.com
// http://www.codefarts.com
// </copyright>

using System.Runtime.Loader;

namespace Codefarts.TypeLocator;

public class TypeLocator
{
    public static IEnumerable<Type> FindTypes(Func<Type, bool> typeFilter, IEnumerable<string>? assemblyFiles = null,
                                              AssemblyLoadContext? context = null)
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().AsEnumerable();
            var domainTypes = assemblies.AsParallel().SelectMany(asm => asm.GetTypes().Where(typeFilter));

            var assemblyLoadContext = context == null ? AssemblyLoadContext.Default : context;
            assemblyFiles = assemblyFiles == null ? Enumerable.Empty<string>() : assemblyFiles;
            assemblies = assemblyFiles.AsParallel().Select(file =>
            {
                var assembly = assemblyLoadContext.LoadFromAssemblyPath(file);
                return assembly;
            });

            var asmFileTypes = assemblies.AsParallel().SelectMany(asm => asm.GetTypes().Where(typeFilter));
            return domainTypes.Union(asmFileTypes);
        }
        catch (Exception ex)
        {
            throw new TypeLoadException($"Unexpected exception thrown. See inner exception for details.", ex);
        }
    }

    public static IEnumerable<Type> FindTypesByName(string typeName,
                                                    IEnumerable<string>? assemblyFiles = null,
                                                    AssemblyLoadContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException(nameof(typeName));
        }

        return FindTypes(x => x.Name.Equals(typeName, StringComparison.Ordinal), assemblyFiles, context);
    }

    public static IEnumerable<Type> FindTypesByFullName(string typeName,
                                                        IEnumerable<string>? assemblyFiles = null,
                                                        AssemblyLoadContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException(nameof(typeName));
        }

        return FindTypes(x => x.FullName.Equals(typeName, StringComparison.Ordinal), assemblyFiles, context);
    }
}