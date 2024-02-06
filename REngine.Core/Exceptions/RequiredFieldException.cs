namespace REngine.Core.Exceptions;

public class RequiredFieldException(Type type, string fieldName) : Exception($"The field '{fieldName}' on '{type.Name} is required.")
{
    public Type Type => type;
    public string FieldName => fieldName;
}