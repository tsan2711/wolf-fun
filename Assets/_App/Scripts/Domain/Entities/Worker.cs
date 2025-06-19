using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Worker : MonoBehaviour
{
    [Header("Worker Settings")]
    public int Id;
    public float MoveSpeed = 2f;
    
    [Header("Work Settings")]
    public float WorkDuration = 120f; // 2 minutes

    // Components
    private Transform _transform;
    
    // State Machine
    private WorkerStateMachine stateMachine;
    
    // States
    private IdleState idle;
    private MovingState moving;
    private WorkingState working;
    private SleepingState sleeping;
    
    // Properties
    public IWorkTask CurrentTask { get; set; }
    public Plot TargetPlot { get; set; }
    public Vector3 Position => _transform.position;
    
    // Events
    public event Action<Worker> TaskCompleted;
    public event Action<Worker, WorkerState> StateChanged;

    protected virtual void Awake()
    {
        _transform = transform;
        
        // Setting up the state machine
        stateMachine = new WorkerStateMachine();
        
        // Create states
        idle = new IdleState(this);
        moving = new MovingState(this, MoveSpeed);
        working = new WorkingState(this, WorkDuration);
        sleeping = new SleepingState(this);
        
        // Add At transitions (specific state to state)
        At(idle, moving, HasTaskAndNotAtTarget());
        At(idle, working, HasTaskAndAtTarget());
        At(moving, working, IsAtTarget());
        At(moving, idle, HasNoTask());
        At(working, idle, HasNoTask());
        At(sleeping, idle, IsNotNightTime());
        
        // Add Any transitions (from any state)
        Any(sleeping, IsNightTime);
        
        // Set initial state
        stateMachine.SetState(idle);
        
        // Helper methods for setting up transitions
        void At(IState from, IState to, Func<bool> condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState state, Func<bool> condition) => stateMachine.AddAnyTransition(state, condition);
        
       // Condition functions
        Func<bool> HasTaskAndNotAtTarget() => () => CurrentTask != null && TargetPlot != null && !IsAtTargetCheck() && !IsNightTimeCheck();
        Func<bool> HasTaskAndAtTarget() => () => CurrentTask != null && TargetPlot != null && IsAtTargetCheck() && !IsNightTimeCheck();
        Func<bool> IsAtTarget() => () => IsAtTargetCheck();
        Func<bool> HasNoTask() => () => CurrentTask == null || IsNightTimeCheck();
        
        // Helper functions to avoid recursion
        bool IsAtTargetCheck() => TargetPlot != null && Vector3.Distance(Position, GetPlotPosition(TargetPlot)) < 0.5f;
        bool IsNightTimeCheck() {
            int hour = DateTime.Now.Hour;
            return hour >= 22 || hour <= 6;
        }
        Func<bool> IsNotNightTime() => () => !IsNightTimeCheck();
    }

    void Update()
    {
        stateMachine?.Tick();
    }

    public void AssignTask(IWorkTask task, Plot targetPlot)
    {
        if (IsNightTime()) return;
        
        CurrentTask = task;
        TargetPlot = targetPlot;
    }

    public void CompleteTask()
    {
        CurrentTask?.Execute();
        TaskCompleted?.Invoke(this);
        CurrentTask = null;
        TargetPlot = null;
    }

    public void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - Position).normalized;
        _transform.position += direction * MoveSpeed * Time.deltaTime;
    }

    private Vector3 GetPlotPosition(Plot plot)
    {
        return new Vector3(plot.Id % 5 * 2f, plot.Id / 5 * 2f, 0);
    }

    private bool IsAtTarget()
    {
        if (TargetPlot == null) return false;
        return Vector3.Distance(Position, GetPlotPosition(TargetPlot)) < 0.5f;
    }

    private bool IsNightTime()
    {
        int hour = DateTime.Now.Hour;
        return hour >= 22 || hour <= 6;
    }

    public bool IsAvailable => IsIdle && !IsNightTime();
    public bool IsIdle => stateMachine?.GetCurrentState() is IdleState;
    
    public WorkerState GetCurrentState()
    {
        return stateMachine?.GetCurrentState() switch
        {
            IdleState => WorkerState.Idle,
            MovingState => WorkerState.Moving,
            WorkingState => WorkerState.Working,
            SleepingState => WorkerState.Sleeping,
            _ => WorkerState.Idle
        };
    }

    public void NotifyStateChanged(WorkerState state)
    {
        StateChanged?.Invoke(this, state);
    }
}
