using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS_Notification.Models
{
    class QueryResult
    {

        [Serializable]
        public class Result
        {
            public bool HasError { get; set; }
            public int MessageType { get; set; }
            public string ErrorMessage { get; set; }
            public Object ReturnValue { get; set; }
        }
    }
}
