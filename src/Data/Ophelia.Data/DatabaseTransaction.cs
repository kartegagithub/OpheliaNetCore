using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Ophelia.Data
{
    public class DatabaseTransaction : DbTransaction
    {
        internal DbTransaction InternalTransaction { get; set; }
        private Connection DatabaseConnection { get; set; }
        public bool IsDisposed { get; set; }
        internal DatabaseTransactionStatus Status { get; set; }   

        public override IsolationLevel IsolationLevel { get { return this.InternalTransaction.IsolationLevel; } }
        protected override DbConnection? DbConnection { get { return this.InternalTransaction.Connection; } }

        protected override void Dispose(bool disposing)
        {
            this.IsDisposed = true;
            this.Status = DatabaseTransactionStatus.Disposed;
            base.Dispose(disposing);
            this.DatabaseConnection.ReleaseCurrentTransaction();
            this.InternalTransaction.Dispose();
            this.InternalTransaction = null;
            this.DatabaseConnection = null;
        }
        public override Task CommitAsync(CancellationToken cancellationToken = default)
        {
            var task = this.InternalTransaction.CommitAsync(cancellationToken);
            this.Status = DatabaseTransactionStatus.Committed;
            this.DatabaseConnection.ReleaseCurrentTransaction();
            return task;
        }
        public override void Release(string savepointName)
        {
            this.InternalTransaction.Release(savepointName);
            this.Status = DatabaseTransactionStatus.Released;
        }
        public override Task ReleaseAsync(string savepointName, CancellationToken cancellationToken = default)
        {
            var task = this.InternalTransaction.ReleaseAsync(savepointName, cancellationToken);
            this.Status = DatabaseTransactionStatus.Released;
            return task;
        }
        public override void Rollback(string savepointName)
        {
            this.InternalTransaction.Rollback(savepointName);
            this.Status = DatabaseTransactionStatus.RolledBack;
            this.DatabaseConnection.ReleaseCurrentTransaction();
        }
        public override Task SaveAsync(string savepointName, CancellationToken cancellationToken = default)
        {
            var task = this.InternalTransaction.SaveAsync(savepointName, cancellationToken);
            this.Status = DatabaseTransactionStatus.Saved;
            return task;
        }
        public override Task RollbackAsync(string savepointName, CancellationToken cancellationToken = default)
        {
            var task = this.InternalTransaction.RollbackAsync(savepointName, cancellationToken);
            this.Status = DatabaseTransactionStatus.RolledBack;
            this.DatabaseConnection.ReleaseCurrentTransaction();
            return task;
        }
        public override Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            var task = this.InternalTransaction.RollbackAsync(cancellationToken);
            this.Status = DatabaseTransactionStatus.RolledBack;
            this.DatabaseConnection.ReleaseCurrentTransaction();
            return task;
        }
        public override void Save(string savepointName)
        {
            this.InternalTransaction.Save(savepointName);
            this.Status = DatabaseTransactionStatus.Saved;
        }
        public override void Commit()
        {
            this.InternalTransaction.Commit();
            this.Status = DatabaseTransactionStatus.Committed;
            this.DatabaseConnection.ReleaseCurrentTransaction();
        }

        public override void Rollback()
        {
            this.InternalTransaction.Rollback();
            this.Status = DatabaseTransactionStatus.RolledBack;
            this.DatabaseConnection.ReleaseCurrentTransaction();
        }

        public static DatabaseTransaction Create(Connection connection, IsolationLevel IsolationLevel = IsolationLevel.ReadUncommitted)
        {
            return new DatabaseTransaction(connection, IsolationLevel);
        }
        public DatabaseTransaction(Connection connection, IsolationLevel isolationLevel)
        {
            this.DatabaseConnection = connection;
            this.Status = DatabaseTransactionStatus.Created;
            this.InternalTransaction = connection.InternalConnection.BeginTransaction(isolationLevel);
        }
    }
    internal enum DatabaseTransactionStatus
    {
        Created,
        Committed,
        RolledBack,
        Saved,
        Released,
        Disposed
    }
}
