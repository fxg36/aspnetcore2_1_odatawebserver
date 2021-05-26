using ODataWebserver.Global;
using System;
using System.Runtime.Serialization;

namespace ODataWebserver.Models
{
    public class HyperParameter : IModel
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

        public string Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}