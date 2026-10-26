# MP2B: mine, sell, defend

Open `Assets/Scenes/MineDefense.unity` in the shared repository on branch `BillyTD`.

## What changed

- One saved scene containing `Drape_Cave`, `Billy_DefenseRoom`, and `ConnectingPassage`. No runtime scene loading.
- Drape's XR rig and wrist inventory are retained. The imported room's rig and EventSystem are removed from the combined scene.
- A passage near the cave selling area leads through a 3m opening in the room. Two boundary wall pieces, a wooden panel, and a protruding cave foundation are disabled only here. The room's old invisible front boundary is also disabled.
- `PlayerResources.sharedEconomy` references Drape's existing `EconomyManager`. All purchases, upgrades, recycling, wave rewards and ore sales therefore share one balance. There is no new runtime manager or runtime script.
- The combined game starts with the cave's existing zero gold. Mine and sell to buy the first tower. Scrap still exists as physical loot, but recycling pays gold. Existing tower prices are unchanged.
- Tower currency messages now say GOLD. Most touched scripts contain only this text change.
- WristInventoryUI now subscribes when InventoryManager becomes available, avoiding an Awake/OnEnable ordering problem.
- Original GameScene and RoomScaleDefense files and prefab models are unchanged. Standalone RoomScaleDefense retains its serialized starting balance through FormerlySerializedAs.

## Unity test (stop here before adding progression)

1. Open MineDefense and Play with the real headset. Confirm head, controller, locomotion, and wrist inventory inputs.
2. Mine a deposit. Confirm ore count rises on the wrist display. Select the sell cart and confirm the inventory empties and gold rises.
3. Walk through the signed passage near the selling area into the defense room and back. Check doorway clearance, floor transitions and controller movement.
4. Earn at least 20 gold, then grab a tower. The cave gold display and room/display balance must decrease together by 20.
5. Place the tower on a free cell; start a wave with the existing button. Confirm rats follow the moved path and the tower fires.
6. Earn enough gold for a hammer upgrade (15 first upgrade). Confirm both balances change together. Recycle a purchased tower for +10 and confirm the sale station reflects the same gold.
7. Return to the mine, mine/sell again, then return to the room. Everything should remain in its current state; there is no scene transition or reset.
8. Test the retained lantern and toolbox after the main loop is confirmed.

## Validation completed

Unity batch compilation and scene creation completed successfully. Automated checks verified one XR Origin, one EventSystem, one economy, five assigned display prefabs, intact enemy waypoints, no missing script components, shared income/spending/overspend rejection, and capsule clearance plus floor coverage on three lines through the connecting passage. These checks do not establish headset controls, visual quality, or the entire gameplay loop.

Unity still reports the pre-existing unconfigured Unity Version Control client. Git remains the collaboration workflow; no Plastic configuration was changed. Unity also attempted to switch the project-level XR settings references during import; that unrelated EditorBuildSettings change was restored. No build-profile or XR settings changes were intentionally included.

## Editing and submission

Restyle the room under `Billy_DefenseRoom`; do not individually move waypoints or resource objects away from the group. The signed passage is intentionally simple placeholder geometry for your cave-themed visual pass.

`BuildMineDefense` is one Editor-only construction/check tool; it is not needed to play. It refuses to overwrite the combined scene once it exists. Do not rebuild after decorating.

No commits, pushes, PRs, or changes to main were made. Review and headset-test on BillyTD first. If exporting a build, explicitly include MineDefense in the chosen build profile; this task did not change the startup scene.

This is the first cross-contribution gameplay connection, not a completed MP2B scoring submission. Threshold unlocks, reveal order, sandbox lessons, and achievement trophies are intentionally deferred until this loop works.
