using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using Com.Uken.ActionQueue.Internal;

namespace Com.Uken.ActionQueue {
  public class ActionQueue : BaseActionQueue<object, object, object> {
    public void Start() {
      this.BaseStart();
    }

    public void Enqueue(UnityAction<UnityAction> action, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, _, __, ___) => action(() => resume(null, null, null)),
        onSkipped: null,
        conditional: conditional);
    }

    public void EnqueueImmediateConsuming(UnityAction action, Func<bool> conditional = null) {
      this.BaseEnqueueImmediateConsuming(action, conditional);
    }

    public void EnqueueDelay(MonoBehaviour mono, float delay, Func<bool> conditional = null) {
      this.BaseEnqueueDelay(mono, delay, conditional);
    }
  }

  public class ActionQueue<T> : BaseActionQueue<T, object, object> {
    public void Start(T initialParam = default(T)) {
      this.BaseStart(initialParam);
    }

    public void Enqueue(UnityAction<UnityAction, T> action, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param, _, __) => action(() => resume(param, null, null), param),
        onSkipped: null,
        conditional: conditional);
    }

    public void Enqueue(UnityAction<UnityAction<T>, T> action, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param, _, __) => action((resumeParam) => resume(resumeParam, null, null), param),
        onSkipped: null,
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
  }

  public class ActionQueue<T1, T2> : BaseActionQueue<T1, T2, object> {
    public void Start(T1 initialParam1 = default(T1), T2 initialParam2 = default(T2)) {
      this.BaseStart(initialParam1, initialParam2);
    }

    public void Enqueue(UnityAction<UnityAction, T1, T2> action, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param1, param2, _) => action(() => resume(param1, param2, null), param1, param2),
        onSkipped: null,
        conditional: conditional);
    }

    public void Enqueue(UnityAction<UnityAction<T1, T2>, T1, T2> action, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param1, param2, _) => action((resumeParam1, resumeParam2) => resume(resumeParam1, resumeParam2, null), param1, param2),
        onSkipped: null,
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
  }

  public class ActionQueue<T1, T2, T3> : BaseActionQueue<T1, T2, T3> {
    public void Start(T1 initialParam1 = default(T1), T2 initialParam2 = default(T2), T3 initialParam3 = default(T3)) {
      this.BaseStart(initialParam1, initialParam2, initialParam3);
    }

    public void Enqueue(UnityAction<UnityAction, T1, T2, T3> action, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param1, param2, param3) => action(() => resume(param1, param2, param3), param1, param2, param3),
        onSkipped: null,
        conditional: conditional);
    }

    public void Enqueue(UnityAction<UnityAction<T1, T2, T3>, T1, T2, T3> action, Func<bool> conditional = null) {
      this.BaseEnqueue(action, onSkipped: null, conditional: conditional);
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
  }
}
