using Task = TaskAPI.Models.Task;
using Microsoft.EntityFrameworkCore;


namespace TaskAPI.Data
{

    /// <summary>
    /// Classe which holds the database context. <see cref="MyDatabaseContext"/> class.
    /// </summary>
    public class MyDatabaseContext : DbContext
    {

        // Note: To open the DB in sql management studio either:
        // Use: (LocalDb)\mssqllocaldb
        //
        // Or obtain the connection using
        // C:\Program Files\Microsoft SQL Server\140\Tools\Binn>sqllocaldb info mssqllocaldb
        // Copy the Instance pipe name: 
        // Ex: np:\\.\pipe\LOCALDB#B40AAB9D\tsql\query 

        /// <summary>
        /// Initializes a new instance of the <see cref="MyDatabaseContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <remarks></remarks>
        public MyDatabaseContext(DbContextOptions<MyDatabaseContext> options) : base(options)
        {
            // This will cause the Database and Tables to be created.
            Database.EnsureCreated();
        }

        /// <summary>
        /// Represents the Tasks table (Entity Set)
        /// </summary>
        /// <value>
        /// The Tasks.
        /// </value>
        /// <remarks></remarks>
        public DbSet<Task> Task{ get; set; }


        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        /// <remarks>
        /// Step 6c
        /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        /// then this method will not be run.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Adds the Task to tne entity model linking it to the Task table
            modelBuilder.Entity<Task>().ToTable("Task");

        }
    }

}
