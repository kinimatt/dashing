﻿namespace Dashing.SchemaReading {
    using System.Data;

    public class ColumnDto {
        public string TableName { get; set; }

        public string Name { get; set; }

        public string DbTypeName { get; set; }

        public DbType? DbType { private get; set; }

        public int? Precision { get; set; }

        public int? Scale { get; set; }

        public bool? MaxLength { get; set; }

        public int? Length { get; set; }

        public string Default { get; set; }

        public bool IsNullable { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsAutoGenerated { get; set; }
    }
}