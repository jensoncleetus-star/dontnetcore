using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;

// Placed in the Microsoft.EntityFrameworkCore namespace so every file that already uses `db.Database.SqlQueryRaw<T>`
// (which imports this namespace) can call `SqlQueryDedup<T>` with no extra using.
namespace Microsoft.EntityFrameworkCore
{
    public static class SqlQueryDedupExtensions
    {
        // EF Core 10's SqlQueryRaw<T> throws "An item with the same key has already been added" when a stored
        // procedure returns DUPLICATE column names (e.g. SP_AVCOMethod returns ItemDescription twice). This is a
        // drop-in replacement that reads the result manually: maps each column to a T property by name with
        // FIRST-occurrence-wins (skipping later duplicates), binds DBNull safely, and converts value types.
        // Returns a materialized List<T> (the call sites chain .AsEnumerable()/.OrderBy()/.Where()/.ToList(), all valid on a List).
        public static List<T> SqlQueryDedup<T>(this DatabaseFacade database, string sql, params object[] parameters) where T : new()
        {
            var result = new List<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var conn = database.GetDbConnection();
            var wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    // Honor the timeout the heavy report methods set via db.SetCommandTimeOut(60*60) (the legacy intent) —
                    // this raw command would otherwise ignore the context timeout. Default to 1h (Dedup is only used for the
                    // heavy Stock* SPs, e.g. all-items SP_AVCOMethod, which the legacy ran with a 60*60 command timeout).
                    cmd.CommandTimeout = database.GetCommandTimeout() ?? (60 * 60);
                    foreach (var p in parameters)
                        if (p is DbParameter dp) cmd.Parameters.Add(dp);

                    using (var rdr = cmd.ExecuteReader())
                    {
                        var colMap = new List<KeyValuePair<int, PropertyInfo>>();
                        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            var name = rdr.GetName(i);
                            if (string.IsNullOrEmpty(name) || !seen.Add(name)) continue; // skip blank/duplicate columns
                            if (props.TryGetValue(name, out var prop)) colMap.Add(new KeyValuePair<int, PropertyInfo>(i, prop));
                        }
                        while (rdr.Read())
                        {
                            var obj = new T();
                            foreach (var kv in colMap)
                            {
                                var val = rdr.GetValue(kv.Key);
                                if (val == null || val == DBNull.Value) continue;
                                var t = Nullable.GetUnderlyingType(kv.Value.PropertyType) ?? kv.Value.PropertyType;
                                try
                                {
                                    object conv = t.IsEnum
                                        ? Enum.ToObject(t, Convert.ChangeType(val, Enum.GetUnderlyingType(t)))
                                        : (t.IsInstanceOfType(val) ? val : Convert.ChangeType(val, t));
                                    kv.Value.SetValue(obj, conv);
                                }
                                catch { /* leave default on conversion failure */ }
                            }
                            result.Add(obj);
                        }
                    }
                }
            }
            finally { if (wasClosed) conn.Close(); }
            return result;
        }
    }
}
