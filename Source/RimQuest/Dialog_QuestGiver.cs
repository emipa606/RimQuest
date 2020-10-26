using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimQuest
{
    public class Dialog_QuestGiver : Window
    {
        private readonly string title = "RQ_QuestOpportunity".Translate();

        private const float TitleHeight = 42f;

        private const float ButtonHeight = 35f;

        public float interactionDelay;

        public const float defaultSilverCost = 50;

        public int actualSilverCost = 50;

        public int actualPlayerSilver = 0;

        public QuestPawn questPawn;

        public Pawn interactor;

        public object selectedQuest = null;

        private Vector2 scrollPosition = Vector2.zero;

        private readonly float creationRealTime = -1f;

        private string Text => "RQ_QuestDialog".Translate(interactor.LabelShort, questPawn.pawn.LabelShort, actualSilverCost);

        public Dialog_QuestGiver(QuestPawn newQuestPawn, Pawn newInteractor)
        {
            questPawn = newQuestPawn;
            interactor = newInteractor;
            //this.closeOnEscapeKey = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            //this.closeOnEscapeKey = false;
            creationRealTime = RealTime.LastRealTime;
            onlyOneOfTypeAllowed = false;
            actualSilverCost = DetermineSilverCost();
            actualPlayerSilver = DetermineSilverAvailable(interactor);
        }

        private int DetermineSilverAvailable(Pawn pawn)
        {
            var currencies = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Silver);
            if (currencies == null || currencies.Count <= 0) return 0;
            return currencies.Sum(currency => currency.stackCount);
        }

        private int DetermineSilverCost()
        {
            var currentSilver = defaultSilverCost; //50
            var priceFactorBuy_TraderPriceFactor = (float)questPawn.pawn.Faction.RelationWith(Faction.OfPlayer).goodwill;
            priceFactorBuy_TraderPriceFactor += (priceFactorBuy_TraderPriceFactor < 0f) ? 0f : 100f;
            priceFactorBuy_TraderPriceFactor *= (priceFactorBuy_TraderPriceFactor < 0f) ? -1f : 1f;
            priceFactorBuy_TraderPriceFactor *= 0.005f;
            priceFactorBuy_TraderPriceFactor = 1f - priceFactorBuy_TraderPriceFactor;

            var priceGain_PlayerNegotiator = interactor.GetStatValue(StatDefOf.TradePriceImprovement, true); //Max 20

            //Avoid 0's for division operation
            if (priceGain_PlayerNegotiator == 0)
                priceGain_PlayerNegotiator = Mathf.Max(priceFactorBuy_TraderPriceFactor, 1);
            currentSilver = Mathf.Max(currentSilver, 1);
            currentSilver /= priceGain_PlayerNegotiator;

            currentSilver += (currentSilver * priceFactorBuy_TraderPriceFactor *
                                             (1f + Find.Storyteller.difficulty.tradePriceFactorLoss));
            currentSilver = Mathf.Min(currentSilver, 200f);
            return Mathf.RoundToInt(currentSilver);
        }

        public override Vector2 InitialSize => new Vector2(640f, 460f);

        private float TimeUntilInteractive =>
            interactionDelay - (Time.realtimeSinceStartup - creationRealTime);

        private bool InteractionDelayExpired => TimeUntilInteractive <= 0f;

        public override void DoWindowContents(Rect inRect)
        {
            float num = inRect.y;
            //if (!this.title.NullOrEmpty())
            //{
            Verse.Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, num, inRect.width, TitleHeight), title);
            num += TitleHeight;
            //}
            Verse.Text.Font = GameFont.Small;
            Rect outRect = new Rect(inRect.x, num, inRect.width, inRect.height - ButtonHeight - 5f - num);
            float width = outRect.width - 16f;
            Rect viewRect = new Rect(0f, 0f, width, CalcHeight(width) + CalcOptionsHeight(width));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
            Widgets.Label(new Rect(0f, 0f, viewRect.width, viewRect.height - CalcOptionsHeight(width)),
                Text.AdjustedFor(questPawn.pawn));
            if(questPawn.questsAndIncidents.Count == 0)
            {
                questPawn.GenerateQuestsAndIncidents();
            }
            for (var index = 0; index < questPawn.questsAndIncidents.Count; index++)
            {
                string defname;
                string keyedName;
                object questDef = null;
                string questName = string.Empty;
                if (questPawn.questsAndIncidents[index] is QuestScriptDef questScriptDef)
                {
                    defname = questScriptDef.defName;
                    keyedName = "RimQuest_" + defname;
                    if (keyedName.Translate() == keyedName)
                    {
                        if (defname.Contains("_"))
                        {
                            defname = questScriptDef.defName.Split('_')[1];
                        }
                        if (questScriptDef.defName.Contains("Hospitality"))
                        {
                            defname = questScriptDef.defName.Replace("_", " ");
                        }
                        questName = Regex.Replace(defname, "(\\B[A-Z])", " $1");
                    }
                    else
                    {
                        questName = keyedName.Translate();
                    }
                    questDef = questScriptDef;
                }
                if (questPawn.questsAndIncidents[index] is IncidentDef incidentDef)
                {
                    defname = incidentDef.defName;
                    questName = incidentDef.LabelCap;
                    questDef = incidentDef;
                }
                if (string.IsNullOrEmpty(questName))
                {
                    continue;
                }

                Rect rect6 = new Rect(24f,
                    (viewRect.height - CalcOptionsHeight(width)) +
                    (Verse.Text.CalcHeight(questName, width) + 12f) * index + 8f, viewRect.width / 2f,
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
            if (Widgets.ButtonText(new Rect(0f, inRect.height - ButtonHeight, inRect.width / 2f - 20f, ButtonHeight),
                "CancelButton".Translate(), true, false, true))
            {
                Close(true);
            }
            if (actualPlayerSilver >= actualSilverCost)
            {
                if (selectedQuest != null &&
                    Widgets.ButtonText(
                        new Rect(inRect.width / 2f + 20f, inRect.height - ButtonHeight, inRect.width / 2f - 20f, ButtonHeight),
                        "Confirm".Translate() + " (" + "RQ_SilverAmt".Translate(actualSilverCost) + ")", true, false, true))
                {
                    if (selectedQuest is QuestScriptDef questDef)
                    {
                        var incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.GiveQuest, Find.World);
                        var storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) =>
                                x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
                        incidentParms = storytellerComp.GenerateParms(IncidentCategoryDefOf.GiveQuest, incidentParms.target);
                        QuestUtility.SendLetterQuestAvailable(QuestUtility.GenerateQuestAndMakeAvailable(questDef, incidentParms.points));
                    }
                    if (selectedQuest is IncidentDef incidentDef)
                    {
                        IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.World);
                        if (incidentDef.pointsScaleable)
                        {
                            StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) =>
                                x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
                            incidentParms = storytellerComp.GenerateParms(incidentDef.category, incidentParms.target);
                        }
                        incidentDef.Worker.TryExecute(incidentParms);
                    }
                    var questPawns = Find.World.GetComponent<RimQuestTracker>().questPawns;
                    if (questPawns != null && questPawns.Contains(questPawn))
                        questPawns.Remove(questPawn);
                    SoundDefOf.ExecuteTrade.PlayOneShotOnCamera(null);
                    ReceiveSilver(questPawn.pawn, actualSilverCost);
                    Close(true);
                    Find.WindowStack.Add(new Dialog_MessageBox(
                        "RQ_QuestDialogTwo".Translate(questPawn.pawn.LabelShort, interactor.LabelShort)
                            .AdjustedFor(questPawn.pawn), "OK".Translate(), null, null, null, title));
                }
            }
            else
            {
                if (Widgets.ButtonText(
                    new Rect(inRect.width / 2f + 20f, inRect.height - ButtonHeight, inRect.width / 2f - 20f, ButtonHeight),
                    "RQ_LackFunds".Translate(), true, false, true))
                {
                    SoundDefOf.ClickReject.PlayOneShotOnCamera(null);
                    Messages.Message("RQ_LackFundsMessage".Translate(), MessageTypeDefOf.RejectInput);
                }
            }
        }

        public void ReceiveSilver(Pawn receiver, int amountOwed)
        {
            int amountUnpaid = amountOwed;
            List<Thing> currencies = receiver.Map.listerThings.ThingsOfDef(ThingDefOf.Silver);
            if (currencies != null && currencies.Count > 0)
            {
                foreach (Thing currency in currencies.InRandomOrder())
                {
                    if (amountUnpaid <= 0)
                    {
                        break;
                    }
                    int num = Math.Min(amountUnpaid, currency.stackCount);
                    currency.SplitOff(num).Destroy(DestroyMode.Vanish);
                    amountUnpaid -= num;
                }
            }
            Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver, null);
            thing.stackCount = amountOwed;
            receiver.inventory.TryAddItemNotForSale(thing);
        }

        private float CalcHeight(float width)
        {
            var result = Verse.Text.CalcHeight(Text, width);
            return result;
        }

        private float CalcOptionsHeight(float width)
        {
            var result = 0f;
            foreach (var quest in questPawn.quests)
            {
                result += Verse.Text.CalcHeight(quest.label, width);
            }
            foreach (var incident in questPawn.incidents)
            {
                result += Verse.Text.CalcHeight(incident.letterLabel, width);
            }
            return result;
        }
    }
}
