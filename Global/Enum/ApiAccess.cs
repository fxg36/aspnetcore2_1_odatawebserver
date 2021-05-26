using System;

namespace ODataWebserver.Global
{
    [Flags]
    public enum ApiAccess
    {
        None = 1,
        Read = 2,
        Insert = 4,
        Update = 8,
        Delete = 16,
        InsertRead = Read|Insert,
        Full = Read|Insert|Update|Delete
    }
}
