using System.Reflection;

namespace SolidSpace.Editor
{
    public struct ValidationMethod
    {
        public MethodInfo method;
        public object validator;
    }
}