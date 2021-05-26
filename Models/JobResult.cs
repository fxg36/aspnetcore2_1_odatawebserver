using ODataWebserver.Global;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ODataWebserver.Models
{
    public class JobResult : IModel
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
        [Required, ForeignKey(nameof(Job))]
        public int? JobId { get; set; }
        public byte[] Blob { get; set; }
        public string DataOriginFormat { get; set; }
        public string DataEncoding { get; set; }

    }
}