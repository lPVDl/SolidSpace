namespace SpaceSimulator
{
    public interface IValidator<T>
    {
        public string Validate(T data);
    }
}