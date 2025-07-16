using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimQuest;

[HarmonyPatch(typeof(IncidentWorker_VisitorGroup), "TryConvertOnePawnToSmallTrader")]
public static class IncidentWorker_VisitorGroup_TryConvertOnePawnToSmallTrader
{
    public static void Postfix(List<Pawn> pawns, ref bool __result)
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

        if (!Rand.Chance(RimQuestMod.instance.Settings.questChance))
        {
            return;
        }

        var questPawn = new QuestPawn(newQuestPawn);
        questPawns.Add(questPawn);
    }
}