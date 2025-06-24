using UnityEngine;

public abstract class BaseState : IState
{
    protected Worker worker;
    protected WorkerState stateType;
    protected abstract int AnimationState { get; }

    public BaseState(Worker worker, WorkerState stateType)
    {
        this.worker = worker;
        this.stateType = stateType;
    }

    public virtual void OnEnter()
    {
        worker.NotifyStateChanged(stateType);
        worker.SetAnimationState(AnimationState);
    }

    public abstract void Tick();

    public virtual void OnExit()
    {
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