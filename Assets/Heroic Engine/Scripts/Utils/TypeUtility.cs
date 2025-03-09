using System;
using System.Reflection;

namespace HeroicEngine.Utils
{
    public class TypeUtility
    {
        /// <summary>
        /// This method returns Type by given string name. If type wasn't found in assembly, it returns null.
        /// </summary>
        /// <param name="name">Type name</param>
        /// <returns>Type</returns>
        public static Type GetTypeByName(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == name)
                        return type;
                }
            }

            return null;
        }
    }
}