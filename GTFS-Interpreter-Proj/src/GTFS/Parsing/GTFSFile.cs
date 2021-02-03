using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Nixill.GTFS.Parsing;
using Nixill.SQLite;

namespace Nixill.GTFS.Parsing {
  /// <summary>
  /// Represents a container for the contents of a single GTFS file.
  /// </summary>
  /// <remarks>
  /// <c>GTFSFile</c> doesn't contain a public constructor; to get a
  /// reference to a <c>GTFSFile</c> object, use
  /// <link cref="GTFSLoader.Load(string)"><c>GTFSLoader.Load()</c></link>.
  /// </remarks>
  public class GTFSFile : IDisposable {
    internal SqliteConnection Conn;
    internal HashSet<string> Files;

    internal GTFSFile(SqliteConnection conn, HashSet<string> files) {
      Conn = conn;
      Files = files;
    }

    /// <summary>
    /// Disposes this <c>GTFSFile</c>, closing its internal database
    /// connection.
    /// </summary>
    public void Dispose() {
      Conn.Dispose();
    }

    /// <summary>
    /// Queries this <c>GTFSFile</c>'s internal database, and returns a
    /// single result.
    /// <para/>
    /// For queries that return more than one result or have more than one
    /// column, this returns the value of the first column in the first
    /// row.
    /// <para/>
    /// If the query returns no rows, <c>null</c> is returned.
    /// </summary>
    public object GetResult(string query, params object[] pars) {
      return Conn.GetResult(query, pars);
    }

    /// <summary>
    /// Queries this <c>GTFSFile</c>'s internal database, and returns a
    /// list of results.
    /// <para/>
    /// If the query returns no rows, an empty list is returned. If the
    /// query returns multiple columns, only the first is used.
    /// </summary>
    public List<object> GetResultList(string query, params object[] pars) {
      return Conn.GetResultList(query, pars);
    }

    /// <summary>
    /// Queries this <c>GTFSFile</c>'s internal database, and returns a
    /// dictionary of results.
    /// <para/>
    /// Queries are expected to return two columns, where the first has
    /// distinct strings. The first column is used as keys, and the second
    /// is used as values.
    /// <para/>
    /// If the query returns no rows, an empty dictionary is returned. If
    /// the query returns more than two columns, the third and beyond are
    /// ignored.
    /// </summary>
    public Dictionary<string, object> GetResultDict(string query, params object[] pars) {
      return Conn.GetResultDict(query, pars);
    }

    /// <summary>
    /// Queries this <c>GTFSFile</c>'s internal database, and returns a
    /// single row, with column name keys.
    /// <para/>
    /// Queries are expected to return one row with multiple columns.
    /// <para/>
    /// If the query returns no rows, <c>null</c> is returned. If the
    /// query returns multiple rows, only the first is used.
    /// </summary>
    public Dictionary<string, object> GetRowDict(string query, params object[] pars) {
      return Conn.GetRowDict(query, pars);
    }

    /// <summary>
    /// Queries this <c>GTFSFile</c>'s internal database, and returns the
    /// resulting <c>ResultSet</c> directly.
    /// </summary>
    public SqliteDataReader Query(string query, params object[] pars) {
      SqliteCommand cmd = Conn.CreateCommand();
      cmd.CommandText = query;
      if (pars != null) {
        for (int i = 0; i < pars.Length; i++) {
          cmd.Parameters.AddWithValue("@p" + i, pars[i]);
          cmd.Prepare();
        }
      }
      return cmd.ExecuteReader();
    }
  }
}