# Mining and defense guidance

Open `Assets/Scenes/MineDefense.unity` and start a fresh Play session.

The cyan ground trail suggests the next task. It never locks the wave button or moves the player. Press **Y on the left controller** or **G on the keyboard** to hide/show guidance anywhere. The wrist inventory displays the guide's ON/OFF state. The old room-only button is disabled during gameplay.

1. Mine with the pickaxe until the inventory is worth 40 Gold. The trail should switch to the selling cart. Sell, then follow it to the basic tower shelf.
2. Grab a tower, release it on an empty green cell, and start wave 1. The trail should disappear during combat.
3. After the wave, buy a toolbox and deliver it to A. If short of Gold, the trail leads to mining, then selling first.
4. With A open, the guide suggests funding an upgrade using its richer ore. Pick up the hammer and strike the indicated placed tower. Then follow the guide to the wave button. You may also build an extra tower or start a wave early.
5. Try B with A open but only three waves cleared: no toolbox should be consumed or counted; its label should explain the 3/4 requirement.
6. Clear wave 4, then buy/deliver three toolboxes to B. Watch the counter and reveal. Subsequent mining guidance uses B's richer area.
7. Hover a placed tower to check its upgrade cost; confirm the actual Gold deduction matches. Level two costs twice level one.

| Machine | Purchase | First upgrade | Second upgrade |
|---|---:|---:|---:|
| Rapid / Bottle Cap | 80 | 120 | 240 |
| Long-range / Spring Punch | 110 | 140 | 280 |
| Soda sprayer | 140 | 160 | 320 |
| Cooling fan | 100 | 120 | 240 |
| Amplifier | 180 | 180 | 360 |

Starting Gold stays 200. Toolboxes stay 50. Ore sale values and combat rewards are unchanged. This is a first balance pass; test waves 1–4 before recording.

Implementation: `CaveDefenseProgression` selects the task from existing game state. `CaveGuideTrail` draws one line along Inspector waypoint arrays. The `GroundGuidance` child of `ProgressFeedback` holds those waypoints; adjust them if the room or cave moves. Editor setup is manual (`Tools > MP2C > Set Up Ground Guidance and Prices`); the scene is already configured.

Verification: the automated Play-mode smoke test simulates wave counts and toolbox purchase/overlap. It checks guide switching and hiding, the B gate, exact upgrade deductions, and existing unlock/error/trophy behavior. It does not test tracked controllers or combat balance. For recording, use normal gameplay rather than the smoke test.
