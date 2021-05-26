using ODataWebserver.Global;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ODataWebserver.Models
{
    public class ValueOverride : IModel
    {
        public int Id { get; set; }
        [IgnoreDataMember] // do not show it in rest api
        public DateTime? CreatedUtc { get; set; }
        [IgnoreDataMember] // do not show it in rest api
        public DateTime? LastChangeUtc { get; set; }
        [IgnoreDataMember] // do not show it in rest api
        public string CreatedBy { get; set; }
        [IgnoreDataMember] // do not show it in rest api
        public string LastChangeBy { get; set; }
        public virtual Job Job { get; set; }
        [ForeignKey(nameof(Job))]
        public int? JobId { get; set; }
        public string Context { get; set; }
        public string TableName { get; set; }
        public string EntityId { get; set; }
        public string AttributeName { get; set; }
        public string Value { get; set; }
    }
}