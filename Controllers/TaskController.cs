using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TaskAPI.Data;
using TaskAPI.DataTransferObjects;
using TaskAPI.Lib;
using Task = TaskAPI.Models.Task;

namespace TaskAPI.Controllers
{


    /// <summary>
    /// Implement the controller for Task API
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Produces("application/json")]
    [ApiController]
    [Route("tasks")]
    public class TaskController : ControllerBase
    {


        /// <summary>
        /// The get task by identifier route
        /// </summary>
        private const string GetTaskByIdRoute = "GetTaskByIdRoute";

        /// <summary>
        /// The database context
        /// </summary>
        private readonly MyDatabaseContext _context;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Customs settings - max task limists
        /// </summary>
        private readonly CustomSettings _taskCustomSettings;


        /// <summary>
        /// Initializes a new instance of the <see cref="TaskController"/> class.
        /// </summary>
        public TaskController(ILogger<TaskController> logger,
                                   MyDatabaseContext context,
                                   IConfiguration configuration,
                                   IOptions<CustomSettings> customSettings) 

        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _taskCustomSettings = customSettings.Value; // or .MaxTasks

        }


        /// <summary>
        /// Determines whether this more tasks can be added.
        /// </summary>
        /// <returns>
        ///   true if more tasks can be added false if not
        /// </returns>
        private bool CanAddMoreTasks()
        {
            long totalTasks = (from c in _context.Task select c).Count();

            // DEMO SETTINGS:
            if (_taskCustomSettings.MaxTasks > totalTasks)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns all tasks.
        /// </summary>
        /// <param name="orderByDate">Order By Due Date (Asc or Desc).</param>
        /// <param name="taskStatus">Task Status to be filtered (Completed or NotCompleted or All).</param>
        /// <returns>All tasks based on the filter and order by defined in the query parameters.</returns>
        /// <remarks>
        /// Demo Notes:
        /// If orderByDate is not provided, the defauld will be Asc.
        /// If taskStatus is not provided, the default is All.
        /// </remarks>
        // GET: [Route at  controller]?orderByDate={}&taskStatus={}
        [ProducesResponseType(typeof(TaskResult[]), (int)HttpStatusCode.OK)] //200
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]  //400
        [HttpGet]
        public ActionResult Index([FromQuery] string orderByDate = "Asc", [FromQuery] string taskStatus = "All")
        {

            try
            {
                Task[] tasks;

                // return an error response, if param OrderByDate in not valiod
                if (orderByDate!="Asc" && orderByDate != "Desc")
                {
                    // Param wasn't valid.
                    return BadRequest(ErrorLibrary.GetErrorResponse(7, "orderByDate", orderByDate.ToString()));
                }

                // return and error, if param taskStatus in not valiod
                if (taskStatus != "Completed" && taskStatus != "NotCompleted" && taskStatus != "All")
                {
                    // Param wasn't valid.
                     return BadRequest(ErrorLibrary.GetErrorResponse(7, "taskStatus", taskStatus.ToString()));
                }

                // Check if should return all records
                if (taskStatus == "All")
                {
                    // check the order by
                    if (orderByDate == "Desc")
                    {
                        // Query all records in descending due date order.
                        tasks = (from t in _context.Task orderby t.dueDate descending select t).ToArray();
                    } else
                    {
                        // Query all records in ascending due date order.
                        tasks = (from t in _context.Task orderby t.dueDate ascending select t).ToArray();
                    }
                }
                else 
                {
                    // Filter is required on isCompleted
                    bool isCompleted;
                    if (taskStatus=="Completed")
                    {
                        // Tasks completed only
                        isCompleted = true;
                    } else
                    {
                        // Tasks not completed yet.
                        isCompleted = false;
                    }

                    // check the order by dueDate.
                    if (orderByDate=="Desc")
                    {
                        // Query records filtered by isCompleted and descending due date order.
                        tasks = (from t in _context.Task where t.isCompleted == isCompleted orderby t.dueDate descending select t).ToArray();
                    } else
                    {
                        // Query records filtered by isCompleted and ascending due date order.
                        tasks = (from t in _context.Task where t.isCompleted == isCompleted orderby t.dueDate ascending select t).ToArray();
                    }
                    
                }
               

                // taskResults hold data to be returned
                var taskResults = new List<TaskResult>();

                // Loop all tasks returned by the query.
                foreach (Task task in tasks)
                {
                    // add to the result response.
                    taskResults.Add(new TaskResult(task));
                }

                // returns the records, encapsulated in "tasks" 
                var dictTasks = new Dictionary<string, List<TaskResult>>();
                dictTasks.Add("tasks", taskResults);
                return new ObjectResult(dictTasks);
            }
            catch (Exception ex)
            {
                // Any other exception that may happen
                _logger.LogError(LoggingEvents.InternalError, ex, $"TaskController All tasks caused an internal error.");
                return Conflict(ErrorLibrary.GetErrorResponse(0, null, null));
            }
        }


        /// <summary>
        /// Gets the specified task based on the id parameter.
        /// </summary>
        /// <param name="id">The task's id.</param>
        /// <returns>The task</returns>
        /// <remarks>
        /// Demo Notes:
        /// In case record is not found, returns 404 error.</remarks>

        // GET: [Route at  controller]/5
        [ProducesResponseType(typeof(TaskResult), (int)HttpStatusCode.OK)] //200
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]   //404

        [HttpGet("{id}", Name = GetTaskByIdRoute)]

        public ActionResult Get(int id)
        {

            try
            {

                Task taskEntity = new Models.Task();

                // Check if record exists
                taskEntity = (from t in _context.Task where t.id == id select t).SingleOrDefault();

                if (taskEntity == null)
                {
                    // Task id not found.
                    return NotFound(ErrorLibrary.GetErrorResponse(10, "id", id.ToString()));
                }

 
                // Record was found. Initialize TaskResult using the task returned from the DB.
                TaskResult taskResult = new TaskResult(taskEntity);

                // returns the record.
                return Ok(taskResult);
            }
            catch (Exception ex)
            {
                // Any other exception that may happen
                _logger.LogError(LoggingEvents.InternalError, ex, $"TaskController Delete taskEntity([{id}]) caused an internal error.");
                return Conflict(ErrorLibrary.GetErrorResponse(0, null, null));
            }
        }

        /// <summary>
        /// Creates the task
        /// </summary>
        /// <param name="taskCreatePayload">The task.</param>
        /// <returns>Returns the response as specified.</returns>
        // Post: tasks/
        [ProducesResponseType(typeof(TaskResult), (int)HttpStatusCode.Created)] //201
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]  //400
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.Forbidden)]   //403
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.Conflict)]   //409
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskCreatePayload taskCreatePayload)
        {

            Task taskEntity = new Task();

            try
            {
                // check if the model was validated.    
                if (ModelState.IsValid)
                {
                    // First verify if max number of tasks was reached. 
                    if (!CanAddMoreTasks())
                    {
                        // No more tasks can be added.
                        return StatusCode(403, ErrorLibrary.GetErrorResponse(4, null, null));

                    }

                    taskEntity.taskName = taskCreatePayload.taskName;
                    taskEntity.isCompleted = taskCreatePayload.isCompleted;
                    taskEntity.dueDate = DataConversion.ToDateTime(taskCreatePayload.dueDate); // ** REMOVE DateTime.ParseExact(taskCreatePayload.dueDate, "s", new CultureInfo("en-US"));

                    // Tell entity framework to add the task entity.
                    _context.Task.Add(taskEntity);

                    // save the changes to the database.
                    _context.SaveChanges();
                }
                else
                {
                    // ** IMPROVEMENT : This part of code must be revised so it can be reused between POST and PUT
                    ErrorResponse errorResponse = new ErrorResponse();

                    // DEMO: Enable multi-stream read
                    // The EnableMultipleStreamReadMiddleware is needed for reading from the
                    // Request Body a second time, the first time the Request.Body is read
                    // is in the middleware for deserializing the Customer Input

                    // This allows us access to the raw input
                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        // This is an approach for determining which properties have errors and knowing the
                        // property name as its the key value
                        foreach (string key in ModelState.Keys)
                        {
                            if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                            {
                                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in ModelState[key].Errors)
                                {
                                    string cleansedKey = key.CleanseModelStateKey();
                                    string camelCaseKey = cleansedKey.ToCamelCase();

                                    //System.Diagnostics.Trace.WriteLine($"MODEL ERROR: key:{cleansedKey} attemtedValue:{jsonDocument.RootElement.GetProperty(camelCaseKey)}, errorMessage:{error.ErrorMessage}");
                                    try
                                    {
                                        errorResponse = ErrorLibrary.GetErrorResponse(int.Parse(error.ErrorMessage),
                                            camelCaseKey,
                                            jsonDocument.RootElement.GetProperty(camelCaseKey).ToString());
                                    }
                                    // Exception raised when atribute is missing in the JSON 
                                    catch (KeyNotFoundException knfEx)
                                    {
                                        _logger.LogInformation(LoggingEvents.GetItem, knfEx, "TaskController Post Task structure not following the specification.");
                                        errorResponse = ErrorLibrary.GetErrorResponse(int.Parse(error.ErrorMessage),
                                            camelCaseKey,
                                            "");
                                        return BadRequest(errorResponse);
                                    };
                                }
                            }
                        }
                    }
                    return BadRequest(errorResponse);
                }
            }
            catch (DbUpdateException ex)
            {
                // Improvement needed : must be checked if the error was really unique index violation.  
                // Error entry duplicated
                _logger.LogInformation(LoggingEvents.InsertItem, ex, $"TaskController Post taskEntity([{taskEntity}]) taskCreatePayload([{taskCreatePayload}], task name duplicated.");

                return Conflict(ErrorLibrary.GetErrorResponse(1, "taskName", taskCreatePayload.taskName));

            }
            catch (Exception ex)
            {
                // Any other exception that may happen
                _logger.LogError(LoggingEvents.InternalError, ex, $"TaskController Post taskEntity([{taskEntity}]) taskCreatePayload([{taskCreatePayload}] caused an internal error.");

                return BadRequest(ErrorLibrary.GetErrorResponse(0, null, null));
            }

            return CreatedAtRoute(GetTaskByIdRoute, new { id = taskEntity.id }, new TaskResult(taskEntity));

        }


        /// <summary>
        /// Updates the task
        /// </summary>
        /// <param name="id">id of the task to be updated.</param>
        /// <param name="taskCreatePayload">The task data to be updated.</param>
        /// <returns>An IAction result indicating HTTP 204 updated if success otherwise BadRequest if the input is not valid.</returns>
        // PUT: tasks/5
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)] //204
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]  //400
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]   //404
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.Conflict)]   //409

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] TaskCreatePayload taskCreatePayload)
        {
            Task taskEntity;

            try
            {
                // check if the model was validated.    
                if (ModelState.IsValid)
                {
                    // Check if record already exist
                    taskEntity = (from t in _context.Task where t.id == id select t).SingleOrDefault();

                    if (taskEntity == null)
                    {
                        // Task id not found.
                        return NotFound(ErrorLibrary.GetErrorResponse(10, "id", id.ToString()));

                    }


                    // Update the entity based on inbound data..
                    taskEntity.taskName = taskCreatePayload.taskName;
                    taskEntity.isCompleted = taskCreatePayload.isCompleted;
                    taskEntity.dueDate = DataConversion.ToDateTime(taskCreatePayload.dueDate);

                    _context.SaveChanges();
                }
                else
                {
                    // ** IMPROVEMENT : This part of code must be revised so it can be reused between POST and PUT

                    ErrorResponse errorResponse = new ErrorResponse();


                    // DEMO: Enable multi-stream read
                    // The EnableMultipleStreamReadMiddleware is needed for reading from the
                    // Request Body a second time, the first time the Request.Body is read
                    // is in the middleware for deserializing the Customer Input

                    // This allows us access to the raw input
                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        // This is an approach for determining which properties have errors and knowing the
                        // property name as its the key value
                        foreach (string key in ModelState.Keys)
                        {
                            if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                            {
                                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in ModelState[key].Errors)
                                {
                                    string cleansedKey = key.CleanseModelStateKey();
                                    string camelCaseKey = cleansedKey.ToCamelCase();

                                    try
                                    {
                                        errorResponse = ErrorLibrary.GetErrorResponse(int.Parse(error.ErrorMessage),
                                            camelCaseKey,
                                            jsonDocument.RootElement.GetProperty(camelCaseKey).ToString());
                                    }
                                    // Exception raised when atribute is missing in the JSON 
                                    catch (KeyNotFoundException knfEx)
                                    {
                                        _logger.LogInformation(LoggingEvents.GetItem, knfEx, "TaskController Put Task structure not following the specification.");
                                        errorResponse = ErrorLibrary.GetErrorResponse(int.Parse(error.ErrorMessage),
                                            camelCaseKey,
                                            "");
                                        return BadRequest(errorResponse);
                                    };
                                }
                            }
                        }
                    }
                    return BadRequest(errorResponse);
                }
            }
            catch (DbUpdateException ex)
            {
                // Error Update caused task name duplication.

                _logger.LogError(LoggingEvents.InternalError, ex, $"TaskController Put taskEntity([{taskCreatePayload.taskName}]) duplicated, taskCreatePayload([{taskCreatePayload}] caused an internal error.");

                return Conflict(ErrorLibrary.GetErrorResponse(1, "taskName", taskCreatePayload.taskName));

            }
            catch (Exception ex)
            {
                // Any other exception that may happen
                _logger.LogError(LoggingEvents.InternalError, ex, $"TaskController Put taskEntity([{taskCreatePayload.taskName}]) taskCreatePayload([{taskCreatePayload}] caused an internal error.");

                return BadRequest(ErrorLibrary.GetErrorResponse(0, null, null));
            }

            return NoContent();

        }



        /// <summary>
        /// Deletes the task
        /// </summary>
        /// <param name="id">id of the task to be deleted.</param>
        /// <returns>An IAction result indicating HTTP 204 deleted if success otherwise Not Found 404 if entity is not found.</returns>
        // DELETE: TaskController/Delete/5
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)] //204
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]   //404
        [HttpDelete("{id}")]

        public IActionResult Delete(int id)
        {
            Task taskEntity;

            try
            {

                // Check if record already exist
                taskEntity = (from t in _context.Task where t.id == id select t).SingleOrDefault();

                if (taskEntity == null)
                {
                    // Task id not found.
                    return NotFound(ErrorLibrary.GetErrorResponse(10, "id", id.ToString()));
                }


                // Delete the entity specified by the caller.
                _context.Remove(taskEntity);
                _context.SaveChanges();


                return NoContent();
            }
            catch (Exception ex)
            {
                // Any other exception that may happen
                _logger.LogError(LoggingEvents.InternalError, ex, $"TaskController Delete taskEntity([{id}]) caused an internal error.");
                return Conflict(ErrorLibrary.GetErrorResponse(0, null, null));
            }
        }
    }
}
