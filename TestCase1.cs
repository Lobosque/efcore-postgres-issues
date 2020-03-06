using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyPoC
{
    /// <summary>
    /// This test case demonstrate how detaching an entity when a Transaction is open does
    /// not work for Postgres
    /// </summary>
    public class TestCase1 : ITestCase
    {
        private readonly BaseContext _dataContext;
        private readonly string _dbType;

        public TestCase1(string dbType) {
            _dbType = dbType;
            _dataContext = ContextFactory.CreateContext(dbType);
        }

        public void Run()
        {
            using var transaction = _dataContext.Database.BeginTransaction(IsolationLevel.ReadUncommitted);

            var isFirstRun = true;
            var saved = false;

            var blog = _dataContext.Blogs.Single(p => p.BlogId == 1);

            while(!saved)
            {
                blog.Url = "blog1_alter";

                var post = new Post() {
                    Title = "My Post Title",
                    BlogId = 1
                };
                _dataContext.Posts.Add(post);

                // If it is the first time running the loop, simulate an external update
                // to force throwing a DbUpdateConcurrencyException
                if(isFirstRun) {
                    ExternalUpdate();
                    isFirstRun = false;
                }

                try
                {
                    // Attempt to save changes to the database
                    _dataContext.SaveChanges();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Since the save did not work, detach the added entries
                    var addedEntries = _dataContext.ChangeTracker.Entries()
                        .Where(e => e.State == EntityState.Added)
                        .ToList();

                    foreach (var entry in addedEntries)
                        entry.State = EntityState.Detached;

                    _dataContext.Entry(blog).Reload();
                }
            }

            transaction.Commit();
        }

        public void Assert()
        {
            //Assert that only one Post was added
            using var context = ContextFactory.CreateContext(_dbType);
            Console.WriteLine("Assert that only 1 Post was created...");
            Debug.Assert(1 == context.Posts.Count());
        }

        public void ExternalUpdate() {
            // Change the entry in the database to simulate a concurrency conflict
            using var context = ContextFactory.CreateContext(_dbType);
            context.Database.ExecuteSqlRaw("UPDATE blogs SET url = 'blog1_updatedElsewhere' WHERE blog_id = 1");
        }
    }
}