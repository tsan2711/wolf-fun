using UnityEngine;

public class SleepingState : BaseState
{
    public SleepingState(Worker worker) : base(worker, WorkerState.Sleeping) { }

    public override void OnEnter()
    {
        base.OnEnter();
        worker.ClearCurrentTask();
    }

    public override void Tick()
    {
        // Sleep until morning
    }
}