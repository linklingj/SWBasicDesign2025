using UnityEngine;

public class Timer
{
    public float StartTime { get; set; }
    public float Duration { get; set; } = float.PositiveInfinity;

    public float Elapsed => Time.time - StartTime;

    public float Remaining => Duration - Elapsed;

    public float Progress => Duration == 0f ? 1 : Mathf.Clamp01(Elapsed / Duration);

    public bool IsFinished => Time.time >= StartTime + Duration;

    public bool IsSet => !float.IsPositiveInfinity(Duration);

    /// <summary>
    /// Resets the timer and Finishes it immediately
    /// </summary>
    public void Set()
    {
        StartTime = Time.time;
        Duration = 0f;
    }

    /// <summary>
    /// Sets the timer
    /// </summary>
    /// <param name="duration">the duration</param>
    public void Set(float duration)
    {
        StartTime = Time.time;
        Duration = duration;
    }

    /// <summary>
    /// Sets the StartTime without changing the EndTime
    /// </summary>
    /// <param name="startTime"></param>
    public void SetStartTime(float startTime)
    {
        Duration += StartTime - startTime;
        StartTime = startTime;
    }

    /// <summary>
    /// Adds some time to the timer (StartTime doesn't change; Progress won't reset)
    /// </summary>
    /// <param name="duration">duration to add</param>
    public void Add(float duration)
    {
        if (!IsSet)
        {
            Set();
        }
        Duration += duration;
    }

    /// <summary>
    /// Unsets the timer. It won't finish until another Set() is called
    /// </summary>
    public void Unset()
    {
        Duration = float.PositiveInfinity;
    }

    public class Unscaled
    {
        public float StartTime { get; set; }
        public float Duration { get; set; } = float.PositiveInfinity;

        public float Elapsed => Time.unscaledTime - StartTime;

        public float Remaining => Duration - Elapsed;

        public float Progress => Duration == 0f ? 1 : Mathf.Clamp01(Elapsed / Duration);

        public bool IsFinished => Time.unscaledTime >= StartTime + Duration;

        public bool IsSet => !float.IsPositiveInfinity(Duration);

        /// <summary>
        /// Resets the timer and Finishes it immediately
        /// </summary>
        public void Set()
        {
            StartTime = Time.unscaledTime;
            Duration = 0f;
        }

        /// <summary>
        /// Sets the timer
        /// </summary>
        /// <param name="duration">the duration</param>
        public void Set(float duration)
        {
            StartTime = Time.unscaledTime;
            Duration = duration;
        }

        /// <summary>
        /// Sets the StartTime without changing the EndTime
        /// </summary>
        /// <param name="startTime"></param>
        public void SetStartTime(float startTime)
        {
            Duration += StartTime - startTime;
            StartTime = startTime;
        }

        /// <summary>
        /// Adds some time to the timer (StartTime doesn't change; Progress won't reset)
        /// </summary>
        /// <param name="duration">duration to add</param>
        public void Add(float duration)
        {
            if (!IsSet)
            {
                Set();
            }
            Duration += duration;
        }

        /// <summary>
        /// Unsets the timer. It won't finish until another Set() is called
        /// </summary>
        public void Unset()
        {
            Duration = float.PositiveInfinity;
        }
    }
}