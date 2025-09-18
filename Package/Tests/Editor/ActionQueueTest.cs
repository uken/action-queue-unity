using NUnit.Framework;
using Com.Uken.ActionQueue;

namespace Com.Uken.ActionQueue.Tests.Editor {
  [Timeout(500)]
  [Category("UnitTest")]
  public class ActionQueueTest {
    [Test]
    public void NoParamQueue_AllStepsRun() {
      int numStepsRun = 0;

      var queue = new ActionQueue();

      queue.Enqueue(resume => { numStepsRun++; resume(); });
      queue.EnqueueImmediateConsuming(() => numStepsRun++);
      queue.Enqueue(resume => { numStepsRun++; resume(); });
      queue.Enqueue(resume => { numStepsRun++; resume(); });
      queue.EnqueueImmediateConsuming(() => numStepsRun++);
      queue.EnqueueImmediateConsuming(() => numStepsRun++);

      queue.Start();

      Assert.AreEqual(6, numStepsRun);
    }

    [Test]
    public void NoParamQueue_Cancel_PreventsFutureStepsFromRunning() {
      int numStepsRun = 0;

      var queue = new ActionQueue();

      queue.Enqueue(resume => { numStepsRun++; resume(); });
      queue.Enqueue(resume => { queue.Cancel(); resume(); });
      queue.Enqueue(resume => { numStepsRun++; resume(); });
      queue.Enqueue(resume => { numStepsRun++; resume(); });
      queue.EnqueueImmediateConsuming(() => numStepsRun++);
      queue.EnqueueImmediateConsuming(() => numStepsRun++);

      queue.Start();

      Assert.AreEqual(1, numStepsRun);
    }

    [Test]
    public void NoParamQueue_Unpause_DoesNotContinueACancelledQueue() {
      int numStepsRun = 0;

      var queue = new ActionQueue();

      queue.Enqueue(resume => { numStepsRun++; resume(); });
      queue.Enqueue(resume => { queue.Pause(); resume(); });
      queue.Enqueue(resume => { numStepsRun++; resume(); });
      queue.Enqueue(resume => { numStepsRun++; resume(); });
      queue.EnqueueImmediateConsuming(() => numStepsRun++);
      queue.EnqueueImmediateConsuming(() => numStepsRun++);

      queue.Start();
      queue.Cancel();
      queue.Unpause();

      Assert.AreEqual(1, numStepsRun);
    }

