using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TaskAPI.DataTransferObjects;

namespace TaskAPI.Lib
{

    /// <summary>
    /// Classe holds the logics related to Errors in general in the API, such as messages, error codes, and responses.
    /// </summary>

    public class ErrorLibrary
    {
        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error number
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error number</returns>
        public static int GetErrorNumberFromDescription(string encodedErrorDescription)
        {
            if (int.TryParse(encodedErrorDescription, out int errorNumber))
            {
                return errorNumber;
            }
            return 0;
        }



        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error response
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error message and number</returns>
        public static (string decodedErrorMessage, int decodedErrorNumber) GetErrorMessage(string encodedErrorDescription)
        {

            int errorNumber = GetErrorNumberFromDescription(encodedErrorDescription);

            switch (errorNumber)
            {
                case 1:
                    {
                        return ("The entity already exists.", errorNumber);
                    }
                case 2:
                    {
                        return ("The parameter value is too large.", errorNumber);
                    }
                case 3:
                    {
                        return ("The parameter is required.", errorNumber);
                    }
                case 4:
                    {
                        return ("The maximum number of entities have been created. No further entities can be created at this time.", errorNumber);
                    }
                case 5:
                    {
                        return ("The entity could not be found.", errorNumber);
                    }
                case 6:
                    {
                        return ("The parameter value is too small.", errorNumber);
                    }
                case 7:
                    {
                        return ("The parameter value is not valid.", errorNumber);
                    }
                case 8:
                    {
                        return ("Due date must be in future.", errorNumber);
                    }
                case 9:
                    {
                        return ("Not a valid date, the format is yyyy-MM-dd.", errorNumber);
                    }
                case 10:
                    {
                        return ("The entity could not be found.", errorNumber);
                    }
                default:
                    {
                        return ($"Raw Error: {encodedErrorDescription}", errorNumber);
                    }

            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents Error Response result.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents the Erro Response.
        /// </returns>
        public static ErrorResponse GetErrorResponse(int errorNumber, string param, string value)
        {
            // initiate Error Response object.
            ErrorResponse errorResponse = new ErrorResponse();
            
            // add the values to the object
            (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorLibrary.GetErrorMessage(errorNumber.ToString());
            errorResponse.parameterName = param;
            errorResponse.parameterValue = value;
            
            // returns the error response.
            return(errorResponse);
            
        }

 
    }
}
