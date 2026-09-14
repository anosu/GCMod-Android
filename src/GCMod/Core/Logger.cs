namespace GCMod;

/// <summary>
/// 统一 LemonLoader 日志封装。
/// </summary>
public static class Logger
{
    public static void Info(string msg) => Core.Log?.Msg(msg);

    public static void Warn(string msg) => Core.Log?.Warning(msg);

    public static void Error(string msg) => Core.Log?.Error(msg);
}
