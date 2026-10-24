namespace ContosoUniversity.IntegrationTests
{
    using System;
    using System.Linq;
    using DAL;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class TestContextFixture
    {
        private readonly TestServiceScope scope;

        public TestContextFixture(TestServiceScope scope)
        {
            this.scope = scope;
        }

        public void SetUp()
        {
            scope.CreateNestedScope();
        }

        public void SaveAll(params object[] entities)
        {
            Do(dbContext =>
            {
                foreach (var entity in entities)
                {
                    var entry = dbContext.ChangeTracker.Entries().FirstOrDefault(entityEntry => entityEntry.Entity == entity);
                    if (entry == null)
                    {
                        // EF6's dbContext.Set(entity.GetType()).Add(entity) has no non-generic
                        // DbSet equivalent in EF Core; the non-generic Add is the direct match.
                        dbContext.Add(entity);
                    }
                }
            });
        }

        public void Reload<TEntity, TIdentity>(
            ref TEntity entity,
            TIdentity id)
            where TEntity : class
        {
            TEntity e = entity;

            Do(ctx => e = ctx.Set<TEntity>().Find(id));

            entity = e;
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            Do(dbContext => dbContext.Set<TEntity>().Remove(entity));
        }

        public void Do(Action action)
        {
            var dbContext = scope.GetInstance<SchoolContext>();
            try
            {
                dbContext.BeginTransaction();
                action();
                dbContext.CloseTransaction();
            }
            catch (Exception e)
            {
                dbContext.CloseTransaction(e);
                throw;
            }
        }

        public void Do(Action<DbContext> action)
        {
            var dbContext = scope.GetInstance<SchoolContext>();
            try
            {
                dbContext.BeginTransaction();
                action(dbContext);
                dbContext.CloseTransaction();
            }
            catch (Exception e)
            {
                dbContext.CloseTransaction(e);
                throw;
            }
        }

        public void DoClean(Action<DbContext> action)
        {
            var connString = LocalDbFactory.Instance.CreateConnectionStringBuilder();
            connString.InitialCatalog = "ContosoUniversity";

            // SchoolContext is now EF Core and only accepts DbContextOptions, so the old
            // new SchoolContext(connectionString) is expressed as an options build here. The
            // semantics are unchanged: a fresh context outside the test's service scope.
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseSqlServer(connString.ToString())
                .Options;

            var dbContext = new SchoolContext(options);

            try
            {
                dbContext.BeginTransaction();
                action(dbContext);
                dbContext.CloseTransaction();
            }
            catch (Exception e)
            {
                dbContext.CloseTransaction(e);
                throw;
            }
        }

        public void Send(IRequest message)
        {
            // MediatR 12's IRequest is the non-generic, Unit-returning request; ISender exposes
            // Send<TRequest>(TRequest, CancellationToken) : Task for it. The public signature here
            // stays synchronous, so the async send is awaited inline.
            var context = scope.GetInstance<SchoolContext>();

            context.BeginTransaction();

            var mediator = scope.GetInstance<IMediator>();

            Exception exc = null;
            try
            {
                mediator.Send(message).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                exc = e;
            }

            context.CloseTransaction(exc);

            if (exc != null)
            {
                throw new Exception("Failed to send message.", exc);
            }
        }

        public TResult Send<TResult>(IRequest<TResult> message, bool asNonPullStep = false)
        {
            var result = default(TResult);

            var context = scope.GetInstance<SchoolContext>();

            context.BeginTransaction();

            var mediator = scope.GetInstance<IMediator>();

            Exception exc = null;
            try
            {
                result = mediator.Send(message).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                exc = e;
            }

            context.CloseTransaction(exc);

            if (exc != null)
            {
                throw new Exception("Failed to send message.", exc);
            }

            return result;
        }
    }
}
