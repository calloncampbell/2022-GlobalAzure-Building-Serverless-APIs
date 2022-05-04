using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoListApi.FunctionApp.Abstractions.Models;

namespace TodoListApi.FunctionApp.Abstractions.Interfaces
{
    public interface IToDoListService
    {
        Task<List<ToDoItem>> GetToDoItemsAsync();
        Task<ToDoItem> GetToDoItemAsync(string id);
        Task<ToDoItem> UpsertToDoItemAsync(ToDoItem item);
        Task<ToDoItem> DeleteToDoItemAsync(string id);
    }
}
