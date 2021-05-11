namespace SolidSpace
{
    public interface IDataValidator<T>
    {
        public string Validate(T data);
    }
}