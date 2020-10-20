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

        public QuestPawn()
        {
        }
        
        public QuestPawn(Pawn pawn, QuestGiverDef questGiverDef, List<QuestScriptDef> quests)
        {
            this.pawn = pawn;
            this.questGiverDef = questGiverDef;
            this.quests = quests;
        }
        
        public QuestPawn(Pawn pawn)
        {
            this.pawn = pawn;
            var pawnFaction = pawn.Faction.def;
            if (pawnFaction == null) Log.Error("Factionless quest giver.");
            GenerateQuestGiver(pawnFaction);
            if (questGiverDef == null) Log.Error("No quest giver found.");
            GenerateQuests();
        }

        private void GenerateQuestGiver(FactionDef pawnFaction)
        {
            questGiverDef = DefDatabase<QuestGiverDef>.AllDefs.FirstOrDefault(x =>
                (x.factions != null && x.factions.Contains(pawnFaction)) ||
                (x.techLevels != null && x.techLevels.Contains(pawnFaction.techLevel)));
        }

        private void GenerateQuests()
        {
            quests = new List<QuestScriptDef>();            
            var tempQuestsFromDef = questGiverDef.anyQuest ? GenerateAllQuests() : new List<QuestGenOption>(questGiverDef.quests);
            for (int i = 0; i < questGiverDef.maxOptions; i++)
            {
                if (tempQuestsFromDef.TryRandomElementByWeight(x => x.selectionWeight, out var quest))
                {
                    quests.Add(quest.def);
                    tempQuestsFromDef.Remove(quest);
                }
            }
        }

        private List<QuestGenOption> GenerateAllQuests()
        {
            List<QuestGenOption> result = new List<QuestGenOption>();
            foreach (var def in DefDatabase<QuestScriptDef>.AllDefsListForReading.Where(IsAcceptableQuest))
            {
                result.Add(new QuestGenOption(def, 100));
            }
            return result;
        }

        private bool IsAcceptableQuest(QuestScriptDef x)
        {
            return (x.defName.Contains("OpportunitySite_") || 
                   (x.defName.Contains("Hospitality_") && !x.defName.Contains("Util_"))) &&
                   (x.GetModExtension<RimQuest_ModExtension>()?.canBeARimQuest ?? true); //mod extension value if not null, otherwise assumed true.
        }


        public void ExposeData()
        {
            Scribe_References.Look(ref this.pawn, "pawn");
            Scribe_Defs.Look(ref this.questGiverDef, "questGiverDef");
            Scribe_Collections.Look(ref this.quests, "quests", LookMode.Def);
        }
    }
}