using REngine.Core.Mathematics;

namespace REngine.Core;

public static class Platform
{
    public static readonly ulong Desktop = Hash.Digest("Desktop");
    public static readonly ulong Mobile = Hash.Digest("Mobile");
    public static readonly ulong Windows = Hash.Digest("Windows");
    public static readonly ulong Android = Hash.Digest("Android");
    public static readonly ulong Linux = Hash.Digest("Linux");
    public static readonly ulong Ios = Hash.Digest("iOS");
    public static readonly ulong Mac = Hash.Digest("MacOS");
    public static readonly ulong Browser = Hash.Digest("Browser");

    public static ulong GetPlatform()
    {
        if (OperatingSystem.IsWindows())
            return Windows;
        if (OperatingSystem.IsAndroid())
            return Android;
        if (OperatingSystem.IsLinux())
            return Linux;
        if (OperatingSystem.IsIOS())
            return Ios;
        if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            return Mac;
        return OperatingSystem.IsBrowser() ? Browser : 0ul;
    }

    public static bool IsDesktop()
    {
        return OperatingSystem.IsWindows() || (OperatingSystem.IsLinux() && !OperatingSystem.IsAndroid()) || OperatingSystem.IsMacOS() ||
               OperatingSystem.IsMacCatalyst();
    }

    public static bool IsMobile()
    {
        return OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();
    }

    public static bool IsWeb()
    {
        return OperatingSystem.IsBrowser();
    }

    public static bool IsTargetPlatform(ulong platformId)
    {
        if (platformId == Mobile)
            return IsMobile();
        if (platformId == Desktop)
            return IsDesktop();
        return platformId == GetPlatform();
    }
}