using System.Reflection;
using System.Runtime.CompilerServices;

namespace TypeExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// <b>To get the instantiable implementors in the calling assembly:</b><br></br>
    /// <code>
    /// var callingAssembly = Assembly.GetCallingAssembly();
    /// var httpModules = typeof(IHttpModule).GetInstantiableImplementors(callingAssembly);
    /// </code>
    ///<br></br>
    /// <b>To get the implementors in the current AppDomain:</b><br></br>
    /// <code>
    /// var appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();<br></br>
    /// var httpModules = typeof(IHttpModule).GetImplementors(appDomainAssemblies);<br></br>
    /// </code>
    /// 
    /// </example>
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns all types in <paramref name="assembliesToSearch"/> that directly or indirectly implement or inherit from the given type. 
        /// </summary>
        public static IEnumerable<Type> GetImplementors(this Type abstractType, params Assembly[] assembliesToSearch)
        {
            var typesInAssemblies = assembliesToSearch.SelectMany(assembly => assembly.GetTypes());
            return typesInAssemblies.Where(abstractType.IsAssignableFrom);
        }

        /// <summary>
        /// Returns the results of <see cref="GetImplementors"/> that match <see cref="IsInstantiable"/>.
        /// </summary>
        public static IEnumerable<Type> GetInstantiableImplementors(this Type abstractType, params Assembly[] assembliesToSearch)
        {
            var implementors = abstractType.GetImplementors(assembliesToSearch);
            return implementors.Where(IsInstantiable);
        }
        
        /// <summary>
        /// Returns all implementors of any T in <paramref name="types"/> that directly or indirectly implement or inherit from the given type and match <see cref="IsInstantiable"/>.
        /// </summary>
        public static IEnumerable<Type> GetTImplementors(this Type abstractType, Assembly[] assembliesToSearch)
        {
            var typesInAssemblies = assembliesToSearch.Where(x => x.FullName.StartsWith("PhylogeneticSoftware"))
                .SelectMany(x => x.GetTypes());
            
            return typesInAssemblies.Where(x => x is { IsClass: true, IsAbstract: false, BaseType.IsGenericType: true } &&
                                                x.BaseType.GetGenericTypeDefinition() == abstractType);
        }

        /// <summary>
        /// Determines whether <paramref name="type"/> is a concrete, non-open-generic type.
        /// </summary>
        public static bool IsInstantiable(this Type type)
        {
            return !(type.IsAbstract || type.IsGenericTypeDefinition || type.IsInterface);
        }
    }
}

