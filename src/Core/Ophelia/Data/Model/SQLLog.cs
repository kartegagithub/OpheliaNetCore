using System;

namespace Ophelia.Data.Model
{
    public class SQLLog
    {
        public string Text { get; set; }
        public object[] Params { get; set; }
        public double Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public void Start()
        {
            this.StartTime = DateTime.Now;
        }
        public void Finish()
        {
            this.EndTime = DateTime.Now;
            this.Duration = this.EndTime.Subtract(this.StartTime).TotalMilliseconds;
        }
        public SQLLog(string text, object[] parameters)
        {
            this.Text = text;
            this.Params = parameters;
        }
    }
}
