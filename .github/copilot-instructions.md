# GitHub Copilot Instructions for RimQuest (Continued)

## Mod Overview and Purpose

RimQuest (Continued) is an update to the original mod by Jecrell, designed to enhance the experience of quest-giving in RimWorld. By integrating quest givers into trader groups and caravans, players can enjoy a dynamic and enriched gaming experience with additional objectives. The mod emphasizes providing players with more direction through quests, making use of both vanilla events and newly planned quests.

## Key Features and Systems

- **Quest Givers in Traveling Groups**: Adds quest givers to caravans and trader groups. Interact with characters marked with a green exclamation to receive quests.
- **Integration with Orions Hospitality Mod**: Supports guest groups for a more nuanced interaction.
- **Customizable Quest Settings**: Offers settings to modify available quests and incidents for purchase, base price adjustments, and quest-giver spawn chances.
- **Multilingual Support**: Includes translations for Russian, French, Korean, and Japanese players.
- **Performance Optimization**: Enhancements for optimized gaming performance.

## Coding Patterns and Conventions

- **File and Class Organization**: The mod's source code is structured into clear functional areas, like `IncidentWorker_VisitorGroup_GiveItems` and `FloatMenuOptionProvider_GetQuest`, adhering to the single responsibility principle.
- **Consistent Naming**: Classes and members use PascalCase for naming, enhancing readability and maintainability.
- **C# Modularity**: Uses partial class definitions to isolate components, improving modularity.

## XML Integration

- **XML Definitions**: Uses XML files to define different game components such as `JobDef` and `QuestGiverDef`.
- **Defining Jobs and Quests**: XML files such as `RQ_Jobs.xml` and `QuestGiverDefs.xml` contain definitions that add new jobs and quest-giving NPCs, modifying how they interact within the game.
- **Patches and Extensions**: XML patches allow for modifying core game functionalities and integrating new features provided by mods.

## Harmony Patching

- **Dependency Integration**: Utilizes `brrainz.harmony` to apply patches, ensuring compatibility and extended functionality. 
- **Effective Patch Management**: Classes such as `HospitalityPatch` demonstrate how Harmony prefixes are used to augment, modify, or extend game behavior without altering base game files.

## Suggestions for Copilot

When using GitHub Copilot, consider the following suggestions to enhance your code development for RimQuest:

1. **Leverage Codex Integrations**: Use Copilot's ability to scan existing Codex data to provide contextual instruction and code suggestions based on the integration needs with the Hospitality mod or quest system.

2. **Repeatable Patterns**: Employ Copilot in recognizing and suggesting repetitive code structures seen in `Dialog_QuestGiver` for creating dialog boxes or interaction windows in other parts of the mod.

3. **Utilize Language Abstraction**: For multilingual updates, use Copilot to automate and suggest translation string headers based on existing patterns seen in provided language files.

4. **Optimize Performance**: Encourage Copilot to identify potential areas for optimization within loop structures or frequent method calls for better mod performance, following patterns established by `Taranchuk's optimizations`.

5. **Testing and Debugging Assistance**: Use Copilot to generate test scenarios for quirks specific to quest functionality, including spawning, dismissing, and re-evaluating quest states.

6. **Comprehensive Documentation**: Enable Copilot to auto-generate function and class summaries based on preceding existing code for self-documentation purposes.

By following these instructions, developers can effectively use GitHub Copilot to enhance and streamline the development of the RimQuest mod. Happy modding!

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

