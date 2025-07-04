using UnityEngine;

public class MovingState : BaseState
{
    private float moveSpeed;

    protected override int AnimationState => 1;

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
    }

    public override void OnExit()
    {
        base.OnExit();
        worker.StopMoving();
    }
}
