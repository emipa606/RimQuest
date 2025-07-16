using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RimQuest;

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
    static HarmonyPatches()
    {
        new Harmony("rimworld.rimquest").PatchAll(Assembly.GetExecutingAssembly());
    }

    public static void RenderExclamationPointOverlay(Pawn pawn)
    {
        if (!pawn.Spawned)
        {
            return;
        }

        var drawPos = pawn.DrawPos;
        drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.21951221f;
        drawPos.x += pawn.def.size.x - 1.52f;
        drawPos.z += pawn.def.size.z - 0.45f;

        renderPulsingOverlayQuest(pawn, Main.exclamationPointMat, drawPos, MeshPool.plane05);
    }

    private static void renderPulsingOverlayQuest(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
    {
        var num = ((float)Math.Sin((Time.realtimeSinceStartup + (397f * (thing.thingIDNumber % 571))) * 4f) + 1f) *
                  0.5f;
        num = 0.3f + (num * 0.7f);
        var material = FadedMaterialPool.FadedVersionOf(mat, num);
        Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, material, 0);
    }
}