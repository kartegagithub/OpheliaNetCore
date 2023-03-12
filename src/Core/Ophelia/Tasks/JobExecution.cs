namespace Ophelia.Tasks
{
    public class JobExecution
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public JobExecutionStatus Status { get; set; }
    }
    public enum JobExecutionStatus
    {
        Waiting = 0,
        Running = 1,
        Finished = 2,
        Aborted = 3,
        Failed = 4,
        Cancelled = 5
    }
}
