using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Com.Uken.ActionQueue.Internal {
  internal static class DelayHandler {
    public static void Schedule(MonoBehaviour mono, float seconds, UnityAction action) {
      if (Mathf.Approximately(seconds, 0.0f) || seconds <= 0) {
        action?.Invoke();
      }

      mono.StartCoroutine(WaitForSeconds(seconds, action));
    }

    private static IEnumerator WaitForSeconds(float seconds, UnityAction action) {
      yield return new WaitForSeconds(seconds);
      action?.Invoke();
    }
  }
}
