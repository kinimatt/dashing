﻿namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;

    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Extensions;

    internal class WhereClauseWriter : IWhereClauseWriter {
        private readonly ISqlDialect dialect;

        private readonly IConfiguration configuration;

        public WhereClauseWriter(ISqlDialect dialect, IConfiguration config) {
            this.dialect = dialect;
            this.configuration = config;
        }

        public SelectWriterResult GenerateSql<T>(IEnumerable<Expression<Func<T, bool>>> whereClauses, FetchNode rootNode) {
            if (whereClauses.IsEmpty()) {
                return new SelectWriterResult(string.Empty, null, rootNode);
            }

            var sql = new StringBuilder(" where ");
            var expressionVisitor = new WhereClauseExpressionVisitor(this.dialect, this.configuration, rootNode);
            foreach (var whereClause in whereClauses) {
                rootNode = expressionVisitor.VisitWhereClause(whereClause);
                sql.Append(expressionVisitor.Sql);
                sql.Append(" and ");
            }

            // remove the last and
            sql.Remove(sql.Length - 5, 5);
            return new SelectWriterResult(sql.ToString(), expressionVisitor.Parameters, rootNode);
        }
    }
}