namespace REngine.Core;

#if RENGINE_VALIDATIONS
internal enum EngineInstanceStep
{
    Setup,
    Start,
    Run,
    Stop
}
#endif