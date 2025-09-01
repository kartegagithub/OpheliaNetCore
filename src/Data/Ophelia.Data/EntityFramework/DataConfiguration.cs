using System;

namespace Ophelia.Data.EntityFramework
{
    public class DataConfiguration : IDisposable
    {
        public DateTime MinDateTime { get; set; } = DateTime.MinValue;
        public DateTime MaxDateTime { get; set; } = DateTime.MaxValue;
        public DateTimeKind DateTimeKind { get; set; } = DateTimeKind.Unspecified;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            //Cleanup
        }
        public DataConfiguration()
        {

        }
    }
}
