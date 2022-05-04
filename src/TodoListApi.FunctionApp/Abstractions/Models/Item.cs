using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoListApi.FunctionApp.Abstractions.Interfaces;

namespace TodoListApi.FunctionApp.Abstractions.Models
{
    public class Item : IItem
    {
        /// <summary>
        /// Gets or sets the item's globally unique identifier. If no value is set then a GUID is used.
        /// </summary>
        /// <remarks>
        /// Initialized by <see cref="Guid.NewGuid"/>.
        /// </remarks>
        [JsonProperty("id")]
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the item's type name. This is used as a discriminator.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// In seconds
        /// </summary>
        [JsonProperty("ttl", NullValueHandling = NullValueHandling.Ignore)]
        public int? TimeToLive { get; set; }

        /// <summary>
        /// Gets the PartitionKey based on <see cref="GetPartitionKeyValue"/>.
        /// Implemented explicitly to keep out of Item API
        /// </summary>
        string IItem.PartitionKey => GetPartitionKeyValue();

        /// <summary>
        /// Default constructor, assigns type name to <see cref="System.Type"/> property.
        /// </summary>
        public Item() => Type = GetType().Name;

        /// <summary>
        /// Gets the partition key value for the given <see cref="Item"/> type.
        /// When overridden, be sure that the <see cref="PartitionKeyPathAttribute.Path"/> value corresponds
        /// to the <see cref="JsonPropertyAttribute.PropertyName"/> value, i.e.; "/partition" and "partition"
        /// respectively. If these two values do not correspond an error will occur.
        /// </summary>
        /// <returns>The <see cref="Item.Id"/> unless overridden by the subclass.</returns>
        protected virtual string GetPartitionKeyValue() => Type;

        [JsonProperty("createdDateUtc")]
        public DateTimeOffset CreatedDateUtc { get; set; } = DateTimeOffset.UtcNow;
    }
}
