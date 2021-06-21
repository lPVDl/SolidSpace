namespace SolidSpace.Playground.Core
{
    public interface IPlaygroundToolValueStorage
    {
        float GetValueOrDefault(string name);

        void SetValue(string name, float value);
    }
}