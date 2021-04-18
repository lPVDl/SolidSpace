using System.Reflection;

namespace SpaceSimulator.Editor.Validation
{
    public struct ValidationMethod
    {
        public MethodInfo method;
        public object validator;
    }
}