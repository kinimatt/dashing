﻿namespace TopHat.Tests {
    using System;
    using System.Data;

    using Microsoft.QualityTools.Testing.Fakes;

    using Moq;

    using TopHat.Engine;

    public class BaseQueryWriterTest : IDisposable {
        protected readonly Mock<IDbConnection> Connection;

        protected readonly Mock<IDbTransaction> Transaction;

        private readonly IDisposable shimsContext;

        public BaseQueryWriterTest() {
            this.Connection = new Mock<IDbConnection>(MockBehavior.Strict);
            this.Connection.Setup(c => c.State).Returns(ConnectionState.Open);
            this.Transaction = new Mock<IDbTransaction>(MockBehavior.Strict);
            this.shimsContext = ShimsContext.Create();
        }

        protected ISession GetTopHat() {
            // Dapper.Fakes.ShimSqlMapper.ExecuteIDbConnectionStringObjectIDbTransactionNullableOfInt32NullableOfCommandType = (connection, SqlWriter, parameters, transaction, timeout, type) => 1;
            var session = new Session(new SqlServerEngine(), this.Connection.Object, this.Transaction.Object);
            return session;
        }

        public void Dispose() {
            this.shimsContext.Dispose();
        }
    }
}