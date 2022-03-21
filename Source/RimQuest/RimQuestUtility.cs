using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimQuest;

public static class RimQuestUtility
{
    public static Pawn GetNewQuestGiver(List<Pawn> pawns)
    {
        return pawns.FirstOrDefault(x => !x.NonHumanlikeOrWildMan() && x.trader == null);
    }

    public static bool CanRequestQuestNow(this Pawn pawn)
    {
        if (pawn.Dead || !pawn.Spawned || !pawn.CanCasuallyInteractNow() ||
            pawn.Downed || pawn.IsPrisoner || pawn.Faction == Faction.OfPlayer ||
            pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }

        var questPawns = RimQuestTracker.Instance.questPawns;
        return !questPawns.NullOrEmpty() && questPawns.Any(x => x.pawn == pawn);
    }

    public static QuestPawn GetQuestPawn(this Pawn pawn)
    {
        var possiblePawns = RimQuestTracker.Instance.questPawns;
        return possiblePawns is { Count: > 0 }
            ? possiblePawns.FirstOrDefault(x => x.pawn == pawn)
            : null;
    }
}