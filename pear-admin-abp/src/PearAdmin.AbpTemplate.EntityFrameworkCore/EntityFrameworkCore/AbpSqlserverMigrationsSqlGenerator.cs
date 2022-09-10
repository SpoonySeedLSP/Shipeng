using Abp.Extensions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PearAdmin.AbpTemplate.EntityFrameworkCore.EntityFrameworkCore
{
    /// <summary>
    /// abp 框架拓展sqlserver 迁移：增加数据库表和列备注
    /// </summary>
    public class AbpSqlserverMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
    {
        public AbpSqlserverMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            IRelationalAnnotationProvider relationalAnnotationProvider)
            : base(dependencies,relationalAnnotationProvider)
        {
        }

        protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);

            if (operation is CreateTableOperation || operation is AlterTableOperation)
                CreateTableComment(operation, model, builder);

            if (operation is AddColumnOperation || operation is AlterColumnOperation)
                CreateColumnComment(operation, model, builder);
        }

        /// <summary>
        /// 创建表注释
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="builder"></param>
        private void CreateTableComment(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            string tableName = string.Empty;
            string description = string.Empty;
            if (operation is AlterTableOperation)
            {
                var t = operation as AlterColumnOperation;
                tableName = (operation as AlterTableOperation).Name;
            }

            if (operation is CreateTableOperation)
            {
                var t = operation as CreateTableOperation;
                var addColumnsOperation = t.Columns;
                tableName = t.Name;

                foreach (var item in addColumnsOperation)
                {
                    CreateColumnComment(item, model, builder);
                    CreateColumnComment(item, model, builder);
                }
            }

            description = DbDescriptionHelper.GetDescription(tableName.Replace(AbpTemplateCoreConsts.DbTablePrefix, ""));

            if (tableName.IsNullOrWhiteSpace())
                throw new Exception("表名为空引起添加表注释异常.");

            var sqlHelper = Dependencies.SqlGenerationHelper;
            builder
            .Append("ALTER TABLE ")
            .Append(sqlHelper.DelimitIdentifier(tableName).ToLower())
            .Append(" COMMENT ")
            .Append("'")
            .Append(description)
            .Append("'")
            .AppendLine(sqlHelper.StatementTerminator)
            .EndCommand();

        }

        /// <summary>
        /// 创建列注释
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="builder"></param>
        private void CreateColumnComment(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            //alter table a1log modify column UUID VARCHAR(26) comment '修改后的字段注释';
            string tableName = string.Empty;
            string columnName = string.Empty;
            string columnType = string.Empty;
            string description = string.Empty;
            if (operation is AlterColumnOperation)
            {
                var t = (operation as AlterColumnOperation);
                tableName = t.Table;
                columnName = t.Name;
                var o = new ColumnOperationDerived() as ColumnOperation;
                o.ClrType = t.ClrType;
                o.ColumnType = t.ColumnType;
                o.Comment = t.Comment;
                o.IsUnicode = t.IsUnicode;
                o.MaxLength = t.MaxLength;
                o.IsFixedLength = t.IsFixedLength;
                o.IsRowVersion = t.IsRowVersion;
                columnType = base.GetColumnType(t.Schema, t.Table, t.Name, o, model);
            }

            if (operation is AddColumnOperation)
            {
                var t = (operation as AddColumnOperation);
                tableName = t.Table;
                columnName = t.Name;
                var o = new ColumnOperationDerived() as ColumnOperation;
                o.ClrType = t.ClrType;
                o.ColumnType = t.ColumnType;
                o.Comment = t.Comment;
                o.IsUnicode = t.IsUnicode;
                o.MaxLength = t.MaxLength;
                o.IsFixedLength = t.IsFixedLength;
                o.IsRowVersion = t.IsRowVersion;
                columnType = GetColumnType(t.Schema, t.Table, t.Name, o, model);
                description = DbDescriptionHelper.GetDescription(tableName.Replace(AbpTemplateCoreConsts.DbTablePrefix, ""), columnName);
            }

            if (columnName.IsNullOrWhiteSpace() || tableName.IsNullOrWhiteSpace() || columnType.IsNullOrWhiteSpace())
                throw new Exception("列名为空或表名为空或列类型为空引起添加列注释异常." + columnName + "/" + tableName + "/" + columnType);

            var sqlHelper = Dependencies.SqlGenerationHelper;
            builder
            .Append("ALTER TABLE ")
            .Append(sqlHelper.DelimitIdentifier(tableName).ToLower())
            .Append(" MODIFY COLUMN ")
            .Append(columnName)
            .Append(" ")
            .Append(columnType)
            .Append(" COMMENT ")
            .Append("'")
            .Append(description)
            .Append("'")
            .AppendLine(sqlHelper.StatementTerminator)
            .EndCommand();


        }
    }

    /// <summary>
    /// 抽象类一般都做为基类，派生类对象是可以被创建的，因此可以创建派生类对象，再将其转化成基类
    /// </summary>
    public class ColumnOperationDerived : ColumnOperation
    {
    }


    public class DbDescriptionHelper
    {
        /// <summary>
        /// 命名空间
        /// </summary>
        public static string _assemblyNamespace { get; set; } = "HmCoreEShopBg.Domain";

        public static List<DbDescription> list { get; set; }

        /// <summary>
        /// 获取表或者字段属性
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string GetDescription(string table, string column = "")
        {
            if (list == null || list.Count() == 0)
            {
                list = GetDescription();
            }

            if (!string.IsNullOrWhiteSpace(table))
            {
                if (string.IsNullOrWhiteSpace(column))
                {
                    var x = list.FirstOrDefault(p => p.Name == table);
                    if (x != null)
                        return x.Description;
                    return string.Empty;
                }
                else
                {
                    var x = list.FirstOrDefault(p => p.Name == table);
                    if (x != null)
                    {
                        var y = x.Column;
                        if (y != null)
                        {
                            var z = y.FirstOrDefault(p => p.Name == column);
                            if (z != null)
                                return z.Description;
                        }
                    }
                    return string.Empty;
                }
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// 获取程序集所有对象注释
        /// </summary>
        /// <returns></returns>
        private static List<DbDescription> GetDescription()
        {
            var descriptionList = new List<DbDescription>();

            var entityAssembly = Assembly.Load(_assemblyNamespace);
            var allType = entityAssembly?.GetTypes();
            var allClass = allType?.Where(t => t.IsClass).ToList();

            foreach (var h in allClass)
            {
                //表注释
                var tableDescription = new DbDescription();
                tableDescription.Column = new List<DbDescription>();
                tableDescription.Name = h.Name;
                var tobjs = h.GetCustomAttributes(typeof(DescriptionAttribute), true);
                tableDescription.Description = GetPropertyDescription(h);

                //所有属性注释
                PropertyInfo[] peroperties = h.GetProperties();
                foreach (PropertyInfo property in peroperties)
                {
                    var column = new DbDescription();
                    column.Name = property.Name;
                    column.Description = GetPropertyDescription(property);

                    tableDescription.Column.Add(column);
                }

                descriptionList.Add(tableDescription);
            }
            return descriptionList;
        }

        /// <summary>
        /// 获取属性注释
        /// </summary>
        /// <returns></returns>
        private static string GetPropertyDescription(PropertyInfo property)
        {
            var objs = property.GetCustomAttributes(typeof(DescriptionAttribute), true);
            return CommondDescription(objs);
        }

        /// <summary>
        /// 获取对象注释
        /// </summary>
        /// <returns></returns>
        private static string GetPropertyDescription(Type type)
        {
            var objs = type.GetCustomAttributes(typeof(DescriptionAttribute), true);
            return CommondDescription(objs);
        }

        private static string CommondDescription(object[] objs)
        {
            if (objs != null && objs.Length > 0)
            {
                return ((DescriptionAttribute)objs[0]).Description;
            }
            return "";
        }
    }

    /// <summary>
    /// 表注释信息
    /// </summary>
    public class DbDescription
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 注释
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 表字段集合
        /// </summary>
        public List<DbDescription> Column { get; set; }
    }

}
