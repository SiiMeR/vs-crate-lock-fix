using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace CrateLockFix;

[HarmonyPatch]
public class CrateBehaviorPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BlockCrate), nameof(BlockCrate.OnBlockInteractStart))]
    public static bool OnBlockInteractStartPrefix(BlockCrate __instance,
        IWorldAccessor world,
        IPlayer byPlayer,
        BlockSelection blockSel)
    {
        if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
        {
            return false;
        }

        var reinforcementSystem = world.Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>();
        var reinforcement = reinforcementSystem.GetReinforcment(blockSel.Position);

        if (reinforcementSystem.IsLockedForInteract(blockSel.Position, byPlayer))
        {
            if (world.Side == EnumAppSide.Client)
                (world.Api as ICoreClientAPI)?.TriggerIngameError(__instance, "locked", Lang.Get("ingameerror-locked"));
            return false; 
        }
        
        var activeHotbarSlot = byPlayer.InventoryManager?.ActiveHotbarSlot;
        var currentlyHeldItem = activeHotbarSlot?.Itemstack?.Collectible;
        switch (currentlyHeldItem)
        {
            case ItemPlumbAndSquare plumbAndSquare when !byPlayer.Entity.Controls.ShiftKey:
            {
                var enumHandHandling = EnumHandHandling.Handled;
                plumbAndSquare.OnHeldInteractStart(activeHotbarSlot, byPlayer.Entity, blockSel, null, true, ref enumHandHandling);
            
                // reinforcement was successfully handled
                if (enumHandHandling == EnumHandHandling.PreventDefaultAction)
                {
                    return false;
                }

                break;
            }
            case ItemPadlock padlock when reinforcementSystem.IsReinforced(blockSel.Position) && (!reinforcement?.Locked ?? false):
            {
                var enumHandHandling = EnumHandHandling.Handled;
                padlock.OnHeldInteractStart(activeHotbarSlot, byPlayer.Entity, blockSel, null, true, ref enumHandHandling);
            
                // locking was successfully handled
                if (enumHandHandling == EnumHandHandling.PreventDefault)
                {
                    return false;
                }

                break;
            }
        }

        return true;
    }
}