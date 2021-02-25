using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TaskAPI.DataTransferObjects
{
    /// <summary>
    /// The Task creation or update structure. Custom validation is implemented to validate some fields.
    /// </summary>
    /// <remarks>
    /// Custom validations:
    /// 1) Due date in the right format.
    /// 2) Due date in the future.
    /// 3) taskName not null or blank.
    /// </remarks>
    [CustomValidation(typeof(TaskCreatePayload), "TaskPayloadValidation")]

    public class TaskCreatePayload
    {

        /// <summary>
        /// Gets or sets the task's name.
        /// </summary>
        /// <value>The name.</value>
        [Required( ErrorMessage = "3")]
        [StringLength(100, ErrorMessage = "2")]
        public string taskName { get; set; }

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        /// <value>The email address.</value>
        [Required( ErrorMessage = "3")]
        public bool isCompleted{ get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>The age.</value>
        [Required( ErrorMessage = "3")]
        public String dueDate { get; set; }


        /// <summary>
        /// Validates the task name and due date.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="ctx">The context which contains the actual value from the field that the custom validator is validating.</param>
        /// <returns>ValidationResult.</returns>
        public static ValidationResult TaskPayloadValidation(TaskCreatePayload task, ValidationContext ctx)
        {
            // Verify that the task name in not null nor empty 
            string taskName = task.taskName;
            DateTime dueDate;
            var errorResponse = new ErrorResponse();


            // Verify if the date is in the right format
            try
            { 
                dueDate = DataConversion.ToDateTime(task.dueDate); 
            }
            catch // (FormatException ex)
            {
                return new ValidationResult("9", new List<string> { "dueDate" }); 
            }
            
            // Verify if the task name is null or empty.
            if (taskName == null || taskName.Length < 1)
            {
                return new ValidationResult("3", new List<string> { "taskName" });
            }

            // Verify that due date is in the present or future.
            if (dueDate < DateTime.Now)
            {
                return new ValidationResult("8", new List<string> { "dueDate"});
            }

            return ValidationResult.Success;

               
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"taskName=[{taskName}], isCompleted=[{isCompleted}], dueDate=[{dueDate}]";
        }


    }
}
