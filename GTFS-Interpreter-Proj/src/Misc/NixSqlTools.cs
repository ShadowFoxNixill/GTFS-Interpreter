using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Nixill.SQLite {
  internal static class GTSSSQLiteConnectionUtils {
    /// <summary>
    /// Returns a single result.
    /// <para/>
    /// For queries that return more than one result or have more than one
    /// column, this returns the value of the first column in the first
    /// row.
    /// <para/>
    /// If the query returns no rows, <c>null</c> is returned.
    /// </summary>
    public static object GetResult(this SqliteConnection conn, string query, params object[] pars) {
      using SqliteCommand cmd = conn.CreateCommand();
      cmd.CommandText = query;
      if (pars != null) {
        for (int i = 0; i < pars.Length; i++) {
          cmd.Parameters.AddWithValue("@p" + i, pars[i]);
          cmd.Prepare();
        }
      }
      SqliteDataReader reader = cmd.ExecuteReader();
      if (reader.Read()) {
        return reader[0];
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Returns a list of results.
    /// <para/>
    /// For queries that return more than one column, this returns the
    /// values of the first column only.
    /// <para/>
    /// If the query returns no rows, an empty list is returned.
    /// </summary>
    public static List<object> GetResultList(this SqliteConnection conn, string query, params object[] pars) {
      using SqliteCommand cmd = conn.CreateCommand();
      cmd.CommandText = query;
      if (pars != null) {
        for (int i = 0; i < pars.Length; i++) {
          cmd.Parameters.AddWithValue("@p" + i, pars[i]);
          cmd.Prepare();
        }
      }
      SqliteDataReader reader = cmd.ExecuteReader();
      List<object> ret = new List<object>();
      while (reader.Read()) {
        ret.Add(reader[0]);
      }
      return ret;
    }

    /// <summary>
    /// Returns a dictionary of results.
    /// <para/>
    /// Queries are expected to return two columns, where the first has
    /// distinct strings.
    /// <para/>
    /// If the query returns no rows, an empty dictionary is returned.
    /// </summary>
    public static Dictionary<string, object> GetResultDict(this SqliteConnection conn, string query, params object[] pars) {
      using SqliteCommand cmd = conn.CreateCommand();
      cmd.CommandText = query;
      if (pars != null) {
        for (int i = 0; i < pars.Length; i++) {
          cmd.Parameters.AddWithValue("@p" + i, pars[i]);
          cmd.Prepare();
        }
      }
      SqliteDataReader reader = cmd.ExecuteReader();
      Dictionary<string, object> ret = new Dictionary<string, object>();
      while (reader.Read()) {
        ret.Add(reader.GetString(0), reader[1]);
      }
      return ret;
    }

    /// <summary>
    /// Returns a multi-column row.
    /// <para/>
    /// Queries are expected to return one row with many columns.
    /// <para/>
    /// If the query returns no rows, <c>null</c> is returned.
    /// </summary>
    public static Dictionary<string, object> GetRowDict(this SqliteConnection conn, string query, params object[] pars) {
      using SqliteCommand cmd = conn.CreateCommand();
      cmd.CommandText = query;
      if (pars != null) {
        for (int i = 0; i < pars.Length; i++) {
          cmd.Parameters.AddWithValue("@p" + i, pars[i]);
          cmd.Prepare();
        }
      }
      SqliteDataReader reader = cmd.ExecuteReader();
      if (reader.Read()) {
        Dictionary<string, object> ret = new Dictionary<string, object>();
        for (int i = 0; i < reader.VisibleFieldCount; i++) {
          ret.Add(reader.GetName(i), reader[i]);
        }
        return ret;
      }
      else {
        return null;
      }
    }
  }
}