using System.Reflection;

namespace SpaceSimulator.Runtime.Editor
{
    public struct ValidationMethod
    {
        public MethodInfo method;
        public object validator;
    }
}