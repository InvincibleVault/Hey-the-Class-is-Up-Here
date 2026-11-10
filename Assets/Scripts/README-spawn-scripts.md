# Object spawning scripts (capstone 3D prototype)

## Files

- `CapstoneObjectSpawner.cs`: the spawner. One per scene. Holds the prefab list,
  the spawn point, and the spawn/clear logic.
- `SpawnButton.cs`: one per UI button. Each button spawns exactly one
  object, so the button layout is the spawn menu.

## Setup (5 minutes, in Unity)

1. Copy both scripts into the project's `Assets/Scripts/` folder.
2. Create an empty GameObject (name it "Spawner") and add
   `CapstoneObjectSpawner` to it.
3. Optional: create an empty GameObject as a spawn point, drag it into
   the spawner's Spawn Point field. Otherwise objects spawn at the
   spawner's own position.
4. For each UI button, add `SpawnButton`, drag the Spawner object into
   its Spawner field, then either drag a prefab into Object To Spawn
   or set Prefab Index to match the spawner's list order.
5. Press Play and click the buttons. Objects appear at the spawn point.

## Adding a new object later

Either add its prefab to the spawner's Spawnable Prefabs list and point
a button at that index, or drag the prefab straight onto a new button's
Object To Spawn field. No code changes needed either way.

## Notes

- `ClearSpawned()` only works when a Spawned Parent is set. Wire it to
  a "Clear" button if the prototype needs a reset.
- Position Jitter and Randomize Rotation Y are off by default (zero /
  false). Turn them on in the inspector if spawned objects should not
  stack exactly on top of each other.
- These scripts were written against standard Unity conventions and
  have not been compiled inside the project yet. Open them in Unity and
  confirm there are no conflicts with existing script or prefab names
  before pushing.
