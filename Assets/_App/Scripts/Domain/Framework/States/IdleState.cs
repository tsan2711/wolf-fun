using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(Worker worker) : base(worker, WorkerState.Idle) { }

    public override void Tick()
    {
        // Just wait for task assignment
    }
}
