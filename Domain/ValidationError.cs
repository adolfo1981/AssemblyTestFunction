using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class ValidationError
    {
        [JsonProperty("propertyName")]
        public string PropertyName { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }

        public ValidationError() {}

        //public ValidationError(ValidationFailure validationFailure)
        //{
        //    PropertyName = validationFailure.PropertyName;
        //    Error = validationFailure.ErrorMessage;
        //}
    }
}
