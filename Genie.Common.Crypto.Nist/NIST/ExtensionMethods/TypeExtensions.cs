using System;
using System.Linq;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Common.ExtensionMethods
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class TypeExtensions
    {
        public static bool ImplementsInterface(this Type type, Type interfaceType)
        {
            var interfaces = type.GetInterfaces();
            return interfaces.Any(t => t == interfaceType);
        }
    }
}
