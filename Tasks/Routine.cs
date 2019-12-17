using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Tasks
{
    public interface Routine
    {
        string WorkingHourStart { get; set; }
        string WorkingHourEnd { get; set; }
        /// <summary>
        /// If false, it can not run at weekends
        /// </summary>
        bool CanRunAtWeekends { get; set; }
        /// <summary>
        /// If false, it can not run at working hours
        /// </summary>
        bool CanRunAtWorkingHours { get; set; }
        /// <summary>
        /// if true, Job can run only after midnight
        /// </summary>
        bool OnlyRunAfterMidnight { get; set; }
        /// <summary>
        /// What is the start of the week: Sunday 0, Monday 1
        /// </summary>
        int StartOfTheWeek { get; set; }
        /// <summary>
        /// How is it scheduled? Daily, weekly, etc... If it is None, it can run again without time condition
        /// </summary>
        byte IntervalType { get; set; }
        /// <summary>
        /// Running interval
        /// </summary>
        int Interval { get; set; }
        /// <summary>
        /// In which days job will run? It is comma seperated days (For example 0,1,2). if blank, it can run every day. Sunday 0, Monday 1, Tuesday 2, Wednesday 3, Thursday 4, Friday 5, Saturday 6
        /// </summary>
        string IncludedDays { get; set; }
        /// <summary>
        /// In which days job will not run? It is comma seperated days (For example 0,1,2), if blank, it can run every day. Sunday 0, Monday 1, Tuesday 2, Wednesday 3, Thursday 4, Friday 5, Saturday 6
        /// </summary>
        string ExcludedDays { get; set; }
        /// <summary>
        /// In which months job will run? It is comma seperated months (For example 0,1,2). if blank, it can run every month.
        /// </summary>
        string IncludedMonths { get; set; }
        /// <summary>
        /// In which months job will not run? It is comma seperated months (For example 0,1,2), if blank, it can run every month.
        /// </summary>
        string ExcludedMonths { get; set; }
        /// <summary>
        /// When will it start (xx.yy.zzzz)
        /// </summary>
        DateTime StartDate { get; set; }
        /// <summary>
        /// When will it start (xx:yy)
        /// </summary>
        string StartTime { get; set; }
        /// <summary>
        /// When will it end (xx.yy.zzzz)
        /// </summary>
        DateTime? EndDate { get; set; }
        /// <summary>
        /// When will it end (xx:yy)
        /// </summary>
        string EndTime { get; set; }
        /// <summary>
        /// After x occurence, it will stop
        /// </summary>
        int OccurenceLimit { get; set; }
    }

    public enum IntervalType: byte
    {
        Second = 0,
        Minute = 1,
        Hour = 2,
        Day = 3,
        Week = 4,
        Month = 5,
        Year = 6
    }
}
