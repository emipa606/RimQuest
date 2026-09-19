using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimQuest;

[HarmonyPatch(typeof(PawnGroupKindWorker_Trader), "GenerateGuards")]
public static class PawnGroupKindWorker_Trader_GenerateGuards
{
    //PawnGroupKindWorker_Trader
    public static void Postfix(List<Pawn> outPawns)
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

        if (!Rand.Chance(RimQuestMod.instance.Settings.questChance))
        {
            return;
        }

        var questPawn = new QuestPawn(newQuestPawn);
        questPawns.Add(questPawn);
    }
}