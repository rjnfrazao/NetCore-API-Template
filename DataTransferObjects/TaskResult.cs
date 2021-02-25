using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Task = TaskAPI.Models.Task;

namespace TaskAPI.DataTransferObjects
{
    /// <summary>
    /// Defines the public facing for the task result, when new task is created or updated.
    /// </summary>
    public class TaskResult
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="TaskResult"/> class using a task as input.
        /// </summary>
        /// <param name="task">The customer.</param>
        public TaskResult(Task task)
        {
            id = task.id ?? -1;
            isCompleted = task.isCompleted;
            taskName = task.taskName;
            dueDate = DataConversion.ToDateTimeString(task.dueDate);
        }

        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int id { get; set; }

        /// <summary>
        /// Gets or sets the task's name.
        /// </summary>
        /// <value>The name.</value>
        public string taskName { get; set; }

        /// <summary>
        /// Get or set if task is completed.
        /// </summary>
        /// <value>Boolean</value>
        public bool isCompleted { get; set; }

        /// <summary>
        /// The date the task is due to be completed.
        /// </summary>
        /// <value>The date.</value>
        public string dueDate { get; set; }

    }
}
