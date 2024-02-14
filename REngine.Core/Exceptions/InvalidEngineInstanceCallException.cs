namespace REngine.Core.Exceptions;

#if RENGINE_VALIDATIONS
public class InvalidEngineInstanceCallException(string message) : Exception(message)
{
    internal static void Validate(EngineInstanceStep currentStep, EngineInstanceStep expectedStep)
    {
        if (currentStep == expectedStep)
            return;
        throw new InvalidEngineInstanceCallException(
            $"Invalid Engine Instance Call. Current Step={currentStep}, Expected Step={expectedStep}");
    }
}
#endif