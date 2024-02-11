namespace REngine.Core.Exceptions;

public class RequiredPlatformException(PlatformType expectedPlatform) : PlatformNotSupportedException($"Unsupported Platform. Expected Platform: {expectedPlatform}")
{
    public PlatformType ExpectedPlatform => expectedPlatform;
}