using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimQuest;

public class JobDriver_QuestWithPawn : JobDriver
{
    private Pawn QuestGiver => (Pawn)TargetThingA;

    public override bool TryMakePreToilReservations(bool yeaaa)
    {
        return pawn.Reserve(QuestGiver, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
            .FailOn(() => !QuestGiver.CanRequestQuestNow());
        var trade = new Toil();
        trade.initAction = delegate
        {
            var actor = trade.actor;
            if (QuestGiver.CanRequestQuestNow())
            {
                Find.WindowStack.Add(new Dialog_QuestGiver(QuestGiver.GetQuestPawn(), actor));
            }
        };
        yield return trade;
    }
}