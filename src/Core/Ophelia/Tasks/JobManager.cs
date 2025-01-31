using Ophelia.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ophelia.Tasks
{
    public abstract class JobManager
    {
        private System.Timers.Timer Timer;
        public bool Executing { get; private set; }
        public List<Job> Jobs { get; private set; }
        public List<AssemblyDefinition> ExternalAssemblies { get; protected set; }
        public bool AppStarted { get; set; }
        public virtual void Execute()
        {
            try
            {
                if (!this.Executing)
                {
                    this.Executing = true;
                    if (this.CheckSystemAvailablity())
                    {
                        this.RegisterJobs();
                        for (int i = 0; i < this.Jobs.Count; i++)
                        {
                            this.Jobs[i].Run();
                        }
                    }
                    else
                    {
                        this.OnSystemNotAvailable();
                    }
                    this.Executing = false;
                }
            }
            catch (Exception ex)
            {
                this.Executing = false;
                Console.WriteLine("JobManager.Execute:");
                Console.WriteLine(ex.ToString());
                this.OnSystemNotAvailable();
            }
        }
        protected virtual void OnSystemNotAvailable()
        {

        }
        protected virtual bool CheckSystemAvailablity()
        {
            return true;
        }
        public void StartTimer(int interval)
        {
            try
            {
                if (this.Timer != null)
                {
                    this.Timer.Stop();
                    this.Timer = null;
                }
                if (!this.AppStarted)
                    this.OnApplicationStart();
                this.AppStarted = true;

                this.Timer = new System.Timers.Timer(interval);
                this.Timer.Elapsed += new System.Timers.ElapsedEventHandler(this.TimeElapsed);
                this.Timer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("StartTimer:");
                Console.WriteLine(ex.ToString());
            }
        }
        protected virtual void OnApplicationStart()
        {

        }
        protected virtual void TimeElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Execute();
        }
        protected virtual void RegisterJobs()
        {

        }
        protected bool RegisterJob(Job job)
        {
            if (this.Jobs.Where(op => op.Key == job.Key).Any())
            {
                return false;
            }
            this.Jobs.Add(job);
            return true;
        }
        internal protected virtual void OnJobFailed(Job job, Exception ex)
        {

        }
        internal protected virtual void OnBeforeInvoke(object instance, object DataParent)
        {

        }
        internal protected virtual void OnBeforeJobExecuted(Job job)
        {

        }
        internal protected virtual void OnAfterJobExecuted(Job job, JobExecution result)
        {

        }
        internal protected virtual void OnExitingThread(Job job)
        {

        }
        public virtual DateTime GetNextExecutionTime(Job job)
        {
            if (job.Routine == null)
                return Utility.Now;

            var notWorkingInterval = false;
            var NextExecution = Utility.Now;
            if (job.Routine.StartDate > Utility.Now)
            {
                NextExecution = job.Routine.StartDate;
                notWorkingInterval = true;
            }
            var IntervalType = (IntervalType)job.Routine.IntervalType;
            if (IntervalType == IntervalType.Hour && job.Routine.Interval == 24)
                IntervalType = IntervalType.Day;
            if (IntervalType == IntervalType.Week)
                job.Routine.Interval *= 7;
            if (IntervalType == IntervalType.Day && job.Routine.Interval % 7 == 0)
                IntervalType = IntervalType.Week;
            if (!notWorkingInterval)
            {
                switch (IntervalType)
                {
                    case IntervalType.Second:
                        NextExecution = Utility.Now.AddSeconds(job.Routine.Interval);
                        break;
                    case IntervalType.Minute:
                        NextExecution = Utility.Now.AddMinutes(job.Routine.Interval);
                        break;
                    case IntervalType.Hour:
                        NextExecution = Utility.Now.AddHours(job.Routine.Interval);
                        break;
                    case IntervalType.Day:
                        NextExecution = Utility.Now.AddDays(job.Routine.Interval);
                        break;
                    case IntervalType.Week:
                        NextExecution = Utility.Now.AddDays(job.Routine.Interval);
                        break;
                    case IntervalType.Month:
                        NextExecution = Utility.Now.AddMonths(job.Routine.Interval);
                        break;
                    case IntervalType.Year:
                        NextExecution = Utility.Now.AddYears(job.Routine.Interval);
                        break;
                }
            }
            if (job.Routine.OnlyRunAfterMidnight && string.IsNullOrEmpty(job.Routine.StartTime))
                job.Routine.StartTime = "01:00";

            if (!string.IsNullOrEmpty(job.Routine.StartTime))
            {
                NextExecution = NextExecution.SetTime(job.Routine.StartTime, "HH:mm");
            }

            if (!job.Routine.CanRunAtWorkingHours)
            {
                var WorkingHourStart = NextExecution.SetTime(job.Routine.WorkingHourStart, "HH:mm");
                var WorkingHourEnd = NextExecution.SetTime(job.Routine.WorkingHourEnd, "HH:mm");
                if (NextExecution > WorkingHourStart && NextExecution < WorkingHourEnd)
                {
                    if ((NextExecution.TimeOfDay - WorkingHourStart.TimeOfDay).TotalHours < 0)
                        NextExecution = NextExecution.SetTime(job.Routine.WorkingHourStart, "HH:mm").AddHours(-1);
                    else
                        NextExecution = NextExecution.SetTime(job.Routine.WorkingHourEnd, "HH:mm").AddHours(1);
                }
            }
            if (!job.Routine.CanRunAtWeekends && NextExecution.IsWeekend())
            {
                if (NextExecution.DayOfWeek == DayOfWeek.Saturday)
                    NextExecution = NextExecution.AddDays(2);
                else if (NextExecution.DayOfWeek == DayOfWeek.Sunday)
                    NextExecution = NextExecution.AddDays(1);
            }
            return NextExecution;
        }
        internal protected virtual bool CanRunJob(Job job)
        {
            if (job.LastExecutionStatus == JobExecutionStatus.Running)
                return false;
            if (job.Routine == null)
                return true;

            if (!job.Routine.CanRunAtWeekends && Utility.Now.IsWeekend())
                return false;
            if (!job.Routine.CanRunAtWorkingHours && !Utility.Now.IsWeekend() && Utility.Now.IsWithinWorkingHours())
                return false;
            if (job.Routine.OnlyRunAfterMidnight && !Utility.Now.IsAfterMidnight())
                return false;
            if (!string.IsNullOrEmpty(job.Routine.IncludedDays) && !job.Routine.IncludedDays.Contains(((int)Utility.Now.DayOfWeek).ToString()))
                return false;
            if (!string.IsNullOrEmpty(job.Routine.ExcludedDays) && job.Routine.ExcludedDays.Contains(((int)Utility.Now.DayOfWeek).ToString()))
                return false;
            if (!string.IsNullOrEmpty(job.Routine.IncludedMonths) && !job.Routine.IncludedMonths.Contains(Utility.Now.Month.ToString()))
                return false;
            if (!string.IsNullOrEmpty(job.Routine.ExcludedMonths) && job.Routine.ExcludedMonths.Contains(Utility.Now.Month.ToString()))
                return false;
            if (job.Routine.OccurenceLimit > 0 && job.OccurenceIndex >= job.Routine.OccurenceLimit)
                return false;
            if (job.Routine.EndDate.GetValueOrDefault(DateTime.MinValue) > DateTime.MinValue && Utility.Now > job.Routine.EndDate.Value)
                return false;
            if (job.NextExecutionTime.GetValueOrDefault(DateTime.MinValue) > DateTime.MinValue && job.NextExecutionTime.Value > Utility.Now)
                return false;
            if (!string.IsNullOrEmpty(job.Routine.EndTime) && Utility.Now > Convert.ToDateTime(Utility.Now.ToShortDateString() + " " + job.Routine.EndTime))
                return false;
            return true;
        }
        internal Assembly GetAssembly(String assemblyName)
        {
            if (this.ExternalAssemblies.Where(op => assemblyName.StartsWith(op.RootNameSpace)).Any())
            {
                var info = this.ExternalAssemblies.Where(op => assemblyName.StartsWith(op.RootNameSpace)).FirstOrDefault();
                if (System.IO.File.Exists(info.FilePath))
                    return Assembly.LoadFile(info.FilePath);
            }
            return AppDomain.CurrentDomain.GetAssemblies().Where(op => op.FullName.Contains(assemblyName)).FirstOrDefault();
        }
        public JobManager()
        {
            this.Jobs = new List<Job>();
            this.ExternalAssemblies = new List<AssemblyDefinition>();
        }
    }
}
