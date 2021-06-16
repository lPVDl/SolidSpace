namespace SolidSpace.Playground.UI
{
    public interface IStringFieldCorrectionBehaviour
    {
        public string TryFixString(string value, out bool wasFixed);
    }
}