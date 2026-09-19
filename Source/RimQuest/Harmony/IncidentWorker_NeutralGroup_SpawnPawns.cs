using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimQuest;

[HarmonyPatch(typeof(IncidentWorker_NeutralGroup), "SpawnPawns")]
public static class IncidentWorker_NeutralGroup_SpawnPawns
{
    public static void Postfix(List<Pawn> __result, IncidentWorker_TravelerGroup __instance)
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

        if (!Rand.Chance(RimQuestMod.instance.Settings.questChance))
        {
            return;
        }

        var questPawn = new QuestPawn(newQuestPawn);
        questPawns.Add(questPawn);
    }
}