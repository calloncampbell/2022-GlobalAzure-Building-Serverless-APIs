using System;
using System.Linq;

namespace TodoListApi.FunctionApp.Abstractions.Interfaces
{
    public interface IItem
    {
        /// <summary>
        /// Gets or sets the item's globally unique identifier.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the item's type name.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Gets the item's PartitionKey. This string is used to instantiate the <c>Cosmos.PartitionKey</c> struct.
        /// </summary>
        string PartitionKey { get; }
    }
}
