using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TimerManagement : MonoBehaviour
{
    static Dictionary<Timer, float> TimerDictionary = new Dictionary<Timer, float>();
    static Dictionary<Timer, float> RealTimerDictionary = new Dictionary<Timer, float>();
    static Queue<Timer> timers = new Queue<Timer>();
    static float time;
    static float realTime;

    void LateUpdate()
    {
        realTime = Time.realtimeSinceStartup;
        var enumerator = RealTimerDictionary.GetEnumerator();
        while (enumerator.MoveNext()) {
            if (enumerator.Current.Key.IsActive && realTime >= enumerator.Current.Value)
                timers.Enqueue(enumerator.Current.Key);
        }
        enumerator.Dispose();
        while (timers.Count > 0)
        {
            Timer timer = timers.Dequeue();
            if (timer != null)
            {
                timer.NormalExcute = true;
                timer.Invoke();
            }
        }
    }

    void FixedUpdate()
    {
        time = Time.time;
        var enumerator = TimerDictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Key.IsActive && time >= enumerator.Current.Value)
                timers.Enqueue(enumerator.Current.Key);
        }
        enumerator.Dispose();
        while (timers.Count > 0)
        {
            Timer timer = timers.Dequeue();
            if (timer != null)
            {
                timer.NormalExcute = true;
                timer.Invoke();
            }
        }
    }

    public static void ClearAll() {
        TimerDictionary.Clear();
        RealTimerDictionary.Clear();
    }

    /// <summary>
    /// 实例化一个计时器并直接开始启用
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="number"></param>
    /// <param name="useRealTime"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static Timer RegisterTimer(float interval, int number, bool useRealTime, Action callback, bool autoDestroy = true) {
        Timer timer = new Timer(interval, number, useRealTime, callback, autoDestroy);
        timer.IsActive = true;
        return timer;
    }

    /// <summary>
    /// 实例化一个计时器并直接开始启用
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="number"></param>
    /// <param name="useRealTime"></param>
    /// <param name="callbackWithNumber">携带当前执行次数的回调</param>
    /// <returns></returns>
    public static Timer RegisterTimer(float interval, int number, bool useRealTime, Action<int> callbackWithNumber, bool autoDestroy = true)
    {
        Timer timer = new Timer(interval, number, useRealTime, callbackWithNumber, autoDestroy);
        timer.IsActive = true;
        return timer;
    }

    /// <summary>
    /// 移除一个计时器，移除后处于IsActive = false状态，可被回收
    /// </summary>
    /// <param name="timer"></param>
    public static void RemoveTimer(Timer timer) {
        if (timer == null)
            return;
        timer.IsActive = false;
        if (timer.UseRealtime)
        {
            if (RealTimerDictionary.ContainsKey(timer))
                RealTimerDictionary.Remove(timer);
        }
        else
        {
            if (TimerDictionary.ContainsKey(timer))
                TimerDictionary.Remove(timer);
        }
    }

    /// <summary>
    /// 获取当前时间戳
    /// </summary>
    /// <returns></returns>
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }

    /// <summary>
    /// 获取一个DateTime的时间戳
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static long GetTimeStamp(DateTime dateTime)
    {
        TimeSpan ts = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }

    /// <summary>
    /// 时间戳转换为DateTime
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public static DateTime TimeStampToDateTime(long timeStamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timeStamp);
    }

    /// <summary>  
    /// 秒转换小时  
    /// </summary>  
    /// <param name="time"></param>  
    /// <returns></returns>  
    public static string SecondToHour(long time)
    {
        StringBuilder str = new StringBuilder();
        long hour = 0;
        long minute = 0;
        long second = time;

        if (second > 60)
        {
            minute = second / 60;
            second = second % 60;
        }
        if (minute > 60)
        {
            hour = minute / 60;
            minute = minute % 60;
        }
        if (hour > 0 && hour < 10)
        {
            str.Append("0" + hour);
        }
        else if (hour >= 10)
        {
            str.Append(hour);
        }
        else
        {
            str.Append("00");
        }
        str.Append(":");
        if (minute > 0 && minute < 10)
        {
            str.Append("0" + minute);
        }
        else if (minute >= 10)
        {
            str.Append(minute);
        }
        else
        {
            str.Append("00");
        }
        str.Append(":");
        if (second > 0 && second < 10)
        {
            str.Append("0" + second);
        }
        else if (second >= 10)
        {
            str.Append(second);
        }
        else
        {
            str.Append("00");
        }
        return str.ToString();
    }

    /// <summary>
    /// 计时器结束时仅设置IsActive = false未启用状态（在此期间可以选择增加次数并重新启用它，或是ReStart重新开始）
    /// 即它不会被回收，若不再需要请调用RemoveTimer方法
    /// </summary>
    public class Timer : IEquatable<Timer>
    {
        static int Index;

        int index;
        Action callback;
        Action customCallback;

        bool autoDestroy;

        /// <summary>
        /// 是否使用真实时间
        /// </summary>
        public bool UseRealtime { get; private set; }

        /// <summary>
        /// 检查点之间间隔时间
        /// </summary>
        public float Interval { get; private set; }

        /// <summary>
        /// 总次数，小于等于零为无限计时器
        /// </summary>
        public int Number { get; private set; }

        int currentNumber;
        /// <summary>
        /// 当前以执行的次数
        /// </summary>
        public int CurrentNumber
        {
            get => currentNumber;
            private set
            {
                currentNumber = value;
                if (Number > 0 && currentNumber > Number)
                    currentNumber = Number;
            }
        }

        /// <summary>
        /// 剩余次数
        /// </summary>
        public int RemainingNumber
        {
            get
            {
                if (Number < 0)
                    return -1;
                else
                    return Number - CurrentNumber;
            }
        }

        float _untilNextNumberTime;
        /// <summary>
        /// 暂停时存储的距离下一个检查点剩余时间
        /// </summary>
        float untilNextNumberTime
        {
            get => _untilNextNumberTime;
            set
            {
                _untilNextNumberTime = value;
                if (value < 0)
                    _untilNextNumberTime = 0;
            }
        }
        /// <summary>
        /// 距离下一个检查点时间，计时器为结束状态返回0
        /// </summary>
        public float UntilNextNumberTime
        {
            get
            {
                if (Number > 0 && CurrentNumber == Number)
                    return 0;
                if (isActive)
                {
                    if (UseRealtime)
                        return RealTimerDictionary[this] - realTime;
                    else
                        return TimerDictionary[this] - time;
                }
                else
                {
                    if (untilNextNumberTime == 0)
                        return Interval;
                    else
                        return untilNextNumberTime;
                }
            }
        }

        /// <summary>
        /// 剩余总时间，无限次数返回-1
        /// </summary>
        public float RemainingTotalTime
        {
            get
            {
                if (Number > 0)
                {
                    if (CurrentNumber == Number)
                        return 0;
                    else
                        return UntilNextNumberTime + (Number - CurrentNumber - 1) * Interval;
                }
                else
                    return -1;
            }
        }

        float startTime;
        public float RunningTime 
        {
            get 
            {
                if (UseRealtime)
                    return Time.realtimeSinceStartup - startTime;
                else
                    return Time.time - startTime;
            }
        }

        public bool NormalExcute { get; set; }

        bool isActive;
        /// <summary>
        /// 是否启用计时器
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (value)
                {
                    if (isActive != value)
                    {
                        if (UseRealtime)
                            RealTimerDictionary[this] = realTime + UntilNextNumberTime;
                        else
                            TimerDictionary[this] = time + UntilNextNumberTime;
                    }
                }
                else
                {
                    if (isActive != value)
                    {
                        if (UseRealtime)
                            untilNextNumberTime = RealTimerDictionary[this] - realTime;
                        else
                            untilNextNumberTime = TimerDictionary[this] - time;
                    }
                }
                isActive = value;
            }
        }

        public bool Stoped { get => RemainingNumber == 0; }

        public Timer(float interval, int number, bool useRealTime, Action callback, bool autoDestroy = true)
        {
            Index++;
            index = Index;
            customCallback = () => { callback?.Invoke(); };
            this.autoDestroy = autoDestroy;
            Init(interval, number, useRealTime);
        }
        public Timer(float interval, int number, bool useRealTime, Action<int> callbackWithNumber, bool autoDestroy = true)
        {
            Index++;
            index = Index;
            customCallback = () => { callbackWithNumber?.Invoke(CurrentNumber); };
            this.autoDestroy = autoDestroy;
            Init(interval, number, useRealTime);
        }
        void Init(float interval, int number, bool useRealTime)
        {
            Interval = interval;
            Number = number;
            UseRealtime = useRealTime;
            startTime = useRealTime ? Time.realtimeSinceStartup : Time.time;
            callback = () =>
            {
                if (UseRealtime)
                    RealTimerDictionary[this] = realTime + Interval;
                else
                    TimerDictionary[this] = time + Interval;
                if (Number <= 0 || CurrentNumber < Number)
                {
                    CurrentNumber++;
                    customCallback?.Invoke();
                }
                if (Number > 0 && CurrentNumber == Number)
                {
                    IsActive = false;
                    untilNextNumberTime = 0;
                    if (autoDestroy)
                        RemoveTimer(this);
                }
            };
        }

        public void Delay(float time)
        {
            if (time > 0)
            {
                if (UseRealtime)
                    RealTimerDictionary[this] += time;
                else
                    TimerDictionary[this] += time;
            }
        }

        /// <summary>
        /// 增加或减少次数，不适用无限计时器
        /// </summary>
        /// <param name="number"></param>
        public void AddNumber(int number)
        {
            if (Number > 0)
            {
                Number += number;
                CurrentNumber = CurrentNumber;
            }
            else
                Debug.LogError("Timer: AddNumber error, timer is Infinite");
        }

        /// <summary>
        /// 置零重启
        /// </summary>
        public void ReStart()
        {
            IsActive = false;
            untilNextNumberTime = 0;
            CurrentNumber = 0;
            IsActive = true;
        }

        public void ReStart(float interval, int number)
        {
            IsActive = false;
            untilNextNumberTime = 0;
            CurrentNumber = 0;
            Interval = interval;
            Number = number;
            IsActive = true;
        }

        /// <summary>
        /// 改变回调函数
        /// </summary>
        /// <param name="callback"></param>
        public void ChangeCallback(Action callback)
        {
            customCallback = () => { callback?.Invoke(); };
        }
        public void ChangeCallback(Action<int> callbackWithNumber)
        {
            customCallback = () => { callbackWithNumber?.Invoke(CurrentNumber); };
        }

        /// <summary>
        /// 执行回调
        /// </summary>
        public void Invoke()
        {
            callback?.Invoke();
            NormalExcute = false;
        }

        public bool Equals(Timer other)
        {
            return index == other.index;
        }

        public override int GetHashCode()
        {
            return index;
        }
    }
}