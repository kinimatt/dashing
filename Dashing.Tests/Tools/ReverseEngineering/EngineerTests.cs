﻿namespace Dashing.Tests.Tools.ReverseEngineering {
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Tools.ReverseEngineering;

    using DatabaseSchemaReader.DataSchema;

    using Xunit;

    public class EngineerTests {
        [Fact]
        public void PrimaryKeySetCorrectly() {
            var engineer = new Engineer();
            var maps = engineer.ReverseEngineer(this.GetSchema());
            Assert.Equal("PostId", maps.First(m => m.Table == "Posts").PrimaryKey.Name);
        }

        [Fact]
        public void AutoIncSetCorrectly() {
            var engineer = new Engineer();
            var maps = engineer.ReverseEngineer(this.GetSchema());
            Assert.True(maps.First(m => m.Table == "Posts").PrimaryKey.IsAutoGenerated);
        }

        [Fact]
        public void ManyToOneSetCorrectly() {
            var engineer = new Engineer();
            var maps = engineer.ReverseEngineer(this.GetSchema());
            Assert.True(maps.First(m => m.Table == "Posts").Columns.First(c => c.Key == "Blog").Value.Relationship == RelationshipType.ManyToOne);
        }

        private DatabaseSchema GetSchema() {
            return this.MakeSchema();
        }

        private DatabaseSchema MakeSchema() {
            var schema = new DatabaseSchema(string.Empty, SqlType.SqlServer);
            var postTable = new DatabaseTable { Name = "Posts" };

            postTable.Columns.Add(new DatabaseColumn { IsIdentity = true, IsPrimaryKey = true, Name = "PostId", DataType = new DataType("int", "System.Int32") });
            postTable.Columns.Add(new DatabaseColumn { Name = "BlogId", IsForeignKey = true, ForeignKeyTableName = "Blogs", DataType = new DataType("int", "System.Int32") });

            schema.Tables.Add(postTable);
            return schema;
        }
    }
}