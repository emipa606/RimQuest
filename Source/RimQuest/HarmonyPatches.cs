using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimQuest;

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
    private static readonly Material ExclamationPointMat =
        MaterialPool.MatFrom("UI/Overlays/RQ_ExclamationPoint", ShaderDatabase.MetaOverlay);

    static HarmonyPatches()
    {
        var harmony = new Harmony("rimworld.rimquest");
        harmony.Patch(
            AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt)),
            null, new HarmonyMethod(typeof(HarmonyPatches), nameof(RenderPawnAt)));
        harmony.Patch(AccessTools.Method(typeof(IncidentWorker_VisitorGroup), "TryConvertOnePawnToSmallTrader"),
            null, new HarmonyMethod(typeof(HarmonyPatches), nameof(AddQuestGiver)));
        harmony.Patch(AccessTools.Method(typeof(PawnGroupKindWorker_Trader), "GenerateGuards"),
            null, new HarmonyMethod(typeof(HarmonyPatches), nameof(AddQuestGiverTwo)));
        harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
            null, new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHumanlikeOrders)));
        harmony.Patch(AccessTools.Method(typeof(IncidentWorker_NeutralGroup), "SpawnPawns"),
            null, new HarmonyMethod(typeof(HarmonyPatches), nameof(AddQuestGiverThree)));
    }

    public static void RenderPawnAt(Pawn ___pawn)
    {
        if (___pawn.GetQuestPawn() != null)
        {
            RenderExclamationPointOverlay(___pawn);
        }
    }

    //PawnGroupKindWorker_Trader
    public static void AddQuestGiverTwo(List<Pawn> outPawns)
    {
        var newQuestPawn = RimQuestUtility.GetNewQuestGiver(outPawns);
        if (newQuestPawn?.Faction == null)
        {
            return;
        }

        var questPawns = RimQuestTracker.Instance.questPawns;
        if (questPawns.Any(x => x.pawn == newQuestPawn))
        {
            return;
        }

        if (!Rand.Chance(RimQuestMod.instance.Settings.QuestChance))
        {
            return;
        }

        var questPawn = new QuestPawn(newQuestPawn);
        questPawns.Add(questPawn);
    }

    //FloatMenuMakerMap
    public static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
    {
        foreach (var localTargetInfo4 in GenUI.TargetsAt(clickPos, ForQuest(), true))
        {
            var dest = localTargetInfo4;
            if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly))
            {
                opts.Add(
                    new FloatMenuOption("RQ_CannotQuest".Translate() + " (" + "NoPath".Translate() + ")", null));
            }
            else if (pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
            {
                opts.Add(new FloatMenuOption(
                    "CannotPrioritizeWorkTypeDisabled".Translate(SkillDefOf.Social.LabelCap), null));
            }
            else
            {
                var pTarg = (Pawn)dest.Thing;

                void Action4()
                {
                    var job = new Job(RimQuestDefOf.RQ_QuestWithPawn, pTarg)
                    {
                        playerForced = true
                    };
                    pawn.jobs.TryTakeOrderedJob(job);
                }

                var str = string.Empty;
                if (pTarg.Faction != null)
                {
                    str = $" ({pTarg.Faction.Name})";
                }

                var label = "RQ_QuestWith".Translate(pTarg.LabelShort) + str;
                var action = (Action)Action4;
                var priority2 = MenuOptionPriority.InitiateSocial;
                var thing = dest.Thing;
                opts.Add(FloatMenuUtility.DecoratePrioritizedTask(
                    new FloatMenuOption(label, action, priority2, null, thing), pawn, pTarg));
            }
        }
    }

    public static void AddQuestGiver(List<Pawn> pawns, ref bool __result)
    {
        if (!__result || !(pawns?.Count > 1))
        {
            return;
        }

        var newQuestPawn = RimQuestUtility.GetNewQuestGiver(pawns);
        if (newQuestPawn?.Faction == null)
        {
            return;
        }

        var questPawns = RimQuestTracker.Instance.questPawns;
        if (questPawns.Any(x => x.pawn == newQuestPawn))
        {
            return;
        }

        if (!Rand.Chance(RimQuestMod.instance.Settings.QuestChance))
        {
            return;
        }

        var questPawn = new QuestPawn(newQuestPawn);
        questPawns.Add(questPawn);
    }


    public static void AddQuestGiverThree(List<Pawn> __result, IncidentWorker_TravelerGroup __instance)
    {
        if (__result == null || __result.Count == 0)
        {
            return;
        }

        if (__instance.def.defName != "TravelerGroup")
        {
            return;
        }

        var newQuestPawn = RimQuestUtility.GetNewQuestGiver(__result);
        if (newQuestPawn?.Faction == null)
        {
            return;
        }

        var questPawns = RimQuestTracker.Instance.questPawns;
        if (questPawns.Any(x => x.pawn == newQuestPawn))
        {
            return;
        }

        if (!Rand.Chance(RimQuestMod.instance.Settings.QuestChance))
        {
            return;
        }

        var questPawn = new QuestPawn(newQuestPawn);
        questPawns.Add(questPawn);
    }


    private static TargetingParameters ForQuest()
    {
        var targetingParameters = new TargetingParameters
        {
            canTargetPawns = true,
            canTargetBuildings = false,
            mapObjectTargetsMustBeAutoAttackable = false,
            validator = x =>
                x.Thing is Pawn pawn &&
                pawn.GetQuestPawn() != null
        };
        return targetingParameters;
    }


    private static void RenderExclamationPointOverlay(Thing t)
    {
        if (!t.Spawned)
        {
            return;
        }

        var drawPos = t.DrawPos;
        drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.28125f;
        if (t is Pawn)
        {
            drawPos.x += t.def.size.x - 0.52f;
            drawPos.z += t.def.size.z - 0.45f;
        }

        RenderPulsingOverlayQuest(t, ExclamationPointMat, drawPos, MeshPool.plane05);
    }

    private static void RenderPulsingOverlayQuest(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
    {
        var num = (Time.realtimeSinceStartup + (397f * (thing.thingIDNumber % 571))) * 4f;
        var num2 = ((float)Math.Sin(num) + 1f) * 0.5f;
        num2 = 0.3f + (num2 * 0.7f);
        var material = FadedMaterialPool.FadedVersionOf(mat, num2);
        Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, material, 0);
    }
}