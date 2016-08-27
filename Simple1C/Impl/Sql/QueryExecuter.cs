﻿using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Simple1C.Impl.Sql.SqlAccess;

namespace Simple1C.Impl.Sql
{
    internal class QueryExecuter
    {
        private readonly PostgreeSqlDatabase[] sources;
        private readonly MsSqlDatabase target;
        private readonly bool dumpSql;
        private volatile bool errorOccured;
        private readonly string queryText;
        private readonly string tableName;

        public QueryExecuter(PostgreeSqlDatabase[] sources, MsSqlDatabase target, string queryFileName, bool dumpSql)
        {
            this.sources = sources;
            this.target = target;
            this.dumpSql = dumpSql;
            queryText = File.ReadAllText(queryFileName);
            tableName = Path.GetFileNameWithoutExtension(queryFileName);
        }

        public bool Execute()
        {
            var s = Stopwatch.StartNew();
            var sourceThreads = new Thread[sources.Length];
            using (var writer = new BatchWriter(target, tableName, 1000))
            {
                var w = writer;
                for (var i = 0; i < sourceThreads.Length; i++)
                {
                    var source = sources[i];
                    sourceThreads[i] = new Thread(delegate(object _)
                    {
                        try
                        {
                            var mappingSchema = new PostgreeSqlSchemaStore(source);
                            var translator = new QueryToSqlTranslator(mappingSchema);
                            var sql = translator.Translate(queryText);
                            if (dumpSql)
                                Console.Out.WriteLine("\r\n[{0}]\r\n{1}\r\n====>\r\n{2}",
                                    source.ConnectionString, queryText, sql);
                            source.ExecuteReader(sql, new object[0], delegate(DbDataReader reader)
                            {
                                if (errorOccured)
                                    throw new OperationCanceledException();
                                w.InsertRow(reader);
                            });
                        }
                        catch (OperationCanceledException)
                        {
                        }
                        catch (Exception e)
                        {
                            errorOccured = true;
                            Console.Out.WriteLine("error for [{0}]\r\n{1}", source.ConnectionString, e);
                        }
                    });
                    sourceThreads[i].Start();
                }
                foreach (var t in sourceThreads)
                    t.Join();
            }
            s.Stop();
            Console.Out.WriteLine("\r\ndone, [{0}] millis", s.ElapsedMilliseconds);
            return errorOccured;
        }
    }
}