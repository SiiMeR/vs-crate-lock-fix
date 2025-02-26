using HarmonyLib;
using Vintagestory.API.Common;

namespace CrateLockFix;

public class CrateLockFixModSystem : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        var harmony = new Harmony(Mod.Info.ModID);
        harmony.PatchAll();
    }

}