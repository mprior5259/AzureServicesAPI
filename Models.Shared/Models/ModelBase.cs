using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Shared.Models
{
    public class ModelBase
    {
        public string? Message { get; set; }
        public bool Success { get; set; }

        public ModelBase()
        {
            Success = true;
        }

        public ModelBase(string errorMessage)
        {
            Success = false;
            Message = errorMessage;
        }
    }
}
