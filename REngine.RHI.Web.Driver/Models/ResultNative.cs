namespace REngine.RHI.Web.Driver.Models;

public unsafe struct ResultNative
{
    public IntPtr Value;
    public IntPtr Error;

    public ref ResultNative GetPinnableReference()
    {
        return ref this;
    }
}