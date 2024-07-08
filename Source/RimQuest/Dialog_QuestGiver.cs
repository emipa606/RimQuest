using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimQuest;

public class Dialog_QuestGiver : Window
{
    private const float TitleHeight = 42f;

    private const float ButtonHeight = 35f;

    public readonly int actualPlayerSilver;

    public readonly int actualSilverCost;

    private readonly float creationRealTime;

    public readonly Pawn interactor;

    public readonly QuestPawn questPawn;
    private readonly string title = "RQ_QuestOpportunity".Translate();

    public float interactionDelay;

    private Vector2 scrollPosition = Vector2.zero;

    public object selectedQuest;

    public Dialog_QuestGiver(QuestPawn newQuestPawn, Pawn newInteractor)
    {
        questPawn = newQuestPawn;
        interactor = newInteractor;
        forcePause = true;
        absorbInputAroundWindow = true;
        creationRealTime = RealTime.LastRealTime;
        onlyOneOfTypeAllowed = false;
        actualSilverCost = DetermineSilverCost();
        actualPlayerSilver = DetermineSilverAvailable(interactor);
    }

    private string Text =>
        "RQ_QuestDialog".Translate(interactor.LabelShort, questPawn.pawn.LabelShort, actualSilverCost);

    public override Vector2 InitialSize => new Vector2(640f, 460f);

    private float TimeUntilInteractive =>
        interactionDelay - (Time.realtimeSinceStartup - creationRealTime);

    private bool InteractionDelayExpired => TimeUntilInteractive <= 0f;

