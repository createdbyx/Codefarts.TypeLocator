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
    //  private readonly Dictionary<string, IEnumerable<Type>> previouslyCreatedTypes;
    private Func<Assembly, bool> filter;

    public TypeLocator()
    {
        this.filter = new Func<Assembly, bool>(x => !x.FullName.StartsWith("System") && !x.FullName.StartsWith("Microsoft"));
        // this.previouslyCreatedTypes = new Dictionary<string, IEnumerable<Type>>();
    }

    public IEnumerable<Type> FindTypes(Func<Type, bool> typeFilter, IEnumerable<string>? assemblyFiles = null,
                                       AssemblyLoadContext? context = null)
    {
        try
        {
            // attempt to create from cache first
            IEnumerable<Type> foundTypes;
            // if (this.CreateTypeFromCache(typeName, cacheType, out instanciatedObject))
            // {
            //     return instanciatedObject;
            // }

            // if not in cache scan for
            if (this.ScanDomainForType(typeFilter, out foundTypes))
            {
                return foundTypes;
            }

            if (this.SearchForAssemblies(typeFilter, assemblyFiles, context, out foundTypes))
            {
                return foundTypes;
            }
        }
        catch (Exception ex)
        {
            throw new TypeLoadException($"Type '{typeFilter}' could not be found.", ex);
        }

        return Enumerable.Empty<Type>();
    }

    private bool ScanDomainForType(Func<Type, bool> typeFilter, out IEnumerable<Type> foundTypes)
    {
        // search through all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var filteredAssemblies = assemblies.AsParallel();
        filteredAssemblies = filteredAssemblies.Where(this.filter);
        var results = new List<Type>();
        foreach (var asm in filteredAssemblies)
        {
            if (this.GetTypesFromAssembly(typeFilter, asm, out foundTypes))
            {
                results.AddRange(foundTypes);
            }
        }

        foundTypes = results.Count > 0 ? results : null;
        return results.Count > 0;
    }

    private bool GetTypesFromAssembly(Func<Type, bool> typeFilter, Assembly asm, out IEnumerable<Type> foundTypes)
    {
        if (asm == null)
        {
            throw new ArgumentNullException(nameof(asm));
        }

        var types = asm.GetTypes().AsParallel();
        var typesFound = types.Where(x => x.IsClass && !x.IsAbstract && x != typeof(string) && typeFilter(x));

        var item = typesFound;

        // if (cacheType)
        // {
        //     // successfully created so add type to cache for faster access
        //     lock (this.previouslyCreatedTypes)
        //     {
        //         IEnumerable<Type> current;
        //         if (!this.previouslyCreatedTypes.TryGetValue(typeFilter, out current))
        //         {
        //             current = item;
        //         }
        //
        //         this.previouslyCreatedTypes[typeFilter] = current.Union(item).ToList();
        //     }
        // }

        foundTypes = item;
        return item.Any();
    }

    private bool SearchForAssemblies(Func<Type, bool> typeFilter, IEnumerable<string>? assemblyFiles, AssemblyLoadContext? context,
                                     out IEnumerable<Type> foundTypes)
    {
        // check each file
        foreach (var file in assemblyFiles.Where(x => File.Exists(x)))
        {
#if NETCOREAPP3_1_OR_GREATER
            var assemblyLoadContext = context == null ? AssemblyLoadContext.Default : context;
            var assembly = assemblyLoadContext.LoadFromAssemblyPath(file);
#else
                var assembly = Assembly.LoadFrom(file);
#endif
            if (this.GetTypesFromAssembly(typeFilter, assembly, out foundTypes))
            {
                return true;
            }
        }

        foundTypes = null;
        return false;
    }

    // private bool CreateTypeFromCache(Func<Type,bool> typeName, bool cacheView, out IEnumerable<Type> instanciatedObject)
    // {
    //     if (this.previouslyCreatedTypes.ContainsKey(typeName))
    //     {
    //         var type = this.previouslyCreatedTypes[typeName];
    //
    //         instanciatedObject = type;
    //         return true;
    //     }
    //
    //     instanciatedObject = null;
    //     return false;
    // }
}