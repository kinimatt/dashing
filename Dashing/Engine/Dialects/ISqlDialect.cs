namespace Dashing.Engine.Dialects {
    using System.Data;
    using System.Text;

    using Dashing.Configuration;

    public interface ISqlDialect {
        void AppendQuotedTableName(StringBuilder sql, IMap map);

        void AppendQuotedName(StringBuilder sql, string name);

        void AppendColumnSpecification(StringBuilder sql, IColumn column);

        void AppendEscaped(StringBuilder sql, string s);

        string WriteDropTableIfExists(string tableName);

        string GetIdSql();

        /// <summary>
        ///     Applies paging to the sql query
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <remarks>
        ///     The sql command will be past the parameters @take and @skip so those names should be used. It is assumed that
        ///     either take or skip are > 0
        /// </remarks>
        void ApplySkipTake(StringBuilder sql, StringBuilder orderClause, int take, int skip);

        bool TypeTakesLength(DbType type);

        bool TypeTakesPrecisionAndScale(DbType type);

        /// <summary>
        /// Changes the name of the column from the name in fromColumn to the name in toColumn
        /// </summary>
        /// <param name="fromColumn"></param>
        /// <param name="toColumn"></param>
        /// <returns></returns>
        /// <remarks>Assumes the table name given by toColumn and assumes the column structure given by fromColumn
        /// i.e. if the column specs are different they should not be changed by this statement</remarks>
        string ChangeColumnName(IColumn fromColumn, IColumn toColumn);

        /// <summary>
        /// Changes the column specification for a particular column
        /// </summary>
        /// <param name="fromColumn"></param>
        /// <param name="toColumn"></param>
        /// <returns></returns>
        /// <remarks>Assumes the column is named as in toColumn (i.e. use ChangeColumnName to change name) and the toColumn table</remarks>
        string ModifyColumn(IColumn fromColumn, IColumn toColumn);
    }
}