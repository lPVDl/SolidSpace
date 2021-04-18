using System.Reflection;

namespace SpaceSimulator.Editor
{
    public struct ValidationMethod
    {
        public MethodInfo method;
        public object validator;
    }
}