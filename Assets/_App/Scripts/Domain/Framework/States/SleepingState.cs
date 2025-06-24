using UnityEngine;

public class SleepingState : BaseState
{
    public SleepingState(Worker worker) : base(worker, WorkerState.Sleeping) { }

    protected override int AnimationState => 0;

    public override void OnEnter()
    {
        base.OnEnter();
        worker.ClearCurrentTask();
    }

    public override void Tick()
    {
    }
}