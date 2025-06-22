using UnityEngine;

public class WorkingState : BaseState
{
    private float workDuration;
    private float workTimer;
    private Animator animator;

    protected override int AnimationState => 0;

    public WorkingState(Worker worker, float workDuration) : base(worker, WorkerState.Working)
    {
        this.workDuration = workDuration;
        this.animator = worker.GetComponent<Animator>();
    }

    public override void OnEnter()
    {
        base.OnEnter();

        workDuration = worker.WorkDuration;
        workTimer = 0f;

        // Start work animation
        if (animator != null)
        {
            animator.SetBool("IsWorking", true);
        }

        // Face the target plot
        if (worker.TargetPlot != null)
        {
            Vector3 plotPosition = GetPlotPosition(worker.TargetPlot);
            Vector3 direction = (plotPosition - worker.Position).normalized;
            if (direction != Vector3.zero)
            {
                worker.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        Debug.Log($"Worker {worker.Id} started working - duration: {workDuration}s");
    }

    public override void Tick()
    {
        Debug.Log("WorkingState Tick called");

        workTimer += Time.deltaTime;
        
        // Calculate progress (0.0 to 1.0)
        float progress = Mathf.Clamp01(workTimer / workDuration);
        
        // Notify progress change
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

        // Stop work animation
        if (animator != null)
        {
            animator.SetBool("IsWorking", false);
        }

        Debug.Log($"Worker {worker.Id} exited working state");
    }
}
