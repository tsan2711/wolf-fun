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
        Debug.Log($"Worker {worker.Id} moving to plot {worker.TargetPlot?.Id}");
    }

    public override void Tick()
    {
        if (worker.TargetPlot == null) return;

        Vector3 targetPosition = GetPlotPosition(worker.TargetPlot);
        worker.MoveTowards(targetPosition);
    }
}