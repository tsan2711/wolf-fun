using UnityEngine;

public class MovingState : BaseState
{
    private float moveSpeed;

    public MovingState(Worker worker, float moveSpeed) : base(worker, WorkerState.Moving)
    {
        this.moveSpeed = moveSpeed;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        if (worker.TargetPlot != null)
        {
            Vector3 targetPosition = GetPlotPosition(worker.TargetPlot);
            worker.MoveTowards(targetPosition);
        }
    }

    public override void Tick()
    {
        // Movement handled by NavMeshAgent
        // State transition handled by condition functions
    }

    public override void OnExit()
    {
        base.OnExit();
        worker.StopMoving();
    }
}
