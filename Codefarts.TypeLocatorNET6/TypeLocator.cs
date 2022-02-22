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

    /// <summary>
    /// Occurs when a view model type needs to be resolved.
    /// </summary>
    public event ResolveEventHandler TypeResolve;

    // public Type FindType(string typeName, bool? cacheType)
    // {
    //     if (typeName == null)
    //     {
    //         throw new ArgumentNullException(nameof(typeName));
    //     }
    //
    //     cacheType = cacheType ?? true;
    //
    //     try
    //     {
    //         // attempt to create from cache first
    //       IEnumerable<  Type> instanciatedObject;
    //         if (this.CreateTypeFromCache(typeName, cacheType.Value, out instanciatedObject))
    //         {
    //             // viewService.SendMessage(GenericMessageConstants.SetModel, wpfView, GenericMessageArguments.SetModel(viewModelRef));
    //             return instanciatedObject;
    //         }
    //
    //         // if not in cache scan for
    //         if (this.ScanDomainForType(typeName, cacheType.Value, out instanciatedObject))
    //         {
    //             //  viewService.SendMessage(GenericMessageConstants.SetModel, wpfView, GenericMessageArguments.SetModel(viewModelRef));
    //             return instanciatedObject;
    //         }
    //
    //         // if (this.SearchForAssemblies(typeName, cacheType, assemblyFiles, context, out instanciatedObject))
    //         // {
    //         //     // viewService.SendMessage(GenericMessageConstants.SetModel, wpfView, GenericMessageArguments.SetModel(viewModelRef));
    //         //     return instanciatedObject;
    //         // }
    //
    //         throw new Exception($"Type '{typeName}' not be found.");
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new Exception($"Type '{typeName}' could not be found.", ex);
    //     }
    // }

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
                // viewService.SendMessage(GenericMessageConstants.SetModel, wpfView, GenericMessageArguments.SetModel(viewModelRef));
                return instanciatedObject;
            }

            // if not in cache scan for
            if (this.ScanDomainForType(typeName, cacheType, out instanciatedObject))
            {
                //  viewService.SendMessage(GenericMessageConstants.SetModel, wpfView, GenericMessageArguments.SetModel(viewModelRef));
                return instanciatedObject;
            }

            if (this.SearchForAssemblies(typeName, cacheType, assemblyFiles, context, out instanciatedObject))
            {
                // viewService.SendMessage(GenericMessageConstants.SetModel, wpfView, GenericMessageArguments.SetModel(viewModelRef));
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

    // private bool GetTypeFromAssembly(string typeName, Assembly asm, bool cacheType, out Type instanciatedObject)
    // {
    //     if (asm == null)
    //     {
    //         throw new ArgumentNullException(nameof(asm));
    //     }
    //
    //     var types = asm.GetTypes().AsParallel();
    //     var typesFound = types.Where(x => x.IsClass && !x.IsAbstract && x != typeof(string) && x.Name.Equals(typeName, StringComparison.Ordinal));
    //
    //     var firstType = typesFound.FirstOrDefault();
    //     //var item = firstType;
    //     // try event first that way consumers could use IoC container and dependency injection if need be
    //     var item = firstType != null ? this.OnTypeResolve(new ResolveEventArgs(firstType.FullName)) : null;
    //
    //     // if null attempt direct type creation fallback
    //     if (item == null)
    //     {
    //         item = firstType != null ? asm.CreateInstance(firstType.FullName) : null;
    //     }
    //
    //     if (item != null && cacheType)
    //     {
    //         // successfully created so add type to cache for faster access
    //         //  if (cacheType)
    //         // {
    //         lock (previouslyCreatedTypes)
    //         {
    //             previouslyCreatedTypes[typeName] = firstType;
    //         }
    //         // }
    //
    //         instanciatedObject = item;
    //         return true;
    //     }
    //
    //     instanciatedObject = null;
    //     return false;
    // }

    private bool GetTypesFromAssembly(string typeName, Assembly asm, bool cacheType, out IEnumerable<Type> instanciatedObject)
    {
        if (asm == null)
        {
            throw new ArgumentNullException(nameof(asm));
        }

        var types = asm.GetTypes().AsParallel();
        var typesFound = types.Where(x => x.IsClass && !x.IsAbstract && x != typeof(string) && x.Name.Equals(typeName, StringComparison.Ordinal));

        //  var firstType = typesFound.FirstOrDefault();
        var item = typesFound;
        //    var item = firstType;
        // try event first that way consumers could use IoC container and dependency injection if need be
        //        var item = firstType != null ? this.OnTypeResolve(new ResolveEventArgs(firstType.FullName)) : null;

        // if null attempt direct type creation fallback
        // if (item == null)
        // {
        //     item = firstType != null ? asm.CreateInstance(firstType.FullName) : null;
        // }

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
        // ====== If we have made it here the view may be located in a currently unloaded assembly located in the app path

        // search application path assemblies
        // TODO: should use codebase? see https://stackoverflow.com/questions/837488/how-can-i-get-the-applications-path-in-a-net-console-application
        //var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // get all assemblies
        //var viewModelFiles = Directory.GetFiles(appPath, "*.vmodels", SearchOption.AllDirectories);

        //var results = assemblyFiles.Where(x => { return File.Exists(x) && this.GetTypesFromAssembly(typeName, x, cacheView, out viewModelRef); })


        // check each file
        foreach (var file in assemblyFiles.Where(x => File.Exists(x)))
        {
            //   var asmFile = Path.ChangeExtension(file, ".dll");
            // if (!File.Exists(file))
            // {
            //     continue;
            // }

#if NETCOREAPP3_1_OR_GREATER
            //var asmName = new AssemblyName(Path.GetFileNameWithoutExtension(file));
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
            // if (this.GetTypeFromAssembly(typeName, type.Assembly, cacheView, out instanciatedObject))
            // {
            return true;
            // }
        }

        instanciatedObject = null;
        return false;
    }

    /// <summary>
    /// Raises the <see cref="TypeResolve"/> event and returns the result.
    /// </summary>
    /// <param name="args">The type creation args containing information about the type to create.</param>
    /// <returns>An object reference that was create from the type information.</returns>
    /// <remarks>If no event handlers are available will return null.</remarks>
    protected virtual object OnTypeResolve(ResolveEventArgs args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        var handler = this.TypeResolve;
        if (handler != null)
        {
            return handler(this, args);
        }

        return null;
    }
}