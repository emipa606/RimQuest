using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimQuest;

public class RimQuestTracker : WorldComponent
{
    public static RimQuestTracker Instance;
    public List<QuestPawn> questPawns = [];

    public RimQuestTracker(World world) : base(world)
    {
        Instance = this;
    }

    public override void FinalizeInit(bool fromLoad)
    {
        base.FinalizeInit(fromLoad);
        Instance = this;
    }

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();
        if (Find.TickManager.TicksGame % 250 == 0 && !questPawns.NullOrEmpty())
        {
            questPawns.RemoveAll(x =>
                x.pawn == null || x.pawn.Downed || x.pawn.Dead || x.pawn.Destroyed || x.pawn.IsColonist);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref questPawns, "questPawns", LookMode.Deep);
        Instance = this;
    }
}