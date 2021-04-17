namespace SpaceSimulator.Runtime
{
    public interface IValidator<T>
    {
        public void Validate(T data, ValidationResult result);
    }
}