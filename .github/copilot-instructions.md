Here is a detailed `.github/copilot-instructions.md` file tailored for your RimWorld modding project in C#:

---

# GitHub Copilot Instructions for RimWorld Modding Project

## Mod Overview and Purpose
The purpose of this project is to enhance the hospitality and quest systems in RimWorld, allowing players to interact more dynamically with visitors and explore comprehensive quest lines. The project focuses on improving interaction mechanics between the player's faction and non-player visitor groups, as well as expanding the quest generation framework.

## Key Features and Systems

**Hospitality Patch System:**
- Enhances the interactions with visitor groups by patching how incidents related to visitors are generated and processed.
- `Prefix_IncidentWorker_VisitorGroup` aims to modify or extend the default behavior when visitor incidents occur.

**Quest System Enhancements:**
- The `RimQuest` series of classes build an integrated quest generation system, offering custom quests and incidents.
- `Dialog_QuestGiver` provides a window interface for players to accept and manage quests.
- The system allows for generating quests dynamically based on various game states and player decisions.

## Coding Patterns and Conventions

- The project adheres to the .NET Framework, specifically targeting versions 4.6.1 to 4.8.1.
- Classes are categorized into logical components (e.g., `QuestGiverDef`, `IncidentGenOption`) to maintain modularity.
- Methods are generally scoped to the minimal necessary access level, with a preference for `private` where possible.
- Use of XML configuration for defining data-driven aspects such as quests and incidents.

## XML Integration

- XML files are used extensively to configure data such as definitions for quests and associated incidents.
- Example: `IncidentGenOption` and `QuestGenOption` both parse data from XML using `LoadDataFromXmlCustom(XmlNode xmlRoot)`.
- XML provides flexibility in adjusting parameters without code recompilation, ideal for mod settings or data driven implementations.

## Harmony Patching

- This project employs Harmony for runtime modification of assemblies, allowing for extensive customization of game behavior.
- `HarmonyPatches` class is designed to contain static patches that modify or extend the game's internal methods.
- Patching is used to adjust interactions with visitors and quests without modifying the original game source code directly.

## Suggestions for Copilot

1. **For Method Completion:**
   - Use method names and parameters as cues to generate method bodies. For example, `DetermineSilverCost` should compute values based on given game logic.

2. **For XML Data Parsing:**
   - Assist in generating XML parsing logic for methods like `LoadDataFromXmlCustom` by suggesting structured ways to read and handle XML nodes.

3. **For Harmony Patching:**
   - Offer patch method templates, including prefix, postfix, or transpiler templates, to mod known methods such as `IncidentWorker` methods in RimWorld.

4. **For User Interface Development:**
   - Provide suggestions for `Dialog_QuestGiver`, guiding UI elements and interactions driven by player's choice.

5. **In Code Documentation:**
   - Assist in generating inline comments and method summaries that reflect the logic and purpose of code blocks, enhancing maintainability and understanding.

---

This document should serve as both a guideline and a support resource for contributors using GitHub Copilot to engage with the modding project efficiently.
