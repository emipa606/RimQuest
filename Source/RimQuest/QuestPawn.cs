using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimQuest
{
    public class QuestPawn : IExposable
    {
        public Pawn pawn;
        public QuestGiverDef questGiverDef;
        public List<QuestScriptDef> quests;
        public List<IncidentDef> incidents;
        public List<object> questsAndIncidents;

        public QuestPawn()
        {
        }

        public QuestPawn(Pawn pawn, QuestGiverDef questGiverDef, List<QuestScriptDef> quests, List<IncidentDef> incidents)
        {
            this.pawn = pawn;
            this.questGiverDef = questGiverDef;
            this.quests = quests;
            this.incidents = incidents;
        }

        public QuestPawn(Pawn pawn)
        {
            this.pawn = pawn;
            var pawnFaction = pawn.Faction.def;
            if (pawnFaction == null) Log.Error("Factionless quest giver.");
            GenerateQuestGiver(pawnFaction);
            if (questGiverDef == null) Log.Error("No quest giver found.");
            GenerateAllQuests();
            GenerateAllIncidents();
            GenerateQuestsAndIncidents();
        }

        private void GenerateQuestGiver(FactionDef pawnFaction)
        {
            questGiverDef = DefDatabase<QuestGiverDef>.AllDefs.FirstOrDefault(x =>
                (x.factions != null && x.factions.Contains(pawnFaction)) ||
                (x.techLevels != null && x.techLevels.Contains(pawnFaction.techLevel)));
        }


        private void GenerateAllQuests()
        {
            quests = new List<QuestScriptDef>();
            if (!questGiverDef.anyQuest && questGiverDef.questsScripts == null)
            {
                return;
            }
            List<QuestGenOption> result = new List<QuestGenOption>();
            if (!questGiverDef.anyQuest)
            {
                foreach (var questGen in new List<QuestGenOption>(questGiverDef.questsScripts))
                {
                    result.Add(questGen);
                }
            }
            else
            {
                foreach (var def in DefDatabase<QuestScriptDef>.AllDefsListForReading.Where(IsAcceptableQuest))
                {
                    result.Add(new QuestGenOption(def, 100));
                }
            }
            for (int i = 0; i < questGiverDef.maxOptions; i++)
            {
                if (result.TryRandomElementByWeight(x => x.selectionWeight, out var quest))
                {
                    quests.Add(quest.def);
                    result.Remove(quest);
                }
            }
        }

        private void GenerateAllIncidents()
        {
            incidents = new List<IncidentDef>();
            if(!questGiverDef.anyQuest && questGiverDef.quests == null)
            {
                return;
            }
            List<IncidentGenOption> result = new List<IncidentGenOption>();
            if (!questGiverDef.anyQuest)
            {
                foreach (var incidentGen in new List<IncidentGenOption>(questGiverDef.quests))
                {
                    result.Add(incidentGen);
                }
            }
            else
            {
                foreach (var def in DefDatabase<IncidentDef>.AllDefsListForReading.Where(IsAcceptableIncident))
                {
                    result.Add(new IncidentGenOption(def, 100));
                }
            }
            for (int i = 0; i < questGiverDef.maxOptions; i++)
            {
                if (result.TryRandomElementByWeight(x => x.selectionWeight, out var incident))
                {
                    incidents.Add(incident.def);
                    result.Remove(incident);
                }
            }
        }

        private void GenerateQuestsAndIncidents()
        {
            questsAndIncidents = new List<object>();
            List<object> tempListToChooseFrom = (from quest in quests select quest as object).ToList();
            tempListToChooseFrom.AddRange(from incident in incidents select incident as object);

            for (int i = 0; i < questGiverDef.maxOptions; i++)
            {
                var objectToAdd = tempListToChooseFrom.RandomElement();
                questsAndIncidents.Add(objectToAdd);
                tempListToChooseFrom.Remove(objectToAdd);
            }
        }

        private bool IsAcceptableQuest(QuestScriptDef x)
        {
            return (x.defName.Contains("OpportunitySite_") ||
                   (x.defName.Contains("Hospitality_") && !x.defName.Contains("Util_"))) &&
                   (x.GetModExtension<RimQuest_ModExtension>()?.canBeARimQuest ?? true); //mod extension value if not null, otherwise assumed true.
        }

        private bool IsAcceptableIncident(IncidentDef x)
        {
            return x.targetTags.Contains(IncidentTargetTagDefOf.World) && x.letterDef != LetterDefOf.NegativeEvent &&
                   x.defName != "JourneyOffer" &&
                   x.defName != "CultIncident_StarsAreWrong" &&
                   x.defName != "CultIncident_StarsAreRight" &&
                   x.defName != "HPLovecraft_BloodMoon" &&
                   x.defName != "Aurora" &&
                   !x.defName.Contains("GiveQuest") &&
                   (x.GetModExtension<RimQuest_ModExtension>()?.canBeARimQuest ?? true); //mod extension value if not null, otherwise assumed true.
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref this.pawn, "pawn");
            Scribe_Defs.Look(ref this.questGiverDef, "questGiverDef");
            Scribe_Collections.Look<QuestScriptDef>(ref this.quests, "quests", LookMode.Def);
            Scribe_Collections.Look<IncidentDef>(ref this.incidents, "incidents", LookMode.Def);
        }
    }
}