    private int DetermineSilverAvailable(Pawn pawn)
    {
        var currencies = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Silver);
        return currencies is not { Count: > 0 } ? 0 : currencies.Sum(currency => currency.stackCount);
    }

    private int DetermineSilverCost()
    {
        var currentSilver = RimQuestMod.instance.Settings.QuestPrice;
        var priceFactorBuy_TraderPriceFactor =
            (float)questPawn.pawn.Faction.RelationWith(Faction.OfPlayer).baseGoodwill;
        priceFactorBuy_TraderPriceFactor += priceFactorBuy_TraderPriceFactor < 0f ? 0f : 100f;
        priceFactorBuy_TraderPriceFactor *= priceFactorBuy_TraderPriceFactor < 0f ? -1f : 1f;
        priceFactorBuy_TraderPriceFactor *= 0.005f;
        priceFactorBuy_TraderPriceFactor = 1f - priceFactorBuy_TraderPriceFactor;

        var priceGain_PlayerNegotiator = interactor.GetStatValue(StatDefOf.TradePriceImprovement);
        priceGain_PlayerNegotiator += 1;

        currentSilver = Mathf.Max(currentSilver, 1);
        currentSilver /= priceGain_PlayerNegotiator;

        currentSilver += currentSilver * priceFactorBuy_TraderPriceFactor *
                         (1f + Find.Storyteller.difficulty.tradePriceFactorLoss);
        currentSilver = Mathf.Min(currentSilver, RimQuestMod.MaxCost);
        return Mathf.RoundToInt(currentSilver);
    }

    public override void DoWindowContents(Rect inRect)
    {
        var num = inRect.y;
        Verse.Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(0f, num, inRect.width, TitleHeight), title);
        num += TitleHeight;
        Verse.Text.Font = GameFont.Small;
        var outRect = new Rect(inRect.x, num, inRect.width, inRect.height - ButtonHeight - 5f - num);
        var width = outRect.width - 16f;
        var viewRect = new Rect(0f, 0f, width, CalcHeight(width) + questPawn.CalcOptionsHeight(width));
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        Widgets.Label(new Rect(0f, 0f, viewRect.width, viewRect.height - questPawn.CalcOptionsHeight(width)),
            Text.AdjustedFor(questPawn.pawn));
        if (questPawn.questsAndIncidents.Count == 0)
        {
            questPawn.GenerateQuestsAndIncidents();
        }

        for (var index = 0; index < questPawn.questsAndIncidents.Count; index++)
        {
            object questDef = null;
            var questName = string.Empty;
            if (questPawn.questsAndIncidents[index] is QuestScriptDef questScriptDef)
            {
                questName = Main.GetQuestReadableName(questScriptDef);
                questDef = questScriptDef;
            }

            if (questPawn.questsAndIncidents[index] is IncidentDef incidentDef)
            {
                questName = incidentDef.LabelCap;
                questDef = incidentDef;
            }

            if (string.IsNullOrEmpty(questName))
            {
                continue;
            }

            var rect6 = new Rect(24f,
                viewRect.height - questPawn.CalcOptionsHeight(width) +
                ((Verse.Text.CalcHeight(questName, width) + 12f) * index) + 8f, viewRect.width / 2f,
                Verse.Text.CalcHeight(questName, width));
            if (Mouse.IsOver(rect6))
            {
                Widgets.DrawHighlight(rect6);
            }

            if (Widgets.RadioButtonLabeled(rect6, questName, selectedQuest == questDef))
            {
                selectedQuest = questDef;
            }
        }

        Widgets.EndScrollView();
        if (Widgets.ButtonText(new Rect(0f, inRect.height - ButtonHeight, (inRect.width / 2f) - 20f, ButtonHeight),
                "CancelButton".Translate(), true, false))
        {
            Close();
        }

        if (actualPlayerSilver >= actualSilverCost)
        {
            if (selectedQuest == null || !Widgets.ButtonText(
                    new Rect((inRect.width / 2f) + 20f, inRect.height - ButtonHeight, (inRect.width / 2f) - 20f,
                        ButtonHeight),
                    "Confirm".Translate() + " (" + "RQ_SilverAmt".Translate(actualSilverCost) + ")", true, false))
            {
                return;
            }

            if (selectedQuest is QuestScriptDef questDef)
            {
                var incidentParms =
                    StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.GiveQuest, Find.World);
                var storytellerComp = Find.Storyteller.storytellerComps.First(comp =>
                    comp is StorytellerComp_OnOffCycle or StorytellerComp_RandomMain);
                incidentParms =
                    storytellerComp.GenerateParms(IncidentCategoryDefOf.GiveQuest, incidentParms.target);
                QuestUtility.SendLetterQuestAvailable(
                    QuestUtility.GenerateQuestAndMakeAvailable(questDef, incidentParms.points));
            }

            if (selectedQuest is IncidentDef incidentDef)
            {
                var incidentParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.World);
                if (incidentDef.pointsScaleable)
                {
                    var storytellerComp = Find.Storyteller.storytellerComps.First(comp =>
                        comp is StorytellerComp_OnOffCycle or StorytellerComp_RandomMain);
                    incidentParms = storytellerComp.GenerateParms(incidentDef.category, incidentParms.target);
                }

                incidentDef.Worker.TryExecute(incidentParms);
            }

            var questPawns = RimQuestTracker.Instance.questPawns;
            if (questPawns != null && questPawns.Contains(questPawn))
            {
                questPawns.Remove(questPawn);
            }

            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            ReceiveSilver(questPawn.pawn, actualSilverCost);
            Close();
            Find.WindowStack.Add(new Dialog_MessageBox(
                "RQ_QuestDialogTwo".Translate(questPawn.pawn.LabelShort, interactor.LabelShort)
                    .AdjustedFor(questPawn.pawn), "OK".Translate(), null, null, null, title));
        }
        else
        {
            if (!Widgets.ButtonText(
                    new Rect((inRect.width / 2f) + 20f, inRect.height - ButtonHeight, (inRect.width / 2f) - 20f,
                        ButtonHeight),
                    "RQ_LackFunds".Translate(), true, false))
            {
                return;
            }

            SoundDefOf.ClickReject.PlayOneShotOnCamera();
            Messages.Message("RQ_LackFundsMessage".Translate(), MessageTypeDefOf.RejectInput);
        }
    }

    private void ReceiveSilver(Pawn receiver, int amountOwed)
    {
        var amountUnpaid = amountOwed;
        var currencies = receiver.Map.listerThings.ThingsOfDef(ThingDefOf.Silver);
        if (currencies is { Count: > 0 })
        {
            foreach (var currency in currencies.InRandomOrder())
            {
                if (amountUnpaid <= 0)
                {
                    break;
                }

                var num = Math.Min(amountUnpaid, currency.stackCount);
                currency.SplitOff(num).Destroy();
                amountUnpaid -= num;
            }
        }

        var thing = ThingMaker.MakeThing(ThingDefOf.Silver);
        thing.stackCount = amountOwed;
        receiver.inventory.TryAddItemNotForSale(thing);
    }

    private float CalcHeight(float width)
    {
        var result = Verse.Text.CalcHeight(Text, width);
        return result;
    }
}