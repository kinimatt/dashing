﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Tools.ReverseEngineering
{
    public class DefaultConvention : IConvention
    {
        public string PropertyNameForManyToOneColumnName(string columnName)
        {
            if (columnName.EndsWith("Id"))
            {
                return columnName.Substring(0, columnName.Length - 2);
            }

            return columnName;
        }
    }
}
