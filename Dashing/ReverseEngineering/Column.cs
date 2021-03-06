﻿namespace Dashing.ReverseEngineering {
    using System;
    using System.Collections.Generic;

    using Dashing.SchemaReading;

    internal class Column<T> : Dashing.Configuration.Column<T>, IReverseEngineeringColumn {
        private readonly object typeLock = new object();

        private Type actualType;

        public override Type Type
        {
            get
            {
                if (this.actualType == null) {
                    lock (this.typeLock) {
                        if (this.actualType == null) {
                            if (string.IsNullOrEmpty(this.ForeignKeyTableName)) {
                                this.actualType = base.Type;
                            }
                            else {
                                this.actualType = this.TypeMap[this.ForeignKeyTableName];
                            }
                        }
                    }
                }

                return this.actualType;
            }
        }

        public string ForeignKeyTableName { get; set; }

        public IDictionary<string, Type> TypeMap { get; set; }

        public ColumnDto ColumnSpecification { get; set; }
    }
}