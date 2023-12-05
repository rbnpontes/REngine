namespace ImGuiNET;

public class Constants
{
#if WINDOWS
    public const string ImGuiLibrary = "cimgui.dll";
#elif ANDROID || LINUX
    public const string ImGuiLibrary = "libcimgui.so";
#endif
}