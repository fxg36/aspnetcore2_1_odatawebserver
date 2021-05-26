using ODataWebserver.Global;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ODataWebserver.Models
{
    public class Job : IModel
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
        public string Name { get; set; }
        public bool? IsResultFetched { get; set; }
        public bool? IsFinished { get; set; }
        public bool? IsProcessing { get; set; }
        [IgnoreDataMember] // do not show it in rest api
        public string StatusComment { get; set; }
        public virtual ICollection<JobResult> JobResults { get; set; }
        public virtual ICollection<ValueOverride> ValueOverrides { get; set; }
        public virtual ICollection<HyperParameterForJob> HyperParameters { get; set; }
    }
}