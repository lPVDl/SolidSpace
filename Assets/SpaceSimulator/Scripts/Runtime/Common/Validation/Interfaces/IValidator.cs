namespace SpaceSimulator.Runtime
{
    public interface IValidator<T>
    {
        public string Validate(T data);
    }
}