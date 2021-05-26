using System;
using System.Linq;
using System.Collections.Generic;

namespace ODataWebserver.Global
{
    public static class CollectionHelper
    {
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> collection) => !collection.IsNullOrEmpty();

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection) => collection == null || !collection.Any();
    }
}