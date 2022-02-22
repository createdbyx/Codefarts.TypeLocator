// <copyright file="TypeLocator.cs" company="Codefarts">
// Copyright (c) Codefarts
// contact@codefarts.com
// http://www.codefarts.com
// </copyright>

using System.Reflection;
#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Loader;
#endif

namespace Codefarts.TypeLocator;

public class TypeLocator
{
    public IEnumerable<Type> FindTypes(Func<Type, bool> typeFilter, IEnumerable<string>? assemblyFiles = null,
                                       AssemblyLoadContext? context = null)
    {
        try
        {
            // attempt to create from cache first
            IEnumerable<Type> domainTypes;
            IEnumerable<Type> asmFileTypes;

            // if not in cache scan for
            this.ScanDomainForType(typeFilter, out domainTypes);
            this.SearchAssemblies(typeFilter, assemblyFiles, context, out asmFileTypes);
            return domainTypes.Union(asmFileTypes);
        }
        catch (Exception ex)
        {
            throw new TypeLoadException($"Unexpected exception thrown. See inner exception for details.", ex);
        }
    }

    private bool ScanDomainForType(Func<Type, bool> typeFilter, out IEnumerable<Type> foundTypes)
    {
        // search through all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var results = assemblies.AsParallel().SelectMany(asm => asm.GetTypes().Where(typeFilter));

        foundTypes = results;
        return results.Any();
    }

    private bool SearchAssemblies(Func<Type, bool> typeFilter, IEnumerable<string>? assemblyFiles, AssemblyLoadContext? context,
                                  out IEnumerable<Type> foundTypes)
    {
        // check each file
#if NETCOREAPP3_1_OR_GREATER
        var assemblyLoadContext = context == null ? AssemblyLoadContext.Default : context;
#endif

        var results = assemblyFiles == null
            ? Enumerable.Empty<Type>()
            : assemblyFiles.AsParallel().SelectMany(file =>
            {
#if NETCOREAPP3_1_OR_GREATER
                var assembly = assemblyLoadContext.LoadFromAssemblyPath(file);
#else
                var assembly = Assembly.LoadFrom(file);
#endif
                return assembly.GetTypes().Where(typeFilter);
            });

        foundTypes = results;
        return results.Any();
    }
}