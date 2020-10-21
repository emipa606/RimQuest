using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimQuest
{
    public class QuestGiverDef : Def
    {
        public List<FactionDef> factions;
        public List<TechLevel> techLevels;
        public int maxOptions = 3;
        public bool anyQuest = false;
        public List<QuestGenOption> questsScripts;
        public List<IncidentGenOption> quests;
        public List<string> tags;
    }
}