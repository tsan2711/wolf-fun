using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class Worker : MonoBehaviour
{
    [Header("Worker Settings")]
    public int Id;

    [Header("Work Settings")]
    public float WorkDuration = 120f; // 2 minutes
    public float MoveSpeed = 2f;
    public Rarity Rarity = Rarity.Common;


    [HideInInspector] public float _moveStartTime;

    // Components
    private Transform _transform;
    private NavMeshAgent _navMeshAgent;
    private FarmManager _farmManager;

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
    public event Action<float> TaskStateChanged;

    public void NotifyTaskStateChanged(float progress) => TaskStateChanged?.Invoke(progress);
    public event Action<Worker, WorkerState> StateChanged;

    protected virtual void Awake()
    {
        _transform = transform;
        _navMeshAgent = GetComponent<NavMeshAgent>();

        // Find FarmManager
        _farmManager = FindAnyObjectByType<FarmManager>();

        SetUpStateMachine();
        SetUpLoadingBar();

    }

    void Update()
    {
        stateMachine?.Tick();

        // Timeout protection
        if (CurrentTask != null && GetCurrentState() == WorkerState.Moving)
        {
            float timeSinceTaskAssigned = Time.time - _moveStartTime;
            if (timeSinceTaskAssigned > 30f) // 30 second timeout
            {
                CompleteTask();
            }
        }
    }

    private void SetUpLoadingBar()
    {

    }


    private void SetUpStateMachine()
    {

        // Configure NavMeshAgent
        if (_navMeshAgent != null)
        {
            _navMeshAgent.speed = MoveSpeed;
            _navMeshAgent.stoppingDistance = 2.0f;
            _navMeshAgent.autoBraking = true;
            _navMeshAgent.radius = 0.5f;
            _navMeshAgent.height = 2f;
            _navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        }

        // Setup state machine
        stateMachine = new WorkerStateMachine();

        // Create states
        idle = new IdleState(this);
        moving = new MovingState(this, MoveSpeed);
        working = new WorkingState(this, WorkDuration);
        sleeping = new SleepingState(this);

        // Setup transitions
        At(idle, moving, HasTaskAndNotAtTarget());
        At(idle, working, HasTaskAndAtTarget());
        At(moving, working, IsAtTarget());
        At(moving, idle, HasNoTask());
        At(working, idle, HasNoTask());
        At(sleeping, idle, IsNotNightTime());

        Any(sleeping, IsNightTime);

        stateMachine.SetState(idle);

        // Helper methods
        void At(IState from, IState to, Func<bool> condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState state, Func<bool> condition) => stateMachine.AddAnyTransition(state, condition);

        // Condition functions
        Func<bool> HasTaskAndNotAtTarget() => () => CurrentTask != null && TargetPlot != null && !IsAtTargetPosition() && !IsNightTimeCheck();
        Func<bool> HasTaskAndAtTarget() => () => CurrentTask != null && TargetPlot != null && IsAtTargetPosition() && !IsNightTimeCheck();
        Func<bool> IsAtTarget() => () => IsAtTargetPosition();
        Func<bool> HasNoTask() => () => CurrentTask == null || IsNightTimeCheck();

        bool IsNightTimeCheck()
        {
            int hour = DateTime.Now.Hour;
            return hour >= 22 || hour <= 6;
        }
        Func<bool> IsNotNightTime() => () => !IsNightTimeCheck();
    }

    public Vector3 GetPlotPosition(Plot plot)
    {
        if (_farmManager != null)
        {
            return _farmManager.GetPlotWorldPosition(plot.Id);
        }

        // Fallback calculation
        return new Vector3(plot.Id % 5 * 3f, 0, plot.Id / 5 * 3f);
    }

    public bool IsAtTargetPosition()
    {
        if (TargetPlot == null) return false;

        Vector3 targetPosition = GetPlotPosition(TargetPlot);
        float distance = Vector3.Distance(Position, targetPosition);
        float threshold = 3.0f;

        return distance <= threshold;
    }

    public bool AssignTask(IWorkTask task, Plot targetPlot)
    {
        if (IsNightTime())
        {
            Debug.Log($"Worker {Id} cannot work at night");
            return false;
        }

        if (!IsAvailable)
        {
            Debug.Log($"Worker {Id} is not available (State: {GetCurrentState()})");
            return false;
        }

        var gameController = FindAnyObjectByType<GameController>();
        var farm = gameController?.Farm;

        if (farm == null)
        {
            Debug.LogError($"Worker {Id} cannot find farm reference");
            return false;
        }

        // Try to reserve the plot
        if (!farm.ReservePlot(targetPlot.Id, Id))
        {
            Debug.Log($"Worker {Id} cannot reserve plot {targetPlot.Id} - already reserved");
            return false;
        }

        CurrentTask = task;
        TargetPlot = targetPlot;
        _moveStartTime = Time.time;

        Debug.Log($"Worker {Id} successfully assigned to plot {targetPlot.Id}");
        return true;
    }

    public void CompleteTask()
    {
        // Release plot reservation
        if (TargetPlot != null)
        {
            var farm = FindAnyObjectByType<GameController>()?.Farm;
            farm?.ReleasePlot(TargetPlot.Id, Id);
            Debug.Log($"Worker {Id} released plot {TargetPlot.Id}");
        }

        CurrentTask?.Execute();
        TaskCompleted?.Invoke(this);
        CurrentTask = null;
        TargetPlot = null;

        if (_navMeshAgent != null && _navMeshAgent.isActiveAndEnabled)
        {
            _navMeshAgent.ResetPath();
        }
    }

    public void ClearCurrentTask()
    {
        if (TargetPlot != null)
        {
            var farm = FindAnyObjectByType<GameController>()?.Farm;
            farm?.ReleasePlot(TargetPlot.Id, Id);
            Debug.Log($"Worker {Id} cleared task and released plot {TargetPlot.Id}");
        }

        CurrentTask = null;
        TargetPlot = null;
    }


    public void MoveTowards(Vector3 target)
    {
        if (_navMeshAgent != null && _navMeshAgent.isActiveAndEnabled)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(target, out hit, 5f, NavMesh.AllAreas))
            {
                _navMeshAgent.SetDestination(hit.position);
            }
            else
            {
                // Try wider radius
                for (float radius = 1f; radius <= 10f; radius += 2f)
                {
                    if (NavMesh.SamplePosition(target, out hit, radius, NavMesh.AllAreas))
                    {
                        _navMeshAgent.SetDestination(hit.position);
                        return;
                    }
                }
            }
        }
        else
        {
            // Manual movement fallback
            Vector3 direction = (target - Position).normalized;
            _transform.position += direction * MoveSpeed * Time.deltaTime;
        }
    }

    public void StopMoving()
    {
        if (_navMeshAgent != null && _navMeshAgent.isActiveAndEnabled)
        {
            _navMeshAgent.ResetPath();
        }
    }

    public bool IsMoving()
    {
        if (_navMeshAgent != null && _navMeshAgent.isActiveAndEnabled)
        {
            return _navMeshAgent.hasPath && _navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance;
        }
        return false;
    }

    public bool IsNightTime()
    {
        int hour = DateTime.Now.Hour;
        return hour >= 22 || hour <= 6;
    }

    public bool IsAvailable => IsIdle && !IsNightTime();

    public bool IsIdle
    {
        get
        {
            var currentState = stateMachine?.GetCurrentState();
            return currentState is IdleState;
        }
    }

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

    private float GetDistanceToTarget()
    {
        if (TargetPlot == null) return float.MaxValue;
        return Vector3.Distance(Position, GetPlotPosition(TargetPlot));
    }
}