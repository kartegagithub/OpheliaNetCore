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
                    this.RegisterJobs();
                    for (int i = 0; i < this.Jobs.Count; i++)
                    {
                        this.Jobs[i].Run();
                    }
                    this.Executing = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("JobManager.Execute:");
                Console.WriteLine(ex.ToString());
            }
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
                return DateTime.Now;

            var NextExecution = DateTime.Now;
            var IntervalType = (IntervalType)job.Routine.IntervalType;
            if (IntervalType == IntervalType.Hour && job.Routine.Interval == 24)
                IntervalType = IntervalType.Day;
            if (IntervalType == IntervalType.Week)
                job.Routine.Interval *= 7;
            if (IntervalType == IntervalType.Day && job.Routine.Interval % 7 == 0)
                IntervalType = IntervalType.Week;
            switch (IntervalType)
            {
                case IntervalType.Second:
                    NextExecution = DateTime.Now.AddSeconds(job.Routine.Interval);
                    break;
                case IntervalType.Minute:
                    NextExecution = DateTime.Now.AddMinutes(job.Routine.Interval);
                    break;
                case IntervalType.Hour:
                    NextExecution = DateTime.Now.AddHours(job.Routine.Interval);
                    break;
                case IntervalType.Day:
                    NextExecution = DateTime.Now.AddDays(job.Routine.Interval);
                    break;
                case IntervalType.Week:
                    NextExecution = DateTime.Now.AddDays(job.Routine.Interval);
                    break;
                case IntervalType.Month:
                    NextExecution = DateTime.Now.AddMonths(job.Routine.Interval);
                    break;
                case IntervalType.Year:
                    NextExecution = DateTime.Now.AddYears(job.Routine.Interval);
                    break;
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

            if (!job.Routine.CanRunAtWeekends && DateTime.Now.IsWeekend())
                return false;
            if (!job.Routine.CanRunAtWorkingHours && !DateTime.Now.IsWeekend() && DateTime.Now.IsWithinWorkingHours())
                return false;
            if (job.Routine.OnlyRunAfterMidnight && !DateTime.Now.IsAfterMidnight())
                return false;
            if (!string.IsNullOrEmpty(job.Routine.IncludedDays) && !job.Routine.IncludedDays.Contains(((int)DateTime.Now.DayOfWeek).ToString()))
                return false;
            if (!string.IsNullOrEmpty(job.Routine.ExcludedDays) && job.Routine.ExcludedDays.Contains(((int)DateTime.Now.DayOfWeek).ToString()))
                return false;
            if (!string.IsNullOrEmpty(job.Routine.IncludedMonths) && !job.Routine.IncludedMonths.Contains(DateTime.Now.Month.ToString()))
                return false;
            if (!string.IsNullOrEmpty(job.Routine.ExcludedMonths) && job.Routine.ExcludedMonths.Contains(DateTime.Now.Month.ToString()))
                return false;
            if (job.Routine.OccurenceLimit > 0 && job.OccurenceIndex >= job.Routine.OccurenceLimit)
                return false;
            if (job.Routine.EndDate.GetValueOrDefault(DateTime.MinValue) > DateTime.MinValue && DateTime.Now > job.Routine.EndDate.Value)
                return false;
            if (job.NextExecutionTime.GetValueOrDefault(DateTime.MinValue) > DateTime.MinValue && job.NextExecutionTime.Value > DateTime.Now)
                return false;
            if (!string.IsNullOrEmpty(job.Routine.EndTime) && DateTime.Now > Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + job.Routine.EndTime))
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
