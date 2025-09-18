# ActionQueue for Unity

ActionQueue is a lightweight, generic action sequencer for Unity. It helps make it easier to manage complex asynchronous call chains. You can use it for animations, network calls, gameplay events, or anything else.

## Installation

### Via Package Manager GUI (Simplest)
Unity → Window → Package Manager → [+] → Add package from git URL…

```
https://github.com/uken/action-queue-unity.git?path=Package#v1.0.0
```

### Via Manifest
Add the following line to your `manifest.json`:
```
"com.uken.actionqueue": "https://github.com/uken/action-queue-unity.git?path=Package#v1.0.0",
```

For Uken internal use, you can instead add the package by name:
```
"com.uken.actionqueue": "1.0.0",
```

If you want to run the package's test suite from within the consuming project, you can also add it to the `"testables"` section, eg:

```
{
  "dependencies": {
    ...
  },
  "testables": [
    "com.uken.actionqueue"
  ]
}
```

---

👉 In all cases, be sure to replace the version number `1.0.0` with the latest version number found in the [`package.json` file](https://github.com/uken/action-queue-unity/blob/master/Package/package.json).

👉 If you are upgrading from the non-packaged version of ActionQueue to `1.0.0+`, there is a small breaking change with `SkippableActionQueue.Enqueue(...)`:
  - The two parameter version's `onSkipped` parameter is now a `Func` that returns a `ValueTuple<T1, T2>` instead of a `Tuple<T1, T2>`.
  - The three parameter version's `onSkipped` parameter is now a `Func` that returns a `ValueTuple<T1, T2, T3>` instead of a custom Triplet class.

## Setup
Add `using Com.Uken.ActionQueue;` and you're ready to go.

## Requirements
Unity 2020.3+ recommended (any .NET 4.x Equivalent runtime).

## License
Apache-2.0. See `LICENSE` for details.

## Usage

### Basic Use:
```csharp
var q = new ActionQueue();

q.Enqueue(resume => {
  // Do something async, then...
  resume();
});

q.Enqueue(resume => {
  // Do something else async, then...
  resume();
});

q.Start();

// (Call `resume` exactly once per step. If it's not called, the queue stays waiting.)
```

For example,

```csharp
public void CharacterDemo() {
  var q = new ActionQueue();

  q.Enqueue(this.WalkToStage);
  q.Enqueue(this.Dance);
  q.Enqueue(this.Bow);

  q.Start();
}

private void WalkToStage(UnityAction onComplete) {
  // onComplete() should be called once this individual action is completed.
}

...
```

Or, something a bit more complicated:

```csharp
public void FightMonster(GameObject monster) {
  var q = new ActionQueue();

  q.Enqueue(this.DrawSword);
  q.Enqueue(resume => this.WalkTo(monster, resume));
  q.Enqueue(resume => this.Attack(monster, resume));

  q.Start();
}

private void DrawSword(UnityAction onComplete) {
  // onComplete() should be called once this individual action is completed.
}

private void WalkTo(GameObject target, UnityAction onComplete) {
  // onComplete() should be called once this individual action is completed.
}

...
```

### Delays:

Delay for two seconds: (pass in a MonoBehaviour to host the coroutine)
```csharp
q.EnqueueDelay(this /* MonoBehaviour */, 2f);
```

### Immediate Actions:

Run an action immediately without needing to call an `onComplete`:
```csharp
q.EnqueueImmediateConsuming(() => { /* Do something... */} );
```

Another example,

```csharp
var q = new ActionQueue();

q.Enqueue(player.DrawBow);
q.Enqueue(player.LoadArrow);
q.Enqueue(player.AimAtTarget);

q.EnqueueDelay(player, 1f);

q.EnqueueImmediateConsuming(logic.DecrementArrowCount);
q.Enqueue(player.ShootArrow);

q.EnqueueDelay(player, 2f);

q.Enqueue(player.LetDownBow);

q.Start();
```

### Conditional Steps:
Execute a step only if the conditional is true. The conditional is evaluated at the time that the step is due to execute.

```csharp
q.Enqueue(player.Cheer, conditional: logic.PlayerIsAlive);
```
```csharp
q.EnqueueImmediateConsuming(player.Cheer, conditional: logic.PlayerIsAlive);
```
```csharp
q.EnqueueDelay(player, 10f, conditional: logic.PlayerIsAlive);
```

This is different than enclosing an enqueue operation in an if statement. In that case, the if statement is evaluated at the time that the step is queued up. For example:

```csharp
bool a = false;

q.EnqueueDelay(this, 10f);
q.EnqueueImmediateConsuming(() => a = true);
if (a) {
  // This does not run, because `a` was set after the if statement was evaluated.
  q.EnqueueImmediateConsuming(() => Debug.Log("`a` is true."));
}
```
```csharp
bool a = false;

q.EnqueueDelay(this, 10f);
q.EnqueueImmediateConsuming(() => a = true);

// This does run, because the conditional is evaluated right before the step is due to execute:
q.EnqueueImmediateConsuming(() => Debug.Log("`a` is true."), conditional: () => a);
```

### Pause, Unpause:
A running queue can be paused (for example, from another asynchronous task); in this case, it will complete its current step but will not move on to the next one. When unpaused, the queue will continue from the next incomplete step.

```csharp
q.Pause();
q.Unpause();
```

```csharp
var q1 = new ActionQueue();
var q2 = new ActionQueue();

q1.EnqueueDelay(this, 5f);
q1.EnqueueImmediateConsuming(() => Debug.Log("Hello world."));

q2.EnqueueDelay(this, 1f);
q2.EnqueueImmediateConsuming(q1.Pause);
q2.EnqueueDelay(this, 10f);
q2.EnqueueImmediateConsuming(q1.Unpause);

q1.Start();
q2.Start();

// After 11 seconds, "Hello world." is printed.
```

### Cancel:
Cancel a running queue. The currently executing step will finish, but subsequent steps will not start.
```csharp
q.Cancel();
```

### Parameters:
Up to three generic parameters can be passed through a queue. `Start()` can be called with initial parameter values, or they can be left empty until populated.

This is useful if you want later steps to depend on the results of earlier ones. For example, if you want to queue up several network operations in a waterfall.

For example, one parameter:

```csharp
var q = new ActionQueue<int>();

q.Enqueue((resume, x) => resume(x + 2));
q.EnqueueImmediateConsuming(x => Debug.Log($"value = {x}"));

q.Start(initialParam: 10);

// "value = 12" is printed.
```

Three parameters:

```csharp
var q = new ActionQueue<int, float, string>();

q.Enqueue((resume, a, b, c) => resume(a + 1, 0.5f, "hi"));

 // Parameters pass through if they are not used:
q.Enqueue(resume => resume());
q.EnqueueImmediateConsuming(() => { /* No-op */ });
q.EnqueueDelay(this, 1f);

q.EnqueueImmediateConsuming((a, b, c) => Debug.Log($"{a}, {b}, {c}"));

q.Start(1); // Not all parameters need to be populated initially.

// After 1 second, "2, 0.5, hi" is printed.
```

A useful example with network calls:

```csharp
// Retrieve the `guildID` for a given `playerID`, then use that to retrieve the list of guild members:
public void GetGuildMembers(Player player, UnityAction<List<Player>> onComplete) {
  var q = new ActionQueue<string, string, List<Player>>();

  q.Enqueue((resume, playerID, _, __)      => GetGuildIDForPlayer(playerID, guildID => resume(playerID, guildID, __)));
  q.Enqueue((resume, playerID, guildID, _) => GetGuildMembers(guildID, guildMembers => resume(playerID, guildID, guildMembers)));
  q.EnqueueImmediateConsuming((playerID, guildID, guildMembers) => onComplete(guildMembers));

  q.Start(player.ID);
}
```

### Skippable Queues:
The `SkippableActionQueue` variant is useful for setting up a queue that can be interrupted. For example, this could be useful if you want to play a multi-part animation that the player can skip by pressing a button.

When setting up a `SkippableActionQueue`, each step requires an `onSkipped` handler. When a queue is skipped, each step, including the currently running step, calls its `onSkipped` handler. If you are playing an animation, you may want `onSkipped` to handle cancelling the animation and setting the object to its completed state. If the step does not require cancelling, you can pass `null` for `onSkipped`.

`SkippableActionQueue` includes the following two additional methods for the control flow:

```csharp
q.EnqueueSkipCheckpoint();
```

```csharp
// Skip to the next checkpoint. If no checkpoint is defined, skip to the end of the queue:
q.SkipToNextCheckpoint();
```

Example:

```csharp
// This sets up an example post-game results scene that the player can skip through:

private SkippableActionQueue q = new SkippableActionQueue();

public void PlayPostGameAnimations() {
  // Show the 'scorePopup':
  q.Enqueue(scorePopup.PlayScoreIncrementAnimation, onSkipped: scorePopup.CancelAndHide);
  q.EnqueueImmediateConsuming(logic.IncrementScore); // Immediate steps always execute, even if skipped
  q.EnqueueSkipCheckpoint();

  // Show the 'achievementsPopup':
  q.Enqueue(achievementsPopup.PlayAchievementEarnedAnimation, onSkipped: achievementsPopup.CancelAndHide);
  q.EnqueueImmediateConsuming(logic.RecordAchievements); // Immediate steps always execute, even if skipped
  q.EnqueueSkipCheckpoint();

  // Finally, show the 'playAgainPopup':
  q.Enqueue(playAgainPopup.ShowWithAnimation, onSkipped: playAgainPopup.ShowImmediately);

  q.Start();
}

public void OnPlayerInput() {
  q.SkipToNextCheckpoint();
}
```

The `SkippableActionQueue` supports all features of `ActionQueue`, including parameters:

```csharp
var q = new SkippableActionQueue<int, string>();

q.Enqueue((resume, a, b) => { /* lock, for illustration */ }, onSkipped: (a, b) => (a + 1, b + "!"));

// onSkipped is `null`, so the parameters will not be incremented if this is skipped:
q.Enqueue((resume, a, b) => resume (a + 1, b + "?"), onSkipped: null);

q.EnqueueSkipCheckpoint();

q.EnqueueImmediateConsuming((a, b) => Debug.Log($"{a}, {b}"));

q.Start(10, "hello");
q.SkipToNextCheckpoint();

// "11, 'hello!'" is printed.
```

## API Overview (Public Surface):
```csharp
using UnityEngine;
using UnityEngine.Events;

namespace Com.Uken.ActionQueue {
  // 0-param
  public class ActionQueue {
    public void Start();
    public void Enqueue(UnityAction<UnityAction> action, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction action, System.Func<bool> conditional = null);
    public void EnqueueDelay(MonoBehaviour mono, float delay, System.Func<bool> conditional = null);
    public void Pause();
    public void Unpause();
    public void Cancel();
    public bool Paused { get; }
  }

  // 1-param
  public class ActionQueue<T> {
    public void Start(T initialParam = default);
    public void Enqueue(UnityAction<UnityAction, T> action, System.Func<bool> conditional = null);
    public void Enqueue(UnityAction<UnityAction<T>, T> action, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction<T> action, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction action, System.Func<bool> conditional = null);
    public void EnqueueDelay(MonoBehaviour mono, float delay, System.Func<bool> conditional = null);
    public void Pause();
    public void Unpause();
    public void Cancel();
    public bool Paused { get; }
  }

  // 2-param
  public class ActionQueue<T1, T2> {
    public void Start(T1 initialParam1 = default, T2 initialParam2 = default);
    public void Enqueue(UnityAction<UnityAction, T1, T2> action, System.Func<bool> conditional = null);
    public void Enqueue(UnityAction<UnityAction<T1, T2>, T1, T2> action, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction<T1, T2> action, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction action, System.Func<bool> conditional = null);
    public void EnqueueDelay(MonoBehaviour mono, float delay, System.Func<bool> conditional = null);
    public void Pause();
    public void Unpause();
    public void Cancel();
    public bool Paused { get; }
  }

  // 3-param
  public class ActionQueue<T1, T2, T3> {
    public void Start(T1 initialParam1 = default, T2 initialParam2 = default, T3 initialParam3 = default);
    public void Enqueue(UnityAction<UnityAction, T1, T2, T3> action, System.Func<bool> conditional = null);
    public void Enqueue(UnityAction<UnityAction<T1, T2, T3>, T1, T2, T3> action, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction<T1, T2, T3> action, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction action, System.Func<bool> conditional = null);
    public void EnqueueDelay(MonoBehaviour mono, float delay, System.Func<bool> conditional = null);
    public void Pause();
    public void Unpause();
    public void Cancel();
    public bool Paused { get; }
  }

  // Skippable (adds onSkipped, checkpoints, skipping)
  public class SkippableActionQueue {
    public void Start();
    public void Enqueue(UnityAction<UnityAction> action, UnityAction onSkipped, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction action, System.Func<bool> conditional = null);
    public void EnqueueDelay(MonoBehaviour mono, float delay, System.Func<bool> conditional = null);
    public void EnqueueSkipCheckpoint(System.Func<bool> checkIfCheckpointIsActive = null);
    public void SkipToNextCheckpoint();
    public void Pause();
    public void Unpause();
    public void Cancel();
    public bool Paused { get; }
  }

  public class SkippableActionQueue<T> {
    public void Start(T initialParam = default);
    public void Enqueue(UnityAction<UnityAction, T> action, UnityAction<T> onSkipped, System.Func<bool> conditional = null);
    public void Enqueue(UnityAction<UnityAction<T>, T> action, System.Func<T, T> onSkipped, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction<T> action, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction action, System.Func<bool> conditional = null);
    public void EnqueueDelay(MonoBehaviour mono, float delay, System.Func<bool> conditional = null);
    public void EnqueueSkipCheckpoint(System.Func<bool> checkIfCheckpointIsActive = null);
    public void SkipToNextCheckpoint();
    public void Pause();
    public void Unpause();
    public void Cancel();
    public bool Paused { get; }
  }

  public class SkippableActionQueue<T1, T2> {
    public void Start(T1 initialParam1 = default, T2 initialParam2 = default);
    public void Enqueue(UnityAction<UnityAction, T1, T2> action, UnityAction<T1, T2> onSkipped, System.Func<bool> conditional = null);
    public void Enqueue(UnityAction<UnityAction<T1, T2>, T1, T2> action, System.Func<T1, T2, (T1, T2)> onSkipped, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction<T1, T2> action, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction action, System.Func<bool> conditional = null);
    public void EnqueueDelay(MonoBehaviour mono, float delay, System.Func<bool> conditional = null);
    public void EnqueueSkipCheckpoint(System.Func<bool> checkIfCheckpointIsActive = null);
    public void SkipToNextCheckpoint();
    public void Pause();
    public void Unpause();
    public void Cancel();
    public bool Paused { get; }
  }

  public class SkippableActionQueue<T1, T2, T3> {
    public void Start(T1 initialParam1 = default, T2 initialParam2 = default, T3 initialParam3 = default);
    public void Enqueue(UnityAction<UnityAction, T1, T2, T3> action, UnityAction<T1, T2, T3> onSkipped, System.Func<bool> conditional = null);
    public void Enqueue(UnityAction<UnityAction<T1, T2, T3>, T1, T2, T3> action, System.Func<T1, T2, T3, (T1, T2, T3)> onSkipped, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction<T1, T2, T3> action, System.Func<bool> conditional = null);
    public void EnqueueImmediateConsuming(UnityAction action, System.Func<bool> conditional = null);
    public void EnqueueDelay(MonoBehaviour mono, float delay, System.Func<bool> conditional = null);
    public void EnqueueSkipCheckpoint(System.Func<bool> checkIfCheckpointIsActive = null);
    public void SkipToNextCheckpoint();
    public void Pause();
    public void Unpause();
    public void Cancel();
    public bool Paused { get; }
  }
}
```
