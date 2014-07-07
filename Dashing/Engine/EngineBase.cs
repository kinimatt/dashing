namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    public class EngineBase : IEngine {
        public IConfiguration Configuration { get; set; }

        protected ISelectWriter SelectWriter { get; set; }

        protected IUpdateWriter UpdateWriter { get; set; }

        protected IInsertWriter InsertWriter { get; set; }

        protected IDeleteWriter DeleteWriter { get; set; }

        /// <summary>
        ///     Gets or sets the maps.
        /// </summary>
        protected IDictionary<Type, IMap> Maps { get; set; }

        protected ISqlDialect Dialect { get; set; }

        protected DbProviderFactory DbProviderFactory { get; set; }

        public EngineBase(ISqlDialect dialect, DbProviderFactory dbProviderFactory) {
            this.Dialect = dialect;
            this.DbProviderFactory = dbProviderFactory;
            this.SelectWriter = new SelectWriter(this.Dialect, this.Configuration);
            this.DeleteWriter = new DeleteWriter(this.Dialect, this.Configuration);
            this.UpdateWriter = new UpdateWriter(this.Dialect, this.Configuration);
            this.InsertWriter = new InsertWriter(this.Dialect, this.Configuration);
        }

        public IDbConnection Open(string connectionString) {
            var connection = this.DbProviderFactory.CreateConnection();
            connection.ConnectionString = connectionString;
            connection.Open();
            return connection;
        }

        public void UseMaps(IDictionary<Type, IMap> maps) {
            this.Maps = maps;
        }

        public virtual IEnumerable<T> Query<T>(IDbConnection connection, SelectQuery<T> query) {
            if (this.SelectWriter == null) {
                throw new Exception("The SelectWriter has not been initialised");
            }

            var sqlQuery = this.SelectWriter.GenerateSql(query);
            return this.Configuration.CodeManager.Query(sqlQuery, query, connection);
        }

        public virtual int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query) {
            if (this.InsertWriter == null) {
                throw new Exception("The InsertWriter has not been initialised");
            }

            foreach (var entity in query.Entities) {
                var sqlQuery = this.InsertWriter.GenerateSql(entity);
                this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);

                var map = this.Configuration.GetMap<T>();
                if (map.PrimaryKey.IsAutoGenerated) {
                    var idQuery = this.InsertWriter.GenerateGetIdSql<T>();
                    var id = this.Configuration.CodeManager.Query<int>(connection, idQuery).First();
                    this.Configuration.GetMap<T>().SetPrimaryKeyValue(entity, id);
                }
            }

            return query.Entities.Count;
        }

        public virtual int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query) {
            if (this.UpdateWriter == null) {
                throw new Exception("The UpdateWriter has not been initialised");
            }

            var sqlQuery = this.UpdateWriter.GenerateSql(query);
            if (sqlQuery.Sql.Length > 0) {
                return this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);
            }

            return 0;
        }

        public virtual int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query) {
            if (this.DeleteWriter == null) {
                throw new Exception("The DeleteWriter has not been initialised");
            }

            var sqlQuery = this.DeleteWriter.GenerateSql(query);
            return this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);
        }

        public T Get<T>(IDbConnection connection, int id, bool? asTracked) {
            var sqlQuery = this.SelectWriter.GenerateGetSql<T>(id);
            return this.Configuration.CodeManager.Query<T>(sqlQuery, connection, asTracked.HasValue ? asTracked.Value : this.Configuration.GetIsTrackedByDefault).SingleOrDefault();
        }

        public T Get<T>(IDbConnection connection, Guid id, bool? asTracked) {
            var sqlQuery = this.SelectWriter.GenerateGetSql<T>(id);
            return this.Configuration.CodeManager.Query<T>(sqlQuery, connection, asTracked.HasValue ? asTracked.Value : this.Configuration.GetIsTrackedByDefault).SingleOrDefault();
        }

        public IEnumerable<T> Get<T>(IDbConnection connection, IEnumerable<int> ids, bool? asTracked) {
            var sqlQuery = this.SelectWriter.GenerateGetSql<T>(ids);
            return this.Configuration.CodeManager.Query<T>(sqlQuery, connection, asTracked.HasValue ? asTracked.Value : this.Configuration.GetIsTrackedByDefault);
        }

        public IEnumerable<T> Get<T>(IDbConnection connection, IEnumerable<Guid> ids, bool? asTracked) {
            var sqlQuery = this.SelectWriter.GenerateGetSql<T>(ids);
            return this.Configuration.CodeManager.Query<T>(sqlQuery, connection, asTracked.HasValue ? asTracked.Value : this.Configuration.GetIsTrackedByDefault);
        }

        public void Execute<T>(IDbConnection connection, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) {
            // generate a tracking class, apply the update, read out the updates
            var updateClass = this.Configuration.CodeManager.CreateUpdateInstance<T>();
            update(updateClass);
            var sqlQuery = this.UpdateWriter.GenerateBulkSql(updateClass, predicates);

            if (sqlQuery.Sql.Length > 0) {
                this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);
            }
        }

        public void ExecuteBulkDelete<T>(IDbConnection connection, IEnumerable<Expression<Func<T, bool>>> predicates) {
            var sqlQuery = this.DeleteWriter.GenerateBulkSql(predicates);
            this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);
        }
    }
}