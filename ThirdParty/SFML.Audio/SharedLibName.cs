namespace SFML.Audio
{
    public static class CSFML
    {
#if WINDOWS
        public const string audio = "csfml-audio-2.dll";
        public const string system = "csfml-system-2.dll";
#elif ANDROID || LINUX
        public const string audio = "libcsfml-audio.so";
        public const string system = "libcsfml-system.so";
#endif
    }
}
