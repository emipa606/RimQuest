using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimQuest
{
    public class QuestGiverDef : Def
    {
        public readonly bool anyQuest = false;
        public readonly int maxOptions = 3;
        public List<FactionDef> factions;
        public List<IncidentGenOption> quests;
        public List<QuestGenOption> questsScripts;
        public List<string> tags;
        public List<TechLevel> techLevels;
    }
}