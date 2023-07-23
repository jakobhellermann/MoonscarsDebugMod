using System.Reflection;
using JetBrains.Annotations;
using ModdingAPI;

namespace MoonscarsDebugMod.DebugMod;

[UsedImplicitly]
internal class DebugMod : Mod {
    public override string GetName() => Assembly.GetExecutingAssembly().GetName().Name;

    public override string Version() => Assembly.GetExecutingAssembly().GetName().Version.ToString();


    public override void Load() {
        Logger.Log("Loaded DebugMod");
    }

    public override void Unload() {
        Logger.Log("Unloaded DebugMod");
    }
}