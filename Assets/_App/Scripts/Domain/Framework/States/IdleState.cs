using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(Worker worker) : base(worker, WorkerState.Idle) { }

    protected override int AnimationState => 0;

    public override void Tick()
    {
        // Just wait for task assignment
    }
}
