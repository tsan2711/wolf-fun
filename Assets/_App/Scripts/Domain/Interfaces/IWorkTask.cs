using UnityEngine;

public interface IWorkTask
{
    void Execute();
    TaskType GetTaskType();
}
