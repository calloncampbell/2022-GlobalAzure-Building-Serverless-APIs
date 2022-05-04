using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoListApi.FunctionApp.Abstractions.Constants
{
    public class Constants
    {
        public class AppConfig
        {
            public const string AppPrefix = "ToDo";
        }

        public class CosmosDb
        {
            public const int DefaultTimeToLive = -1;
            public const string Connection = "CosmosDB-ConnectionStringReadWrite";
            public const string DatabaseId = "ToDo";

            public class Collection
            {
                public const string ToDoItemCollection = "ToDoItem";
            }
        }
    }
}
