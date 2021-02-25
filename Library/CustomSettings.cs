using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskAPI.Lib
{


        /// <summary>
        /// POCO Class used to store the custom settings required by the Task API.
        /// </summary>
        public class CustomSettings
            {

                /// <summary>
                /// Gets or sets the maximum number of tasks.
                /// </summary>
                /// <value>
                /// The maximum number of tasks.
                /// </value>
                /// <remarks>Setting a default max number of tasks if non provided in config</remarks>
                public int MaxTasks { get; set; } = 25;


            }
}
