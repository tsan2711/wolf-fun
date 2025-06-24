using UnityEngine;

public class WorkingState : BaseState
{
    private float workDuration;
    private float workTimer;

    protected override int AnimationState => 0;

    public WorkingState(Worker worker, float workDuration) : base(worker, WorkerState.Working)
    {
        this.workDuration = workDuration;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        workDuration = worker.WorkDuration;
        workTimer = 0f;

        if (worker.TargetPlot != null)
        {
            Vector3 plotPosition = GetPlotPosition(worker.TargetPlot);
            Vector3 direction = (plotPosition - worker.Position).normalized;
            if (direction != Vector3.zero)
            {
                worker.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

    }

    public override void Tick()
    {
        workTimer += Time.deltaTime;
        
        float progress = Mathf.Clamp01(workTimer / workDuration);
        
        worker.NotifyTaskStateChanged(progress);

        if (workTimer >= workDuration)
        {
            Debug.Log($"Worker {worker.Id} work completed after {workTimer:F1}s");
            worker.CompleteTask();
        }
    }

    public override void OnExit()
    {
        base.OnExit();

        Debug.Log($"Worker {worker.Id} exited working state");
    }
}
