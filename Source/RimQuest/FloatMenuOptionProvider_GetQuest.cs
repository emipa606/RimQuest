using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimQuest;

public class FloatMenuOptionProvider_GetQuest : FloatMenuOptionProvider
{
    protected override bool Drafted => true;

    protected override bool Undrafted => true;

    protected override bool Multiselect => false;

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
    {
        if (clickedPawn?.GetQuestPawn() == null)
        {
            yield break;
        }

        if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly))
        {
            yield return new FloatMenuOption("RQ_CannotQuest".Translate() + " (" + "NoPath".Translate() + ")", null);
            yield break;
        }

        if (context.FirstSelectedPawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {
            yield return new FloatMenuOption("CannotPrioritizeWorkTypeDisabled".Translate(SkillDefOf.Social.LabelCap),
                null);
            yield break;
        }

        var str = string.Empty;
        if (clickedPawn.Faction != null)
        {
            str = $" ({clickedPawn.Faction.Name})";
        }

        var label = "RQ_QuestWith".Translate(clickedPawn.LabelShort) + str;
        var action = (Action)tradeAction;
        const MenuOptionPriority priority2 = MenuOptionPriority.InitiateSocial;
        yield return FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption(label, action, priority2, null, clickedPawn), context.FirstSelectedPawn,
            clickedPawn);
        yield break;

        void tradeAction()
        {
            var job = new Job(RimQuestDefOf.RQ_QuestWithPawn, clickedPawn)
            {
                playerForced = true
            };
            context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job);
        }
    }
}