    [Test]
    public void OneParamQueue_StartedWithAnInitialParam_ParamGetsPassedBetweenEachStep() {
      int outputInt = 0;

      var queue = new ActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 2));
      queue.Enqueue((resume, param) => resume(param));
      queue.Enqueue((resume, param) => resume(param + 1));
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 10);

      Assert.AreEqual(13, outputInt);
    }

    [Test]
    public void OneParamQueue_StartedWithNoInitialParam_ParamGetsPassedBetweenEachStep() {
      int outputInt = 0;

      var queue = new ActionQueue<int>();

      queue.Enqueue((resume, _) => resume());
      queue.Enqueue((resume, _) => resume(2));
      queue.Enqueue((resume, param) => resume(param));
      queue.Enqueue((resume, param) => resume(param + 1));
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start();

      Assert.AreEqual(3, outputInt);
    }

    [Test]
    public void OneParamQueue_StartedWithAnInitialParam_ParamGetsPassedBetweenEachStepAndPassesThroughEnqueueImmediate() {
      int outputInt = 0;

      var queue = new ActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 2));
      queue.Enqueue((resume, param) => resume(param));
      queue.Enqueue((resume, param) => resume(param + 1));
      queue.EnqueueImmediateConsuming(param => { /* Pass */ });
      queue.EnqueueImmediateConsuming(() => { /* Pass */ });
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 10);

      Assert.AreEqual(13, outputInt);
    }

    [Test]
    public void OneParamQueue_StartedWithAnInitialParam_ParamGetsPassedBetweenEachStepAndPassesThroughResumeCalledWithNoArgs() {
      int outputInt = 0;

      var queue = new ActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 2));
      queue.Enqueue((resume, param) => resume(param));
      queue.Enqueue((resume, param) => resume()); // Pass
      queue.Enqueue((resume, param) => resume(param + 1));
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 10);

      Assert.AreEqual(13, outputInt);
    }

    [Test]
    public void OneParamQueue_StartedWithAnInitialParam_ParamGetsPassedBetweenEachStepAndPassesThroughPauseAndUnpause() {
      int outputInt = 0;

      var queue = new ActionQueue<int>();

      queue.Enqueue((resume, param) => resume(param + 2));
      queue.Enqueue((resume, param) => resume(param));
      queue.EnqueueImmediateConsuming(() => queue.Pause());
      queue.Enqueue((resume, param) => resume(param + 1));
      queue.EnqueueImmediateConsuming(param => outputInt = param);

      queue.Start(initialParam: 10);
      queue.Unpause();

      Assert.AreEqual(13, outputInt);
    }

    [Test]
    public void TwoParamQueue_StartedWithTwoInitialParams_ParamsGetPassedBetweenEachStep() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new ActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "d"));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam, strParam + "e"));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam));
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc");

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
    }

    [Test]
    public void TwoParamQueue_StartedWithNoInitialParams_ParamsGetPassedBetweenEachStep() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new ActionQueue<int, string>();

      queue.Enqueue((resume, _, __) => resume());
      queue.Enqueue((resume, _, __) => resume(2, __));
      queue.Enqueue((resume, intParam, _) => resume(intParam, "abc"));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam, strParam));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam + "d"));
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start();

      Assert.AreEqual(3, outputInt);
      Assert.AreEqual("abcd", outputStr);
    }

    [Test]
    public void TwoParamQueue_StartedWithTwoInitialParams_ParamsGetPassedBetweenEachStepAndPassThroughEnqueueImmediate() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new ActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "d"));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam, strParam + "e"));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam));
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

      var queue = new ActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "d"));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam, strParam + "e"));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam));
      queue.Enqueue((resume, intParam, strParam) => resume()); // Pass
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc");

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
    }

    [Test]
    public void TwoParamQueue_StartedWithTwoInitialParams_ParamsGetPassedBetweenEachStepAndPassThroughPauseAndUnpause() {
      int outputInt = 0;
      string outputStr = string.Empty;

      var queue = new ActionQueue<int, string>();

      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 2, strParam + "d"));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam, strParam + "e"));
      queue.Enqueue((resume, intParam, strParam) => resume(intParam + 1, strParam));
      queue.EnqueueImmediateConsuming(() => queue.Pause());
      queue.EnqueueImmediateConsuming((intParam, strParam) => { outputInt = intParam; outputStr = strParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc");
      queue.Unpause();

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
    }

    [Test]
    public void ThreeParamQueue_StartedWithThreeInitialParams_ParamsGetPassedBetweenEachStep() {
      int outputInt = 0;
      string outputStr = string.Empty;
      long outputLong = 0;

      var queue = new ActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "d", longParam + 20));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam, strParam + "e", longParam));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam, longParam + 10));
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

      var queue = new ActionQueue<int, string, long>();

      queue.Enqueue((resume, _, __, ___) => resume());
      queue.Enqueue((resume, _, __, ___) => resume(2, __, ___));
      queue.Enqueue((resume, intParam, _, __) => resume(intParam, "abc", __));
      queue.Enqueue((resume, intParam, strParam, _) => resume(intParam, strParam, 20));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam, strParam, longParam));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam + "d", longParam + 10));
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

      var queue = new ActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "d", longParam + 20));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam, strParam + "e", longParam));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam, longParam + 10));
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

      var queue = new ActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "d", longParam + 20));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam, strParam + "e", longParam));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam, longParam + 10));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume()); // Pass
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

      var queue = new ActionQueue<int, string, long>();

      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 2, strParam + "d", longParam + 20));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam, strParam + "e", longParam));
      queue.Enqueue((resume, intParam, strParam, longParam) => resume(intParam + 1, strParam, longParam + 10));
      queue.EnqueueImmediateConsuming(() => queue.Pause());
      queue.EnqueueImmediateConsuming((intParam, strParam, longParam) => { outputInt = intParam; outputStr = strParam; outputLong = longParam; });

      queue.Start(initialParam1: 10, initialParam2: "abc", initialParam3: 100);
      queue.Unpause();

      Assert.AreEqual(13, outputInt);
      Assert.AreEqual("abcde", outputStr);
      Assert.AreEqual(130, outputLong);
    }
  }
}