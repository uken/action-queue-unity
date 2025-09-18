using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Com.Uken.ActionQueue.Internal;

namespace Com.Uken.ActionQueue {
  public class SkippableActionQueue : BaseActionQueue<object, object, object> {
    public void Start() {
      this.BaseStart();
    }

    public void Enqueue(UnityAction<UnityAction> action, UnityAction onSkipped, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, _, __, ___) => action(() => resume(null, null, null)),
        onSkipped: (_, __, ___) => {
          onSkipped?.Invoke();
          return ((object)null, (object)null, (object)null);
        },
        conditional: conditional);
    }

    public void EnqueueImmediateConsuming(UnityAction action, Func<bool> conditional = null) {
      this.BaseEnqueueImmediateConsuming(action, conditional);
    }

    public void EnqueueDelay(MonoBehaviour mono, float delay, Func<bool> conditional = null) {
      this.BaseEnqueueDelay(mono, delay, conditional);
    }

    public void EnqueueSkipCheckpoint(Func<bool> checkIfCheckpointIsActive = null) {
      this.BaseEnqueueSkipCheckpoint(checkIfCheckpointIsActive);
    }

    public void SkipToNextCheckpoint() {
      this.BaseSkipToNextCheckpoint();
    }
  }

  public class SkippableActionQueue<T> : BaseActionQueue<T, object, object> {
    public void Start(T initialParam = default(T)) {
      this.BaseStart(initialParam);
    }

    public void Enqueue(UnityAction<UnityAction, T> action, UnityAction<T> onSkipped, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param, _, __) => action(() => resume(param, null, null), param),
        onSkipped: (param, _, __) => {
          onSkipped?.Invoke(param);
          return (param, (object)null, (object)null);
        },
        conditional: conditional);
    }

    public void Enqueue(UnityAction<UnityAction<T>, T> action, Func<T, T> onSkipped, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param, _, __) => action((resumeParam) => resume(resumeParam, null, null), param),
        onSkipped: (param, _, __) => {
          if (onSkipped == null) {
            return (param, (object)null, (object)null);
          }

          T skipOutput = onSkipped(param);
          return (skipOutput, (object)null, (object)null);
        },
        conditional: conditional);
    }

    public void EnqueueImmediateConsuming(UnityAction<T> action, Func<bool> conditional = null) {
      this.BaseEnqueueImmediateConsuming(
        (param, _, __) => action(param),
        conditional);
    }

    public void EnqueueImmediateConsuming(UnityAction action, Func<bool> conditional = null) {
      this.BaseEnqueueImmediateConsuming(action, conditional);
    }

    public void EnqueueDelay(MonoBehaviour mono, float delay, Func<bool> conditional = null) {
      this.BaseEnqueueDelay(mono, delay, conditional);
    }

    public void EnqueueSkipCheckpoint(Func<bool> checkIfCheckpointIsActive = null) {
      this.BaseEnqueueSkipCheckpoint(checkIfCheckpointIsActive);
    }

    public void SkipToNextCheckpoint() {
      this.BaseSkipToNextCheckpoint();
    }
  }

  public class SkippableActionQueue<T1, T2> : BaseActionQueue<T1, T2, object> {
    public void Start(T1 initialParam1 = default(T1), T2 initialParam2 = default(T2)) {
      this.BaseStart(initialParam1, initialParam2);
    }

    public void Enqueue(UnityAction<UnityAction, T1, T2> action, UnityAction<T1, T2> onSkipped, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param1, param2, _) => action(() => resume(param1, param2, null), param1, param2),
        onSkipped: (param1, param2, _) => {
          onSkipped?.Invoke(param1, param2);
          return (param1, param2, (object)null);
        },
        conditional: conditional);
    }

    public void Enqueue(UnityAction<UnityAction<T1, T2>, T1, T2> action, Func<T1, T2, (T1, T2)> onSkipped, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param1, param2, _) => action((resumeParam1, resumeParam2) => resume(resumeParam1, resumeParam2, null), param1, param2),
        onSkipped: (param1, param2, _) => {
          if (onSkipped == null) {
            return (param1, param2, (object)null);
          }

          var skipOutput = onSkipped(param1, param2);
          return (skipOutput.Item1, skipOutput.Item2, (object)null);
        },
        conditional: conditional);
    }

    public void EnqueueImmediateConsuming(UnityAction<T1, T2> action, Func<bool> conditional = null) {
      this.BaseEnqueueImmediateConsuming(
        (param1, param2, _) => action(param1, param2),
        conditional);
    }

    public void EnqueueImmediateConsuming(UnityAction action, Func<bool> conditional = null) {
      this.BaseEnqueueImmediateConsuming(action, conditional);
    }

    public void EnqueueDelay(MonoBehaviour mono, float delay, Func<bool> conditional = null) {
      this.BaseEnqueueDelay(mono, delay, conditional);
    }

    public void EnqueueSkipCheckpoint(Func<bool> checkIfCheckpointIsActive = null) {
      this.BaseEnqueueSkipCheckpoint(checkIfCheckpointIsActive);
    }

    public void SkipToNextCheckpoint() {
      this.BaseSkipToNextCheckpoint();
    }
  }

  public class SkippableActionQueue<T1, T2, T3> : BaseActionQueue<T1, T2, T3> {
    public void Start(T1 initialParam1 = default(T1), T2 initialParam2 = default(T2), T3 initialParam3 = default(T3)) {
      this.BaseStart(initialParam1, initialParam2, initialParam3);
    }

    public void Enqueue(UnityAction<UnityAction, T1, T2, T3> action, UnityAction<T1, T2, T3> onSkipped, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param1, param2, param3) => action(() => resume(param1, param2, param3), param1, param2, param3),
        onSkipped: (param1, param2, param3) => {
          onSkipped?.Invoke(param1, param2, param3);
          return (param1, param2, param3);
        },
        conditional: conditional);
    }

    public void Enqueue(UnityAction<UnityAction<T1, T2, T3>, T1, T2, T3> action, Func<T1, T2, T3, (T1, T2, T3)> onSkipped, Func<bool> conditional = null) {
      this.BaseEnqueue(
        action,
        onSkipped: (param1, param2, param3) => {
          if (onSkipped == null) {
            return (param1, param2, param3);
          }

          return onSkipped(param1, param2, param3);
        },
        conditional: conditional);
    }

    public void EnqueueImmediateConsuming(UnityAction<T1, T2, T3> action, Func<bool> conditional = null) {
      this.BaseEnqueueImmediateConsuming(action, conditional);
    }

    public void EnqueueImmediateConsuming(UnityAction action, Func<bool> conditional = null) {
      this.BaseEnqueueImmediateConsuming(action, conditional);
    }

    public void EnqueueDelay(MonoBehaviour mono, float delay, Func<bool> conditional = null) {
      this.BaseEnqueueDelay(mono, delay, conditional);
    }

    public void EnqueueSkipCheckpoint(Func<bool> checkIfCheckpointIsActive = null) {
      this.BaseEnqueueSkipCheckpoint(checkIfCheckpointIsActive);
    }

    public void SkipToNextCheckpoint() {
      this.BaseSkipToNextCheckpoint();
    }
  }
}
