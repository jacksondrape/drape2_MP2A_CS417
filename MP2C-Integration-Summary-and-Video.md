# Design Integrations Summary

**Game:** Mine & Defend (working title)  
**Team Number:** 8  
**Contributors covered:** Billy Lu and Jackson Drape — add any other contributing teammates before submission.  
**GitHub Repository:** https://github.com/jacksondrape/drape2_MP2A_CS417  
**Final Build Link:** [Insert group build link]

Jackson's mining and toolbox systems connect with Billy's tower-defense systems in one scene. Players mine ore, sell it for shared Gold, and spend Gold on machines, upgrades, and toolboxes. Clearing defense waves also unlocks workshop machines and makes entrance B available, connecting progress in both areas.

## Learning Curve — self-evaluation: 4/4

1. **Mine and sell:** The pickaxe suggests striking ore, and the selling cart converts collected ore into Gold. The ground guide starts with mining, then directs players to sell. **Timestamp:** [Insert]
2. **Build and defend:** Players extend this familiar earn-and-spend loop by grabbing priced machines from shelves and placing them on green grid cells. Players choose when to start a wave, leaving time to learn placement. **Timestamp:** [Insert]
3. **Expand and improve:** Selling 50 Gold of ore reveals the soda sprayer; clearing waves 1 and 3 reveals the fan and amplifier. One toolbox opens A. B requires A open, four cleared waves, and three toolboxes. Higher upgrade costs encourage returning to richer mining areas. **Timestamps:** [Insert for each unlock]

Introductions appear one at a time, and the guide suggests one immediate task. The exact best tower arrangement remains open for experimentation; attack effects, airflow, and amplifier links help players judge combinations without prescribing a solution.

## Discoverability — self-evaluation: 3/4

The mining-workshop metaphor connects recognizable physical actions: strike ore with a pickaxe, buy machines from shelves, improve them with a hammer, and recycle them in a dumpster. Shelf prices, placement colors, hover explanations, and entrance counters clarify these actions. **Timestamps:** [Insert for mining, shelves, hammer, and recycling]

The cyan trail and nearby task text connect the cave and defense room. Locked shelves reveal additional options gradually, while entrance requirements explain how to reach new areas. We leave optimal layouts for discovery, but make the required actions explicit. Some interactions still depend on text, so we rate discoverability 3 rather than 4. **Timestamps:** [Insert]

## Tolerance — self-evaluation: 2/2

We address these predictable misunderstandings:

- **Upgrading before placement:** The hammer asks players to place the machine first. **Timestamp:** [Insert]
- **Upgrading without enough Gold:** Feedback states the required amount and directs players to mine and sell ore. **Timestamp:** [Insert]
- **Placing two machines on one cell:** Placement is rejected with an occupied-cell message; the purchased machine remains available to reposition. **Timestamp:** [Insert]
- **Opening B too early:** The entrance explains the missing requirement and does not consume the purchased toolbox. **Timestamp:** [Insert]

These responses explain a corrective action and allow another attempt.

## Focusing Feedback — self-evaluation: 2/2

Immediate feedback includes Gold changes, placement colors, upgrade messages and sparks, projectile impacts, and enemy health bars. Longer-term feedback includes wave status, entrance counters, unlock introductions, and achievement trophies. **Timestamps:** [Insert]

The guide shows one task, can be hidden, and disappears during combat and workshop introductions. Temporary messages keep explanations close to the relevant interaction. Combat remains visually busy, but enemy health and wave status show its outcome. Amplifier charging indicators and links signal the delay before its damage boost, while entrance introductions precede the opening. **Timestamps:** [Insert]

**Proposed design self-evaluation: 11/12.** Self Awareness is a separate 2 points, awarded only if this evaluation is within two points of the grader's assessment. **Target: 13/14, not a confirmed score.** Adjust the evaluation to match the final recorded build.

---

# Video recording script

Suggested edited length: about 5–6 minutes, unless the course specifies otherwise. These are planned sections, not real timestamps. Replace all summary placeholders after editing. Show actual gameplay; trim repeated mining, travel, and combat while keeping the relevant counters and before/after results visible.

