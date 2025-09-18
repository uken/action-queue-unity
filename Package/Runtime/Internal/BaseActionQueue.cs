using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Com.Uken.ActionQueue.Internal {
  public abstract class BaseActionQueue<T1, T2, T3> {
    private Queue<ActionQueueDatum> queue = new Queue<ActionQueueDatum>();

    private ActionQueueContext? currentlyExecutingContext;
    private UnityAction continueOnUnpauseAction;
    private bool cancelled;
    private bool currentlyExecutingOnSkipAction;

    public bool InProgress => this.currentlyExecutingContext.HasValue;

    public bool IsEmpty => this.queue.Count == 0;

    public bool Paused { get; private set; }

    public void Cancel() {
      this.cancelled = true;
      this.currentlyExecutingContext = null;
    }

    public void Pause() {
      this.Paused = true;
    }

    public void Unpause() {
      this.Paused = false;

      if (this.continueOnUnpauseAction != null) {
        this.continueOnUnpauseAction();
        this.continueOnUnpauseAction = null;
      }
    }

    protected void BaseStart(T1 initialParam1 = default(T1), T2 initialParam2 = default(T2), T3 initialParam3 = default(T3)) {
      if (this.InProgress) {
        return;
      }

      this.Continue(default(ActionQueueContext), initialParam1, initialParam2, initialParam3, validForAnyContext: true);
    }

    protected void BaseEnqueue(UnityAction<UnityAction<T1, T2, T3>, T1, T2, T3> action, Func<T1, T2, T3, (T1, T2, T3)> onSkipped, Func<bool> conditional = null) {
      this.queue.Enqueue(
        ActionQueueDatum.Factory.CreateForAction(
          (resume, param1, param2, param3) => {
            if (conditional == null || conditional()) {
              action(resume, param1, param2, param3);
            } else {
              resume(param1, param2, param3);
            }
          },
          onSkipped: (param1, param2, param3) => {
            if (onSkipped != null && (conditional == null || conditional())) {
              return onSkipped(param1, param2, param3);
            } else {
              return (param1, param2, param3);
            }
          }));
    }

    protected void BaseEnqueueImmediateConsuming(UnityAction<T1, T2, T3> action, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param1, param2, param3) => {
          action(param1, param2, param3);
          resume(param1, param2, param3);
        },
        onSkipped: (param1, param2, param3) => {
          action(param1, param2, param3);
          return (param1, param2, param3);
        },
        conditional: conditional);
    }

    protected void BaseEnqueueImmediateConsuming(UnityAction action, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param1, param2, param3) => {
          action();
          resume(param1, param2, param3);
        },
        onSkipped: (param1, param2, param3) => {
          action();
          return (param1, param2, param3);
        },
        conditional: conditional);
    }

    protected void BaseEnqueueDelay(MonoBehaviour mono, float delay, Func<bool> conditional = null) {
      this.BaseEnqueue(
        (resume, param1, param2, param3) => DelayHandler.Schedule(mono, delay, () => resume(param1, param2, param3)),
        onSkipped: (param1, param2, param3) => (param1, param2, param3),
        conditional: conditional);
    }

    protected void BaseEnqueueSkipCheckpoint(Func<bool> checkIfCheckpointIsActive) {
      this.queue.Enqueue(ActionQueueDatum.Factory.CreateForSkipCheckpoint(checkIfCheckpointIsActive));
    }

    protected void BaseSkipToNextCheckpoint() {
      while (true) {
        if (this.InProgress == false) {
          return;
        }

        ActionQueueContext currentContext = this.currentlyExecutingContext.Value;
        T1 param1 = currentContext.Param1;
        T2 param2 = currentContext.Param2;
        T3 param3 = currentContext.Param3;

        if (currentContext.Datum.IsSkipCheckpoint) {
          if (currentContext.Datum.CheckIfCheckpointIsActive == null || currentContext.Datum.CheckIfCheckpointIsActive()) {
            this.Continue(currentContext, param1, param2, param3);
            return;
          } else {
            // We want the checkpoint to fall through if it's not active so that it is skipped
          }
        } else {
          if (currentContext.Datum.OnSkipped == null) {
            Debug.LogError("BaseActionQueue: SkipToNextCheckpoint was called, but an ActionQueueDatum in the queue has a null OnSkipped action. Something is probably wrong with the implementation of this class.");
            return;
          }

          this.currentlyExecutingOnSkipAction = true;

          (T1, T2, T3) outputParams = currentContext.Datum.OnSkipped(param1, param2, param3);

          param1 = outputParams.Item1;
          param2 = outputParams.Item2;
          param3 = outputParams.Item3;

          this.currentlyExecutingOnSkipAction = false;
        }

        if (this.cancelled) {
          return;
        }

        if (this.IsEmpty) {
          this.currentlyExecutingContext = null;
          return;
        }

        this.currentlyExecutingContext = new ActionQueueContext(this.queue.Dequeue(), param1, param2, param3);
      }
    }

    private void Continue(ActionQueueContext validContext, T1 param1, T2 param2, T3 param3, bool validForAnyContext = false) {
      if (this.cancelled || this.currentlyExecutingOnSkipAction) {
        return;
      }

      if (validForAnyContext == false) {
        if (this.currentlyExecutingContext.HasValue == false) {
          return;
        }

        if (this.currentlyExecutingContext.Value != validContext) {
          return;
        }
      }

      if (this.Paused) {
        this.continueOnUnpauseAction = () => this.Continue(validContext, param1, param2, param3);
        return;
      }

      if (this.IsEmpty) {
        this.currentlyExecutingContext = null;
        return;
      }

      ActionQueueDatum nextDatum = this.queue.Dequeue();

      // NOTE: A local variable is used here, so that when it's passed to `this.Continue` below, it creates a closure.
      //       This is so that `this.Continue` doesn't use the *current* value of `this.currentlyExecutingContext`, but instead,
      //       it uses the value as it was defined here.
      ActionQueueContext contextForThisAction = new ActionQueueContext(nextDatum, param1, param2, param3);

      this.currentlyExecutingContext = contextForThisAction;

      if (nextDatum.IsSkipCheckpoint) {
        this.Continue(contextForThisAction, param1, param2, param3);
        return;
      }

      nextDatum.Action((continueParam1, continueParam2, continueParam3) => this.Continue(contextForThisAction, continueParam1, continueParam2, continueParam3), param1, param2, param3);
    }

    private struct ActionQueueContext {
      public readonly ActionQueueDatum Datum;
      public readonly T1 Param1;
      public readonly T2 Param2;
      public readonly T3 Param3;

      public ActionQueueContext(ActionQueueDatum datum, T1 param1, T2 param2, T3 param3) {
        this.Datum = datum;
        this.Param1 = param1;
        this.Param2 = param2;
        this.Param3 = param3;
      }

      public static bool operator ==(ActionQueueContext context1, ActionQueueContext context2) {
        return
          context1.Datum == context2.Datum &&
          ((context1.Param1 == null && context2.Param1 == null) || (context1.Param1 != null && context1.Param1.Equals(context2.Param1))) &&
          ((context1.Param2 == null && context2.Param2 == null) || (context1.Param2 != null && context1.Param2.Equals(context2.Param2))) &&
          ((context1.Param3 == null && context2.Param3 == null) || (context1.Param3 != null && context1.Param3.Equals(context2.Param3)));
      }

      public static bool operator !=(ActionQueueContext context1, ActionQueueContext context2) {
        return !(context1 == context2);
      }

      public override bool Equals(object obj) {
        if (obj is ActionQueueContext == false) {
          return false;
        }

        return this == (ActionQueueContext)obj;
      }

      public override int GetHashCode() {
        int hash = 13;

        hash += (hash * 7) + this.Datum.GetHashCode();
        hash += (hash * 7) + this.Param1.GetHashCode();
        hash += (hash * 7) + this.Param2.GetHashCode();
        hash += (hash * 7) + this.Param3.GetHashCode();

        return hash;
      }
    }

    private struct ActionQueueDatum {
      public readonly UnityAction<UnityAction<T1, T2, T3>, T1, T2, T3> Action;
      public readonly Func<T1, T2, T3, (T1, T2, T3)> OnSkipped;
      public readonly bool IsSkipCheckpoint;
      public readonly Func<bool> CheckIfCheckpointIsActive;

      private ActionQueueDatum(UnityAction<UnityAction<T1, T2, T3>, T1, T2, T3> action, Func<T1, T2, T3, (T1, T2, T3)> onSkipped, bool isSkipCheckpoint, Func<bool> checkIfCheckpointIsActive = null) {
        this.Action = action;
        this.OnSkipped = onSkipped;
        this.IsSkipCheckpoint = isSkipCheckpoint;
        this.CheckIfCheckpointIsActive = checkIfCheckpointIsActive;
      }

      public static bool operator ==(ActionQueueDatum datum1, ActionQueueDatum datum2) {
        return
          datum1.Action == datum2.Action &&
          datum1.OnSkipped == datum2.OnSkipped &&
          datum1.IsSkipCheckpoint == datum2.IsSkipCheckpoint;
      }

      public static bool operator !=(ActionQueueDatum datum1, ActionQueueDatum datum2) {
        return !(datum1 == datum2);
      }

      public override bool Equals(object obj) {
        if (obj is ActionQueueDatum == false) {
          return false;
        }

        return this == (ActionQueueDatum)obj;
      }

      public override int GetHashCode() {
        int hash = 13;

        hash += (hash * 7) + this.Action.GetHashCode();
        hash += (hash * 7) + this.OnSkipped.GetHashCode();
        hash += (hash * 7) + this.IsSkipCheckpoint.GetHashCode();

        return hash;
      }

      public static class Factory {
        public static ActionQueueDatum CreateForAction(UnityAction<UnityAction<T1, T2, T3>, T1, T2, T3> action, Func<T1, T2, T3, (T1, T2, T3)> onSkipped) {
          return new ActionQueueDatum(action, onSkipped, isSkipCheckpoint: false);
        }

        public static ActionQueueDatum CreateForSkipCheckpoint(Func<bool> checkIfCheckpointIsActive = null) {
          return new ActionQueueDatum(null, null, isSkipCheckpoint: true, checkIfCheckpointIsActive: checkIfCheckpointIsActive);
        }
      }
    }
  }
}
