namespace ImGuiNET;

public class Constants
{
#if WINDOWS
    public const string ImGuiLibrary = "cimgui.dll";
#elif ANDROID || LINUX
    public const string ImGuiLibrary = "libcimgui.so";
#elif WEB
    public const string ImGuiLibrary = "cimgui";
#else
    public const string ImGuiLibrary = "unknow";
#endif
}