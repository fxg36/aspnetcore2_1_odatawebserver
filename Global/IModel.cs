using System;

namespace ODataWebserver.Global
{
    public interface IModel
    {
        int Id { get; set; }
        DateTime? CreatedUtc { get; set; }
        DateTime? LastChangeUtc { get; set; }
        string CreatedBy { get; set; }
        string LastChangeBy { get; set; }
    }
}
