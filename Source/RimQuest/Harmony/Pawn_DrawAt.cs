using HarmonyLib;
using Verse;

namespace RimQuest;

[HarmonyPatch(typeof(Pawn), "DrawAt")]
public static class Pawn_DrawAt
{
    public static void Postfix(Pawn __instance)
    {
        if (__instance.GetQuestPawn() != null)
        {
            HarmonyPatches.RenderExclamationPointOverlay(__instance);
        }
    }
}