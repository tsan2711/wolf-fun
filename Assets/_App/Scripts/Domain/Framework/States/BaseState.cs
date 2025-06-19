using UnityEngine;

public abstract class BaseState : IState
{
    protected Worker worker;
    protected WorkerState stateType;

    public BaseState(Worker worker, WorkerState stateType)
    {
        this.worker = worker;
        this.stateType = stateType;
    }

    public virtual void OnEnter()
    {
        Debug.Log($"Worker {worker.Id} entered {stateType} state");
        worker.NotifyStateChanged(stateType);
    }

    public abstract void Tick();

    public virtual void OnExit()
    {
        Debug.Log($"Worker {worker.Id} exited {stateType} state");
    }

    protected Vector3 GetPlotPosition(Plot plot)
    {
        return new Vector3(plot.Id % 5 * 2f, plot.Id / 5 * 2f, 0);
    }

    protected bool IsAtTarget()
    {
        if (worker.TargetPlot == null) return false;
        return Vector3.Distance(worker.Position, GetPlotPosition(worker.TargetPlot)) < 0.5f;
    }
}