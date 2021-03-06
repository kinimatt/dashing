﻿namespace Dashing.Engine.DML {
    using Dapper;

    public class SelectWriterResult : SqlWriterResult {
        public FetchNode FetchTree { get; internal set; }

        public int NumberCollectionsFetched { get; set; }

        public SelectWriterResult(string sql, DynamicParameters parameters, FetchNode fetchTree)
            : base(sql, parameters) {
            this.FetchTree = fetchTree;
        }
    }
}