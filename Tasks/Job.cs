using System;
using System.Collections.Generic;
using System.Linq;

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
        public string Parameters { get; set; }
        public bool OneTimeJob { get; set; }
        public DateTime? LastExecutionTime { get; set; }
        public JobExecutionStatus LastExecutionStatus { get; set; }
        public DateTime? NextExecutionTime { get; set; }
        public Routine Routine { get; set; }
        public long OccurenceIndex { get; set; }
        public System.Threading.Thread CurrentThread { get; private set; }
        public void Run()
        {
            try
            {
                if (this.CurrentThread == null && this.Manager.CanRunJob(this))
                {
                    try
                    {
                        if (this.LastExecutionStatus != JobExecutionStatus.Running)
                        {
                            this.LastExecutionStatus = JobExecutionStatus.Running;
                            this.Manager.OnBeforeJobExecuted(this);
                            this.CurrentThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.RunInternal));
                            this.CurrentThread.Priority = System.Threading.ThreadPriority.Lowest;
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
            catch (Exception ex)
            {
                Console.WriteLine("Job.Run:");
                Console.WriteLine(ex.ToString());
            }

        }

        protected virtual void RunInternal()
        {
            var result = new JobExecution();
            try
            {
                var assembly = this.Manager.GetAssembly(this.AssemblyName);
                if (assembly == null)
                    throw new Exception("Assembly could not be loaded " + this.AssemblyName);
                var type = assembly.GetType(this.ClassName);
                if (type == null)
                    throw new Exception("Class " + this.ClassName + " could not be loaded from assembly " + this.AssemblyName);

                var methods = type.GetMethods().Where(op => op.Name == this.MethodName).ToList();
                if (methods.Count == 0)
                    throw new Exception("Method " + this.MethodName + " not found at type " + this.ClassName + " from assembly " + this.AssemblyName);

                var instance = Activator.CreateInstance(type, this.DataParent);
                var methodInfo = methods.FirstOrDefault();
                this.Manager.OnBeforeInvoke(instance, this.DataParent);
                if (string.IsNullOrEmpty(this.Parameters))
                {
                    methodInfo.Invoke(instance, null);
                    this.LastExecutionStatus = JobExecutionStatus.Finished;
                }
                else
                {
                    try
                    {
                        var parameters = this.Parameters.FromJson<Dictionary<string, object>>();
                        var parameterValues = new List<object>();
                        var counter = 0;
                        foreach (var item in methodInfo.GetParameters())
                        {
                            parameterValues.Add(item.ParameterType.ConvertData(parameters[item.Name]));
                            counter++;
                        }
                        methodInfo.Invoke(instance, parameterValues.ToArray());
                        this.LastExecutionStatus = JobExecutionStatus.Finished;
                    }
                    catch (Exception)
                    {
                        this.LastExecutionStatus = JobExecutionStatus.Failed;
                    }
                }
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
