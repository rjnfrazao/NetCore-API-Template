using System;
using System.Linq;
using Task = TaskAPI.Models.Task;

namespace TaskAPI.Data
{


    /// <summary>
    /// Classe holds the logic to initialize the database.
    /// </summary>

    public class DbInitializer
    {
        /// <summary>
        /// Initializes the specified context with data
        /// </summary>
        /// <param name="context">The context.</param>
        public static void Initialize(MyDatabaseContext context)
        {
            // Check to see if there is any data in the task table
            if (context.Task.Any())
            {
                // Task table has data, nothing to do here
                return;
            }

            // Create some data
            Task[] tasks = new Task[]
            {
                new Task() { taskName = "Buy groceries", isCompleted = false, dueDate = Convert.ToDateTime("2021-02-03")},
                new Task() { taskName = "Workout", isCompleted = true, dueDate = Convert.ToDateTime("2021-01-01")},
                new Task() { taskName = "Paint fence", isCompleted = false, dueDate = Convert.ToDateTime("2021-03-15")},
                new Task() { taskName = "Mow Lawn", isCompleted = false, dueDate = Convert.ToDateTime("2021-06-11")}
            };

            // Add the data to the in memory model
            foreach (Task task in tasks)
            {
                context.Task.Add(task);
            }

            // Commit the changes to the database
            context.SaveChanges();
        }

    }
}
