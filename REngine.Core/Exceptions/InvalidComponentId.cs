namespace REngine.Core.Exceptions;

public class InvalidComponentId(int id, Type type) : Exception(
    $"Invalid Component. Id={id}, Type={type.Name}"    
)
{
    public int Id => id;
}