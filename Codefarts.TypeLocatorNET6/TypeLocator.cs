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
    private readonly Dictionary<string, IEnumerable<Type>> previouslyCreatedTypes;
    private Func<Assembly, bool> filter;

    public TypeLocator()
    {
        this.filter = new Func<Assembly, bool>(x => !x.FullName.StartsWith("System") && !x.FullName.StartsWith("Microsoft"));
        this.previouslyCreatedTypes = new Dictionary<string, IEnumerable<Type>>();
    }

    public IEnumerable<Type> FindTypes(string typeName, bool cacheType, IEnumerable<string>? assemblyFiles = null,
                                       AssemblyLoadContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException(nameof(typeName));
        }

        try
        {
            // attempt to create from cache first
            IEnumerable<Type> instanciatedObject;
            if (this.CreateTypeFromCache(typeName, cacheType, out instanciatedObject))
            {
                return instanciatedObject;
            }

            // if not in cache scan for
            if (this.ScanDomainForType(typeName, cacheType, out instanciatedObject))
            {
                return instanciatedObject;
            }

            if (this.SearchForAssemblies(typeName, cacheType, assemblyFiles, context, out instanciatedObject))
            {
                return instanciatedObject;
            }

            throw new TypeLoadException($"Type '{typeName}' not be found.");
        }
        catch (Exception ex)
        {
            throw new TypeLoadException($"Type '{typeName}' could not be found.", ex);
        }
    }

    private bool ScanDomainForType(string viewName, bool cacheViewModel, out IEnumerable<Type> foundTypes)
    {
        // search through all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var filteredAssemblies = assemblies.AsParallel();
        filteredAssemblies = filteredAssemblies.Where(this.filter);
        var results = new List<Type>();
        foreach (var asm in filteredAssemblies)
        {
            if (this.GetTypesFromAssembly(viewName, asm, cacheViewModel, out foundTypes))
            {
                results.AddRange(foundTypes);
            }
        }

        foundTypes = results.Count > 0 ? results : null;
        return results.Count > 0;
    }

    private bool GetTypesFromAssembly(string typeName, Assembly asm, bool cacheType, out IEnumerable<Type> instanciatedObject)
    {
        if (asm == null)
        {
            throw new ArgumentNullException(nameof(asm));
        }

        var types = asm.GetTypes().AsParallel();
        var typesFound = types.Where(x => x.IsClass && !x.IsAbstract && x != typeof(string) && x.Name.Equals(typeName, StringComparison.Ordinal));

        var item = typesFound;

        if (cacheType)
        {
            // successfully created so add type to cache for faster access
            lock (this.previouslyCreatedTypes)
            {
                IEnumerable<Type> current;
                if (!this.previouslyCreatedTypes.TryGetValue(typeName, out current))
                {
                    current = item;
                }

                this.previouslyCreatedTypes[typeName] = current.Union(item).ToList();
            }
        }

        instanciatedObject = item;
        return item.Any();
    }

    private bool SearchForAssemblies(string typeName, bool cacheView, IEnumerable<string>? assemblyFiles, AssemblyLoadContext? context,
                                     out IEnumerable<Type> viewModelRef)
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
            if (this.GetTypesFromAssembly(typeName, assembly, cacheView, out viewModelRef))
            {
                return true;
            }
        }

        viewModelRef = null;
        return false;
    }

    private bool CreateTypeFromCache(string typeName, bool cacheView, out IEnumerable<Type> instanciatedObject)
    {
        if (this.previouslyCreatedTypes.ContainsKey(typeName))
        {
            var type = this.previouslyCreatedTypes[typeName];

            instanciatedObject = type;
            return true;
        }

        instanciatedObject = null;
        return false;
    }
}