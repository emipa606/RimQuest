using System;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimQuest;

public class Dialog_QuestGiver : Window
{
    private const float TitleHeight = 42f;

    private const float ButtonHeight = 35f;

    private readonly int actualPlayerSilver;

    private readonly int actualSilverCost;

    private readonly float creationRealTime;

    private readonly Pawn interactor;

    private readonly QuestPawn questPawn;
    private readonly string title = "RQ_QuestOpportunity".Translate();

    public float interactionDelay;

    private Vector2 scrollPosition = Vector2.zero;

    private object selectedQuest;

    public Dialog_QuestGiver(QuestPawn newQuestPawn, Pawn newInteractor)
    {
        questPawn = newQuestPawn;
        interactor = newInteractor;
        forcePause = true;
        absorbInputAroundWindow = true;
        creationRealTime = RealTime.LastRealTime;
        onlyOneOfTypeAllowed = false;
        actualSilverCost = determineSilverCost();
        actualPlayerSilver = determineSilverAvailable(interactor);
    }

    private string Text =>
        "RQ_QuestDialog".Translate(interactor.LabelShort, questPawn.pawn.LabelShort, actualSilverCost);

    public override Vector2 InitialSize =>
        new(640f, Math.Max(460f, 320f + (RimQuestMod.instance.Settings.amount * 25)));

    private float TimeUntilInteractive =>
        interactionDelay - (Time.realtimeSinceStartup - creationRealTime);

    private bool InteractionDelayExpired => TimeUntilInteractive <= 0f;

    private static int determineSilverAvailable(Pawn pawn)
    {
        var currencies = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Silver);
        return currencies is not { Count: > 0 } ? 0 : currencies.Sum(currency => currency.stackCount);
    }

    private int determineSilverCost()
    {
        var currentSilver = RimQuestMod.instance.Settings.questPrice;
        var priceFactorBuyTraderPriceFactor =
            (float)questPawn.pawn.Faction.RelationWith(Faction.OfPlayer).baseGoodwill;
        priceFactorBuyTraderPriceFactor += priceFactorBuyTraderPriceFactor < 0f ? 0f : 100f;
        priceFactorBuyTraderPriceFactor *= priceFactorBuyTraderPriceFactor < 0f ? -1f : 1f;
        priceFactorBuyTraderPriceFactor *= 0.005f;
        priceFactorBuyTraderPriceFactor = 1f - priceFactorBuyTraderPriceFactor;

        var priceGain_PlayerNegotiator = interactor.GetStatValue(StatDefOf.TradePriceImprovement);
        priceGain_PlayerNegotiator += 1;

        currentSilver = Mathf.Max(currentSilver, 1);
        currentSilver /= priceGain_PlayerNegotiator;

        currentSilver += currentSilver * priceFactorBuyTraderPriceFactor *
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
        var viewRect = new Rect(0f, 0f, width, calcHeight(width) + questPawn.CalcOptionsHeight(width));
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
            var questDescription = string.Empty;
            if (questPawn.questsAndIncidents[index] is QuestScriptDef questScriptDef)
            {
                questName = Main.GetQuestReadableName(questScriptDef);
                questDef = questScriptDef;
                questDescription = questScriptDef.description;
            }

            if (questPawn.questsAndIncidents[index] is IncidentDef incidentDef)
            {
                questName = incidentDef.LabelCap;
                questDef = incidentDef;
                questDescription = incidentDef.description;
            }

            if (string.IsNullOrEmpty(questName))
            {
                continue;
            }

            var rect6 = new Rect(viewRect.width / 4,
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

            TooltipHandler.TipRegion(rect6, questDescription);
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

            switch (selectedQuest)
            {
                case QuestScriptDef questDef:
                {
                    var incidentParms =
                        StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.GiveQuest, Find.World);
                    var storytellerComp = Find.Storyteller.storytellerComps.First(comp =>
                        comp is StorytellerComp_OnOffCycle or StorytellerComp_RandomMain);
                    incidentParms =
                        storytellerComp.GenerateParms(IncidentCategoryDefOf.GiveQuest, incidentParms.target);

                    var slate = new Slate();

                    slate.Set("points", incidentParms.points);
                    slate.Set("discoveryMethod",
                        "QuestDiscoveredFromTrader".Translate(questPawn.pawn.Named("TRADER"),
                            interactor.Named("NEGOTIATOR")));

                    QuestUtility.SendLetterQuestAvailable(
                        QuestUtility.GenerateQuestAndMakeAvailable(questDef, slate));
                    break;
                }
                case IncidentDef incidentDef:
                {
                    var incidentParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.World);
                    if (incidentDef.pointsScaleable)
                    {
                        var storytellerComp = Find.Storyteller.storytellerComps.First(comp =>
                            comp is StorytellerComp_OnOffCycle or StorytellerComp_RandomMain);
                        incidentParms = storytellerComp.GenerateParms(incidentDef.category, incidentParms.target);
                    }

                    incidentDef.Worker.TryExecute(incidentParms);
                    break;
                }
            }

            var questPawns = RimQuestTracker.Instance.questPawns;
            if (questPawns != null && questPawns.Contains(questPawn))
            {
                questPawns.Remove(questPawn);
            }

            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            receiveSilver(questPawn.pawn, actualSilverCost);
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

    private static void receiveSilver(Pawn receiver, int amountOwed)
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

    private float calcHeight(float width)
    {
        var result = Verse.Text.CalcHeight(Text, width);
        return result;
    }
}