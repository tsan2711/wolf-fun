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
        worker.NotifyStateChanged(stateType);
    }

    public abstract void Tick();

    public virtual void OnExit()
    {
        // Override in derived classes if needed
    }

    protected Vector3 GetPlotPosition(Plot plot)
    {
        return worker.GetPlotPosition(plot);
    }

    protected bool IsAtTarget()
    {
        return worker.IsAtTargetPosition();
    }
}