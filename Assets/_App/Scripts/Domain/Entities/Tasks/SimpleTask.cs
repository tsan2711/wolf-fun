using System;
[Serializable]
public class SimpleTask
{
    public Plot Plot { get; set; }
    public TaskType Type { get; set; }
    public CropType? CropType { get; set; }
}