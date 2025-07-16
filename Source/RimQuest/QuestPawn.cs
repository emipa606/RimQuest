using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimQuest;

public class QuestPawn : IExposable
{
    private List<IncidentDef> incidents;
    public Pawn pawn;
    private QuestGiverDef questGiverDef;
    private List<QuestScriptDef> quests;
    public List<object> questsAndIncidents = [];

    public QuestPawn()
    {
    }

    public QuestPawn(Pawn pawn, QuestGiverDef questGiverDef, List<QuestScriptDef> quests,
        List<IncidentDef> incidents)
    {
        this.pawn = pawn;
        this.questGiverDef = questGiverDef;
        this.quests = quests.Where(def => Main.Quests[def]).ToList();
        this.incidents = incidents.Where(def => Main.Incidents[def]).ToList();
    }

    public QuestPawn(Pawn pawn)
    {
        this.pawn = pawn;
        var pawnFaction = pawn.Faction.def;
        if (pawnFaction == null)
        {
            Log.Error("Factionless quest giver.");
        }

        generateQuestGiver(pawnFaction);
        if (questGiverDef == null)
        {
            Log.Error("No quest giver found.");
        }

        generateAllQuests();
        generateAllIncidents();
        if (!quests.Any() && !incidents.Any())
        {
            generateAllQuests(true);
            generateAllIncidents(true);
        }

        GenerateQuestsAndIncidents();
        if (!questsAndIncidents.Any())
        {
            Log.Warning("No quests generated");
        }
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref pawn, "pawn");
        Scribe_Defs.Look(ref questGiverDef, "questGiverDef");
        Scribe_Collections.Look(ref quests, "quests", LookMode.Def);
        Scribe_Collections.Look(ref incidents, "incidents", LookMode.Def);
    }

    private void generateQuestGiver(FactionDef pawnFaction)
    {
        questGiverDef = DefDatabase<QuestGiverDef>.AllDefs.FirstOrDefault(x =>
            x.factions != null && x.factions.Contains(pawnFaction) ||
            x.techLevels != null && x.techLevels.Contains(pawnFaction.techLevel));
    }


    private void generateAllQuests(bool force = false)
    {
        quests ??= [];

        if (!questGiverDef.anyQuest && questGiverDef.questsScripts == null)
        {
            return;
        }

        var result = new List<QuestGenOption>();
        if (!questGiverDef.anyQuest && !force)
        {
            foreach (var questGen in new List<QuestGenOption>(questGiverDef.questsScripts))
            {
                if (Main.Quests[questGen.def])
                {
                    result.Add(questGen);
                }
            }
        }
        else
        {
            foreach (var def in Main.Quests.Where(pair => pair.Value).Select(pair => pair.Key))
            {
                result.Add(new QuestGenOption(def, 100));
            }
        }

        for (var i = 0; i < questGiverDef.maxOptions; i++)
        {
            if (!result.TryRandomElementByWeight(x => x.selectionWeight, out var quest))
            {
                continue;
            }

            quests.Add(quest.def);
            result.Remove(quest);
        }
    }

    private void generateAllIncidents(bool force = false)
    {
        incidents ??= [];

        if (!questGiverDef.anyQuest && questGiverDef.quests == null)
        {
            return;
        }

        var result = new List<IncidentGenOption>();
        if (!questGiverDef.anyQuest && !force)
        {
            foreach (var incidentGen in new List<IncidentGenOption>(questGiverDef.quests))
            {
                if (Main.Incidents[incidentGen.def])
                {
                    result.Add(incidentGen);
                }
            }
        }
        else
        {
            foreach (var def in Main.Incidents.Where(pair => pair.Value).Select(pair => pair.Key))
            {
                result.Add(new IncidentGenOption(def, 100));
            }
        }

        for (var i = 0; i < questGiverDef.maxOptions; i++)
        {
            if (!result.TryRandomElementByWeight(x => x.selectionWeight, out var incident))
            {
                continue;
            }

            incidents.Add(incident.def);
            result.Remove(incident);
        }
    }

    public void GenerateQuestsAndIncidents()
    {
        questsAndIncidents = [];
        var tempListToChooseFrom = (from quest in quests select quest as object).ToList();
        tempListToChooseFrom.AddRange(from incident in incidents select incident as object);

        for (var i = 0; i < questGiverDef.maxOptions; i++)
        {
            var objectToAdd = tempListToChooseFrom.RandomElement();
            questsAndIncidents.Add(objectToAdd);
            tempListToChooseFrom.Remove(objectToAdd);
        }
    }

    public float CalcOptionsHeight(float width)
    {
        var result = 0f;
        foreach (var quest in quests)
        {
            result += Text.CalcHeight(quest.label, width);
        }

        foreach (var incident in incidents)
        {
            result += Text.CalcHeight(incident.letterLabel, width);
        }

        return result;
    }
}