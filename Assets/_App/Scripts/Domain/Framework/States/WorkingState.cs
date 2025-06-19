using UnityEngine;

public class WorkingState : BaseState
{
    private float workDuration;
    private float workTimer;

    public WorkingState(Worker worker, float workDuration) : base(worker, WorkerState.Working)
    {
        this.workDuration = workDuration;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        workTimer = 0f;
        Debug.Log($"Worker {worker.Id} started working on plot {worker.TargetPlot?.Id}");
    }

    public override void Tick()
    {
        workTimer += Time.deltaTime;
        
        if (workTimer >= workDuration)
        {
            worker.CompleteTask();
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        workTimer = 0f;
    }
}