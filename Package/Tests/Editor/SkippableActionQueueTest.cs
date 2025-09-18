using System;
using NUnit.Framework;
using UnityEngine.Events;
using Com.Uken.ActionQueue;

namespace Com.Uken.ActionQueue.Tests.Editor {
  [Timeout(500)]
  [Category("UnitTest")]
  public class SkippableActionQueueTest {
    [Test]
    public void NoParamQueue_AllStepsRun() {
      int numStepsRun = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);
      queue.EnqueueImmediateConsuming(() => numStepsRun++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);
      queue.EnqueueImmediateConsuming(() => numStepsRun++);
      queue.EnqueueImmediateConsuming(() => numStepsRun++);

      queue.Start();

      Assert.AreEqual(6, numStepsRun);
    }

    [Test]
    public void NoParamQueue_EnqueueSkipCheckpoint_HasNoEffectOnQueueIfSkipIsNotCalled() {
      int numStepsRun = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);
      queue.EnqueueImmediateConsuming(() => numStepsRun++);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);

      queue.Start();
      Assert.AreEqual(4, numStepsRun);
    }

    [Test]
    public void NoParamQueue_SkipToNextCheckpoint_SkippedStepsCallOnSkipAndNotTheirAction() {
      int numStepsRun = 0;
      int numStepsSkipped = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { /* Lock up the queue by not calling resume */ }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);

      queue.Start();
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(3, numStepsSkipped);
      Assert.AreEqual(1, numStepsRun);
    }

    [Test]
    public void NoParamQueue_SkipToNextCheckpoint_AllStepsCanBeSkippedIfNoCheckpoint() {
      int numStepsRun = 0;
      int numStepsSkipped = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { /* Lock up the queue by not calling resume */ }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);

      queue.Start();
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(4, numStepsSkipped);
      Assert.AreEqual(0, numStepsRun);
    }

    [Test]
    public void NoParamQueue_SkipToNextCheckpoint_MultipleCheckpointsCanBeUsed() {
      int numStepsRun = 0;
      int numStepsSkipped = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { /* Lock up the queue by not calling resume */ }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue(resume => { /* Lock up the queue by not calling resume */ }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue(resume => { /* Lock up the queue by not calling resume */ }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);

      queue.Start();
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(2, numStepsSkipped);
      Assert.AreEqual(0, numStepsRun);

      queue.SkipToNextCheckpoint();

      Assert.AreEqual(5, numStepsSkipped);
      Assert.AreEqual(0, numStepsRun);

      queue.SkipToNextCheckpoint();

      Assert.AreEqual(9, numStepsSkipped);
      Assert.AreEqual(0, numStepsRun);
    }

    [Test]
    public void NoParamQueue_SkipToNextCheckpoint_SkipsThroughEnqueueImmediateConsumingAndCallsTheAction() {
      bool enqueueImmediateConsumingRan = false;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { /* Lock up the queue by not calling resume */ }, onSkipped: null);
      queue.Enqueue(resume => resume(), onSkipped: null);
      queue.EnqueueImmediateConsuming(() => enqueueImmediateConsumingRan = true);
      queue.Enqueue(resume => resume(), onSkipped: null);

      queue.Start();
      queue.SkipToNextCheckpoint();

      Assert.That(enqueueImmediateConsumingRan);
    }

    [Test]
    public void NoParamQueue_SkipToNextCheckpoint_InvalidatesOldResumeActions() {
      UnityAction firstResumeAction = null;
      int numStepsRun = 0;
      int numStepsSkipped = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => firstResumeAction = resume /* Store the resume action and lock up the queue */, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue(resume => { /* Lock up the queue by not calling resume */ }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);

      queue.Start();
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(0, numStepsRun);
      Assert.AreEqual(2, numStepsSkipped);

      firstResumeAction();

      Assert.AreEqual(0, numStepsRun);
      Assert.AreEqual(2, numStepsSkipped);

      queue.SkipToNextCheckpoint();

      Assert.AreEqual(0, numStepsRun);
      Assert.AreEqual(4, numStepsSkipped);
    }

    [Test]
    public void NoParamQueue_SkipToNextCheckpoint_InvalidatesCurrentResumeAction() {
      UnityAction currentResumeAction = null;
      int numStepsRun = 0;
      int numStepsSkipped = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => currentResumeAction = resume /* Store the resume action and lock up the queue */, onSkipped: () => { currentResumeAction(); numStepsSkipped++; });
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);

      queue.Start();
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(1, numStepsRun);
      Assert.AreEqual(3, numStepsSkipped);
    }

    [Test]
    public void NoParamQueue_SkipToNextCheckpoint_WorksWhileQueueIsPausedAndKeepsPauseStatus() {
      int numStepsRun = 0;
      int numStepsSkipped = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { /* Lock up the queue by not calling resume */ }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);

      queue.Start();
      queue.Pause();
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(0, numStepsRun);
      Assert.AreEqual(2, numStepsSkipped);
      Assert.True(queue.Paused);

      queue.Unpause();
      Assert.AreEqual(1, numStepsRun);
      Assert.AreEqual(2, numStepsSkipped);
      Assert.False(queue.Paused);
    }

    [Test]
    public void NoParamQueue_Cancel_PreventsFutureStepsFromRunning() {
      int numStepsRun = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);
      queue.Enqueue(resume => { queue.Cancel(); resume(); }, onSkipped: null);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);
      queue.EnqueueImmediateConsuming(() => numStepsRun++);
      queue.EnqueueImmediateConsuming(() => numStepsRun++);

      queue.Start();

      Assert.AreEqual(1, numStepsRun);
    }

    [Test]
    public void NoParamQueue_Unpause_DoesNotContinueACancelledQueue() {
      int numStepsRun = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);
      queue.Enqueue(resume => { queue.Pause(); resume(); }, onSkipped: null);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: null);
      queue.EnqueueImmediateConsuming(() => numStepsRun++);
      queue.EnqueueImmediateConsuming(() => numStepsRun++);

      queue.Start();
      queue.Cancel();
      queue.Unpause();

      Assert.AreEqual(1, numStepsRun);
    }

    [Test]
    public void NoParamQueue_SkipToNextCheckpoint_DoesNothingIfQueueHasBeenCancelled() {
      int numStepsRun = 0;
      int numStepsSkipped = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { /* Lock up the queue by not calling resume */ }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);

      queue.Start();
      queue.Cancel();
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(2, numStepsRun);
      Assert.AreEqual(0, numStepsSkipped);
    }

    [Test]
    public void NoParamQueue_Cancel_WillStopAnInProgressSkipToNextCheckpoint() {
      int numStepsRun = 0;
      int numStepsSkipped = 0;

      var queue = new SkippableActionQueue();

      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { /* Lock up the queue by not calling resume */ }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => { queue.Cancel(); numStepsSkipped++; });
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);
      queue.Enqueue(resume => { numStepsRun++; resume(); }, onSkipped: () => numStepsSkipped++);

      queue.Start();
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(2, numStepsRun);
      Assert.AreEqual(3, numStepsSkipped);
    }

    [Test]
    public void OneParamQueue_StartedWithAnInitialParam_ParamGetsPassedBetweenEachStep() {
      int outputInt = 0;

      var queue = new SkippableActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 2), onSkipped: null);
      queue.Enqueue((resume, param) => resume(param), onSkipped: null);
      queue.Enqueue((resume, param) => resume(param + 1), onSkipped: null);
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 10);

      Assert.AreEqual(13, outputInt);
    }

    [Test]
    public void OneParamQueue_StartedWithNoInitialParam_ParamGetsPassedBetweenEachStep() {
      int outputInt = 0;

      var queue = new SkippableActionQueue<int>();

      queue.Enqueue((resume, _) => resume(), onSkipped: null);
      queue.Enqueue((resume, _) => resume(2), onSkipped: null);
      queue.Enqueue((resume, param) => resume(param), onSkipped: null);
      queue.Enqueue((resume, param) => resume(param + 1), onSkipped: null);
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start();

      Assert.AreEqual(3, outputInt);
    }

    [Test]
    public void OneParamQueue_StartedWithAnInitialParam_ParamGetsPassedBetweenEachStepAndPassesThroughEnqueueImmediate() {
      int outputInt = 0;

      var queue = new SkippableActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 2), onSkipped: null);
      queue.Enqueue((resume, param) => resume(param), onSkipped: null);
      queue.Enqueue((resume, param) => resume(param + 1), onSkipped: null);
      queue.EnqueueImmediateConsuming(param => { /* Pass */ });
      queue.EnqueueImmediateConsuming(() => { /* Pass */ });
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 10);

      Assert.AreEqual(13, outputInt);
    }

    [Test]
    public void OneParamQueue_StartedWithAnInitialParam_ParamGetsPassedBetweenEachStepAndPassesThroughResumeCalledWithNoArgs() {
      int outputInt = 0;

      var queue = new SkippableActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 2), onSkipped: null);
      queue.Enqueue((resume, param) => resume(param), onSkipped: null);
      queue.Enqueue((resume, param) => resume(), onSkipped: null); // Pass
      queue.Enqueue((resume, param) => resume(param + 1), onSkipped: null);
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 10);

      Assert.AreEqual(13, outputInt);
    }

    [Test]
    public void OneParamQueue_StartedWithAnInitialParam_ParamGetsPassedBetweenEachStepAndPassesThroughPauseAndUnpause() {
      int outputInt = 0;

      var queue = new SkippableActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 2), onSkipped: null);
      queue.Enqueue((resume, param) => resume(param), onSkipped: null);
      queue.EnqueueImmediateConsuming(() => queue.Pause());
      queue.Enqueue((resume, param) => resume(param + 1), onSkipped: null);
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 10);
      queue.Unpause();

      Assert.AreEqual(13, outputInt);
    }

    [Test]
    public void OneParamQueue_EnqueueSkipCheckpoint_PassesParamsThroughAndHasNoEffectIfSkipNotCalled() {
      int outputInt = 0;

      var queue = new SkippableActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 1), onSkipped: null);
      queue.Enqueue((resume, param) => resume(param + 2), onSkipped: null);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue((resume, param) => resume(param + 3), onSkipped: null);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue((resume, param) => resume(param + 4), onSkipped: null);
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 0);

      Assert.AreEqual(10, outputInt);
    }

    [Test]
    public void OneParamQueue_SkipToNextCheckpoint_ParamsPreservedAfterCheckpoint() {
      int outputInt = 0;

      var queue = new SkippableActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 1), onSkipped: null);
      queue.Enqueue((resume, param) => { /* Lock up the queue by not calling resume */ }, onSkipped: null);
      queue.Enqueue((resume, param) => resume(param + 2), onSkipped: null);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue((resume, param) => resume(param + 3), onSkipped: null);
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 0);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(4, outputInt);
    }

    [Test]
    public void OneParamQueue_SkipToNextCheckpoint_ParamsCanBeModifiedBySkippedStepsInOnSkip() {
      int outputInt = 0;

      var queue = new SkippableActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 1), onSkipped: param => param + 1000);
      queue.Enqueue((resume, param) => { /* Lock up the queue by not calling resume */ }, onSkipped: param => param + 10);
      queue.Enqueue((resume, param) => resume(param + 2), onSkipped: param => param + 20);
      queue.Enqueue((resume, param) => resume(param + 3), onSkipped: param => param + 30);
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 0);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(61, outputInt);
    }

    [Test]
    public void OneParamQueue_SkipToNextCheckpoint_ParamsPassThroughSkippedStepsThatReturnNothingInOnSkip() {
      int outputInt = 0;

      var queue = new SkippableActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 1), onSkipped: null);
      queue.Enqueue((resume, param) => { /* Lock up the queue by not calling resume */ }, onSkipped: param => { /* pass */ });
      queue.Enqueue((resume, param) => resume(), onSkipped: param => { /* pass */ });
      queue.Enqueue((resume, param) => resume(), onSkipped: param => { /* pass */ });
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 0);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(1, outputInt);
    }

    [Test]
    public void OneParamQueue_SkipToNextCheckpoint_ParamsPassThroughStepsWithNullOnSkip() {
      int outputInt = 0;

      var queue = new SkippableActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 1), onSkipped: null);
      queue.Enqueue((resume, param) => { /* Lock up the queue by not calling resume */ }, onSkipped: null);
      queue.Enqueue((resume, param) => resume(param + 2), onSkipped: null);
      queue.Enqueue((resume, param) => resume(param + 3), onSkipped: null);
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 0);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(1, outputInt);
    }

    [Test]
    public void TwoParamQueue_StartedWithTwoInitialParams_ParamsGetPassedBetweenEachStep() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new SkippableActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "d"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam, strParam + "e"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc");

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
    }

    [Test]
    public void TwoParamQueue_StartedWithNoInitialParams_ParamsGetPassedBetweenEachStep() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new SkippableActionQueue<int, string>();

      queue.Enqueue((resume, _, __) => resume(), onSkipped: null);
      queue.Enqueue((resume, _, __) => resume(2, __), onSkipped: null);
      queue.Enqueue((resume, intParam, _) => resume(intParam, "abc"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam, strParam), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam + "d"), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start();

      Assert.AreEqual(3, outputInt);
      Assert.AreEqual("abcd", outputStr);
    }

    [Test]
    public void TwoParamQueue_StartedWithTwoInitialParams_ParamsGetPassedBetweenEachStepAndPassThroughEnqueueImmediate() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new SkippableActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "d"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam, strParam + "e"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam) => { /* Pass */ });
      queue.EnqueueImmediateConsuming(() => { /* Pass */ });
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc");

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
    }

    [Test]
    public void TwoParamQueue_StartedWithTwoInitialParams_ParamsGetPassedBetweenEachStepAndPassThroughResumeCalledWithNoArgs() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new SkippableActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "d"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam, strParam + "e"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(), onSkipped: null); // Pass
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc");

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
    }

    [Test]
    public void TwoParamQueue_StartedWithTwoInitialParams_ParamsGetPassedBetweenEachStepAndPassThroughPauseAndUnpause() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new SkippableActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "d"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam, strParam + "e"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam), onSkipped: null);
      queue.EnqueueImmediateConsuming(() => queue.Pause());
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc");
      queue.Unpause();

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
    }

    [Test]
    public void TwoParamQueue_EnqueueSkipCheckpoint_PassesParamsThroughAndHasNoEffectIfSkipNotCalled() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new SkippableActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam + "a"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "b"), onSkipped: null);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 3, strParam + "c"), onSkipped: null);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 4, strParam + "d"), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 0, initialParam2: string.Empty);

      Assert.AreEqual(10, outputInt);
      Assert.AreEqual("abcd", outputStr);
    }

    [Test]
    public void TwoParamQueue_SkipToNextCheckpoint_ParamsPreservedAfterCheckpoint() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new SkippableActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam + "a"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => { /* Lock up the queue by not calling resume */ }, onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "b"), onSkipped: null);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 3, strParam + "c"), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 0, initialParam2: string.Empty);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(4, outputInt);
      Assert.AreEqual("ac", outputStr);
    }

    [Test]
    public void TwoParamQueue_SkipToNextCheckpoint_ParamsCanBeModifiedBySkippedStepsInOnSkip() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new SkippableActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam + "a"), onSkipped: (intParam, strParam) => (intParam + 1000, strParam + "d"));
      queue.Enqueue((resume, intParam, strParam) => { /* Lock up the queue by not calling resume */ }, onSkipped: (intParam, strParam) => (intParam + 10, strParam + "x"));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "b"), onSkipped: (intParam, strParam) => (intParam + 20, strParam + "y"));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 3, strParam + "c"), onSkipped: (intParam, strParam) => (intParam + 30, strParam + "z"));
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 0, initialParam2: string.Empty);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(61, outputInt);
      Assert.AreEqual("axyz", outputStr);
    }

    [Test]
    public void TwoParamQueue_SkipToNextCheckpoint_ParamsPassThroughSkippedStepsThatReturnNothingInOnSkip() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new SkippableActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam + "a"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => { /* Lock up the queue by not calling resume */ }, onSkipped: (intParam, strParam) => { /* pass */ });
      queue.Enqueue((resume, intParam, strParam) => resume(), onSkipped: (intParam, strParam) => { /* pass */ });
      queue.Enqueue((resume, intParam, strParam) => resume(), onSkipped: (intParam, strParam) => { /* pass */ });
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 0, initialParam2: string.Empty);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(1, outputInt);
      Assert.AreEqual("a", outputStr);
    }

    [Test]
    public void TwoParamQueue_SkipToNextCheckpoint_ParamsPassThroughStepsWithNullOnSkip() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new SkippableActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam + "a"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => { /* Lock up the queue by not calling resume */ }, onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "b"), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 3, strParam + "c"), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 0, initialParam2: string.Empty);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(1, outputInt);
      Assert.AreEqual("a", outputStr);
    }

    [Test]
    public void ThreeParamQueue_StartedWithThreeInitialParams_ParamsGetPassedBetweenEachStep() {
      int outputInt = 0;
      string outputStr = string.Empty;
      long outputLong = 0;

      var queue = new SkippableActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "d", longParam + 20), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam, strParam + "e", longParam), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam, longParam + 10), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { outputInt = intParam; outputStr = strParam; outputLong = longParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc", initialParam3: 100);

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
      Assert.AreEqual(130, outputLong);
    }

    [Test]
    public void ThreeParamQueue_StartedWithNoInitialParams_ParamsGetPassedBetweenEachStep() {
      int outputInt = 0;
      string outputStr = string.Empty;
      long outputLong = 0;

      var queue = new SkippableActionQueue<int, string, long>();

      queue.Enqueue((resume, _, __, ___) => resume(), onSkipped: null);
      queue.Enqueue((resume, _, __, ___) => resume(2, __, ___), onSkipped: null);
      queue.Enqueue((resume, intParam, _, __) => resume(intParam, "abc", __), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, _) => resume(intParam, strParam, 20), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam, strParam, longParam), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam + "d", longParam + 10), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { outputInt = intParam; outputStr = strParam; outputLong = longParam; });

      queue.Start();

      Assert.AreEqual(3, outputInt);
      Assert.AreEqual("abcd", outputStr);
      Assert.AreEqual(30, outputLong);
    }

    [Test]
    public void ThreeParamQueue_StartedWithThreeInitialParams_ParamsGetPassedBetweenEachStepAndPassThroughEnqueueImmediate() {
      int outputInt = 0;
      string outputStr = string.Empty;
      long outputLong = 0;

      var queue = new SkippableActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "d", longParam + 20), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam, strParam + "e", longParam), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam, longParam + 10), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { /* Pass */ });
      queue.EnqueueImmediateConsuming(() => { /* Pass */ });
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { outputInt = intParam; outputStr = strParam; outputLong = longParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc", initialParam3: 100);

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
      Assert.AreEqual(130, outputLong);
    }

    [Test]
    public void ThreeParamQueue_StartedWithThreeInitialParams_ParamsGetPassedBetweenEachStepAndPassThroughResumeCalledWithNoArgs() {
      int outputInt = 0;
      string outputStr = string.Empty;
      long outputLong = 0;

      var queue = new SkippableActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "d", longParam + 20), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam, strParam + "e", longParam), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam, longParam + 10), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(), onSkipped: null); // Pass
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { outputInt = intParam; outputStr = strParam; outputLong = longParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc", initialParam3: 100);

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
      Assert.AreEqual(130, outputLong);
    }

    [Test]
    public void ThreeParamQueue_StartedWithThreeInitialParams_ParamsGetPassedBetweenEachStepAndPassThroughPauseAndUnpause() {
      int outputInt = 0;
      string outputStr = string.Empty;
      long outputLong = 0;

      var queue = new SkippableActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "d", longParam + 20), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam, strParam + "e", longParam), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam, longParam + 10), onSkipped: null);
      queue.EnqueueImmediateConsuming(() => queue.Pause());
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { outputInt = intParam; outputStr = strParam; outputLong = longParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc", initialParam3: 100);
      queue.Unpause();

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
      Assert.AreEqual(130, outputLong);
    }

    [Test]
    public void ThreeParamQueue_EnqueueSkipCheckpoint_PassesParamsThroughAndHasNoEffectIfSkipNotCalled() {
      int outputInt = 0;
      string outputStr = string.Empty;
      long outputLong = 0;

      var queue = new SkippableActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam + "a", longParam + 10), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "b", longParam + 20), onSkipped: null);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 3, strParam + "c", longParam + 30), onSkipped: null);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 4, strParam + "d", longParam + 40), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { outputInt = intParam; outputStr = strParam; outputLong = longParam; });

      queue.Start(initialParam1: 0, initialParam2: string.Empty, initialParam3: 0);

      Assert.AreEqual(10, outputInt);
      Assert.AreEqual("abcd", outputStr);
      Assert.AreEqual(100, outputLong);
    }

    [Test]
    public void ThreeParamQueue_SkipToNextCheckpoint_ParamsPreservedAfterCheckpoint() {
      int outputInt = 0;
      string outputStr = string.Empty;
      long outputLong = 0;

      var queue = new SkippableActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam + "a", longParam + 10), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => { /* Lock up the queue by not calling resume */ }, onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "b", longParam + 20), onSkipped: null);
      queue.EnqueueSkipCheckpoint();
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 3, strParam + "c", longParam + 30), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { outputInt = intParam; outputStr = strParam; outputLong = longParam; });

      queue.Start(initialParam1: 0, initialParam2: string.Empty, initialParam3: 0);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(4, outputInt);
      Assert.AreEqual("ac", outputStr);
      Assert.AreEqual(40, outputLong);
    }

    [Test]
    public void ThreeParamQueue_SkipToNextCheckpoint_ParamsCanBeModifiedBySkippedStepsInOnSkip() {
      int outputInt = 0;
      string outputStr = string.Empty;
      long outputLong = 0;

      var queue = new SkippableActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam + "a", longParam + 10), onSkipped: (intParam, strParam, longParam) => (intParam + 1000, strParam + "d", longParam + 1000));
      queue.Enqueue((resume, intParam, strParam, longParam) => { /* Lock up the queue by not calling resume */ }, onSkipped: (intParam, strParam, longParam) => (intParam + 10, strParam + "x", longParam + 100));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "b", longParam + 20), onSkipped: (intParam, strParam, longParam) => (intParam + 20, strParam + "y", longParam + 200));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 3, strParam + "c", longParam + 30), onSkipped: (intParam, strParam, longParam) => (intParam + 30, strParam + "z", longParam + 300));
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { outputInt = intParam; outputStr = strParam; outputLong = longParam; });

      queue.Start(initialParam1: 0, initialParam2: string.Empty, initialParam3: 0);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(61, outputInt);
      Assert.AreEqual("axyz", outputStr);
      Assert.AreEqual(610, outputLong);
    }

    [Test]
    public void ThreeParamQueue_SkipToNextCheckpoint_ParamsPassThroughSkippedStepsThatReturnNothingInOnSkip() {
      int outputInt = 0;
      string outputStr = string.Empty;
      long outputLong = 0;

      var queue = new SkippableActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam + "a", longParam + 10), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => { /* Lock up the queue by not calling resume */ }, onSkipped: (intParam, strParam, longParam) => { /* pass */ });
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(), onSkipped: (intParam, strParam, longParam) => { /* pass */ });
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(), onSkipped: (intParam, strParam, longParam) => { /* pass */ });
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { outputInt = intParam; outputStr = strParam; outputLong = longParam; });

      queue.Start(initialParam1: 0, initialParam2: string.Empty, initialParam3: 0);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(1, outputInt);
      Assert.AreEqual("a", outputStr);
      Assert.AreEqual(10, outputLong);
    }

    [Test]
    public void ThreeParamQueue_SkipToNextCheckpoint_ParamsPassThroughStepsWithNullOnSkip() {
      int outputInt = 0;
      string outputStr = string.Empty;
      long outputLong = 0;

      var queue = new SkippableActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam + "a", longParam + 10), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => { /* Lock up the queue by not calling resume */ }, onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "b", longParam + 20), onSkipped: null);
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 3, strParam + "c", longParam + 30), onSkipped: null);
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { outputInt = intParam; outputStr = strParam; outputLong = longParam; });

      queue.Start(initialParam1: 0, initialParam2: string.Empty, initialParam3: 0);
      queue.SkipToNextCheckpoint();

      Assert.AreEqual(1, outputInt);
      Assert.AreEqual("a", outputStr);
      Assert.AreEqual(10, outputLong);
    }
  }
}
