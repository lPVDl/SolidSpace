using System.Reflection;

namespace SolidSpace.Editor
{
    internal struct ValidationMethod
    {
        public MethodInfo method;
        public object validator;
    }
}