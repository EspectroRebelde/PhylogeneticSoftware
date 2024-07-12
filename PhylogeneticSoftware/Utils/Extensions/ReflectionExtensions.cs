using TypeExtensions;

namespace ReflectionExtensions
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Returns all types in the current AppDomain implementing the interface or inheriting the type. 
        /// </summary>
        public static IEnumerable<Type> ImplementorsOfAbstractClass(Type abstractType)
        {
            return abstractType.GetTImplementors(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}