## 1. Introduction — about 20 seconds

**Show:** Cave, connecting passage, and defense room in the same scene.

**Say:** “This is our integrated mining and tower-defense game. Jackson contributed the mining and toolbox systems, and Billy contributed the tower-defense systems and progression feedback. Mining funds our defenses, and defense progress helps unlock better mining areas.”

## 2. Mine, sell, and follow the guide — about 40 seconds

**Show:** Hit ore with the pickaxe. Show inventory increasing and the cyan guide changing to the cart once enough ore is carried. Sell at least 50 Gold of ore and show the Gold increase and soda introduction.

**Say:** “We start with a familiar action: use the pickaxe to mine ore. The ground guide then points to the selling cart. Selling increases the same Gold balance used to buy towers. Selling fifty Gold of ore also introduces the soda sprayer, adding a new option after the basic economy is understood.”

## 3. Build and recover from mistakes — about 50 seconds

**Show:** Shelf price, purchase deduction, hammer on an unplaced machine, successful grid placement, then attempt to place another purchased machine on the occupied cell. Move it to a free cell.

**Say:** “Players buy machines by grabbing them from the shelf and activate them by placing them on a free grid cell. Prices and placement colors explain the rules. Trying to upgrade before placement gives a corrective message. An occupied cell also rejects placement without deleting the purchased tower, so we can move it and try again.”

## 4. Defend and open A — about 45 seconds

**Show:** Start wave 1; trail hides. Show shooting, health bars, wave completion, fan introduction, and First Defense trophy. Buy one toolbox and deliver it to A; hold on the introduction and opening.

**Say:** “Players start each wave when ready, so building and learning are not timed. The guide hides during combat. Attacks and health bars show the immediate results, while wave completion introduces the fan. A purchased toolbox opens entrance A, giving us another mining area to explore.”

## 5. Mine richer ore and upgrade — about 45 seconds

**Show:** An insufficient-Gold upgrade attempt and its message. Mine in A, sell, use the hammer successfully, and show the changed stats and Gold deduction. Briefly place the fan or amplifier beside attack towers and show its effect. If necessary, record the amplifier after wave 3 as a separate clip.

**Say:** “Upgrades now cost more, giving richer ore a practical purpose. If we cannot afford an upgrade, the message tells us what is missing and where to earn it. After mining and selling, the hammer improves the machine. Nearby support machines add another decision: we can experiment with layouts and see their effects through airflow and damage-boost links.”

## 6. Wave 3 and entrance B — about 55 seconds

**Show:** Wave-3 completion and amplifier reveal. With A open and only three waves cleared, attempt a B delivery. Show the 3/4 requirement and retained toolbox. Cut to actual wave-4 completion, then deliver three purchased toolboxes. Show the counter, introduction, and opening.

**Say:** “Wave three reveals the amplifier. Entrance B still requires four cleared waves, and an early delivery explains that requirement without consuming our toolbox. After clearing wave four, we deliver three toolboxes to open B. This creates a reason to return to tower defense before exploring the next mining area.”

## 7. Feedback and conclusion — about 30 seconds

**Show:** Reposition a tower, recycle a purchased tower and show the refund, press Y on the left controller to toggle the guide off and on from the cave, show its wrist status, and finish on the objective board and earned trophies. G is the keyboard alternative.

**Say:** “Machines can be repositioned, and recycling returns part of their cost. The guide is optional, while the board and trophies show longer-term progress. Our design self-evaluation is eleven out of twelve: four for learning curve, three for discoverability, two for tolerance, and two for focusing feedback. We still rely on text for some interactions and need further player testing for balance.”

## Before recording

- Resolve or verify the outstanding controller issue so the physical interactions can be demonstrated. The rig investigation was interrupted before a cause was confirmed.
- Use the final gameplay build, not the automated smoke test or Inspector-adjusted wave counts.
- Keep error messages, prices, counters, and unlock introductions readable on screen.
- Add the final build link, complete team list, and real timestamps. Remove any claim you cannot demonstrate.
