using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Tasks
{
    public class Job
    {
        private JobManager Manager;
        public string Key { get; set; }
        public object DataParent { get; set; }
        public object Data { get; set; }
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public DateTime? LastExecutionTime { get; set; }
        public JobExecutionStatus LastExecutionStatus { get; set; }
        public DateTime? NextExecutionTime { get; set; }
        public Routine Routine { get; set; }
        public long OccurenceIndex { get; set; }
        public System.Threading.Thread CurrentThread { get; private set; }
        public void Run()
        {
            if (this.CurrentThread == null && this.Manager.CanRunJob(this))
            {
                try
                {
                    if(this.LastExecutionStatus != JobExecutionStatus.Running)
                    {
                        this.LastExecutionStatus = JobExecutionStatus.Running;
                        this.Manager.OnBeforeJobExecuted(this);
                        this.CurrentThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.RunInternal));
                        this.CurrentThread.Start();
                    }
                }
                catch (Exception ex)
                {
                    this.Manager.OnJobFailed(this, ex);
                    this.Manager.OnAfterJobExecuted(this, new JobExecution() { Code = "ERR1", Description = ex.Message + " " + ex.StackTrace, Status = JobExecutionStatus.Aborted });
                    this.LastExecutionStatus = JobExecutionStatus.Aborted;
                    this.SetNextExecution();
                    this.CurrentThread = null;
                }
            }
        }

        protected virtual void RunInternal()
        {
            var result = new JobExecution();
            try
            {
                var assembly = this.Manager.GetAssembly(this.AssemblyName);
                if(assembly == null)
                    throw new Exception("Assembly could not be loaded " + this.AssemblyName);
                var type = assembly.GetType(this.ClassName);
                if(type == null)
                    throw new Exception("Class " + this.ClassName + " could not be loaded from assembly " + this.AssemblyName);

                var methods = type.GetMethods().Where(op => op.Name == this.MethodName).ToList();
                if(methods.Count == 0)
                    throw new Exception("Method " + this.MethodName + " not found at type " + this.ClassName + " from assembly " + this.AssemblyName);

                var instance = Activator.CreateInstance(type, this.DataParent);
                var methodInfo = methods.FirstOrDefault();
                methodInfo.Invoke(instance, null);
                this.LastExecutionStatus = JobExecutionStatus.Finished;
                result.Status = this.LastExecutionStatus;
            }
            catch (Exception ex)
            {
                this.Manager.OnJobFailed(this, ex);
                result.Status = JobExecutionStatus.Failed;
                result.Code = "ERR2";
                result.Description = ex.Message + " " + ex.StackTrace;
                this.LastExecutionStatus = JobExecutionStatus.Failed;
            }
            finally
            {
                this.Manager.OnAfterJobExecuted(this, result);
                this.SetNextExecution();
                this.Manager.OnExitingThread(this);
                this.CurrentThread = null;
            }
        }

        private void SetNextExecution()
        {
            this.NextExecutionTime = this.Manager.GetNextExecutionTime(this);
            this.LastExecutionTime = DateTime.Now;
        }
        public Job(JobManager Manager)
        {
            this.Manager = Manager;
        }
    }
}
