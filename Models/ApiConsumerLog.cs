using ODataWebserver.Global;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ODataWebserver.Models
{
    public class ApiConsumerLog : IModel
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
        public string Operation { get; set; }
        public virtual ApiConsumer ApiConsumer { get; set; }

        [Required, ForeignKey(nameof(ApiConsumer))]
        public int? ApiConsumerId { get; set; }
    }
}