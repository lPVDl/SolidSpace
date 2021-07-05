namespace SolidSpace.UI.Factory.Intefaces
{
    public interface IStringFieldCorrectionBehaviour
    {
        public string TryFixString(string value, out bool wasFixed);
    }
}