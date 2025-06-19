using UnityEngine;

public class SleepingState : BaseState
{
    public SleepingState(Worker worker) : base(worker, WorkerState.Sleeping) { }

    public override void OnEnter()
    {
        base.OnEnter();
        worker.CurrentTask = null;
        worker.TargetPlot = null;
    }

    public override void Tick()
    {
        // Just sleep
    }
}