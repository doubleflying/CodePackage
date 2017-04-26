/// <summary>
/// 将泛类型集合List类转换成DataTable
/// </summary>
/// <param name="list">泛类型集合</param>
/// <returns></returns>
public static DataTable ListToDataTable<T>(List<T> entitys)
{
    //检查实体集合不能为空
    if (entitys == null || entitys.Count < 1)
    {
        throw new Exception("需转换的集合为空");
    }
    //取出第一个实体的所有Propertie
    Type entityType = entitys[0].GetType();
    PropertyInfo[] entityProperties = entityType.GetProperties();
    //生成DataTable的structure
    //生产代码中，应将生成的DataTable结构Cache起来，此处略
    DataTable dt = new DataTable();
    for (int i = 0; i < entityProperties.Length; i++)
    {
        //dt.Columns.Add(entityProperties[i].Name,  entityProperties[i].PropertyType);
        dt.Columns.Add(entityProperties[i].Name);
    }
    //将所有entity添加到DataTable中
    foreach (object entity in entitys)
    {
        //检查所有的的实体都为同一类型
        if (entity.GetType() != entityType)
        {
            throw new Exception("要转换的集合元素类型不一致");
        }
        object[] entityValues = new object[entityProperties.Length];
        for (int i = 0; i < entityProperties.Length; i++)
        {
            entityValues[i] = entityProperties[i].GetValue(entity, null);
        }
        dt.Rows.Add(entityValues);

        return dt;
}




/// <summary>
/// 使用SqlBulkCopy批量插入数据
/// Attention:
/// 表明和列名需要与数据库表统一
/// </summary>
/// <param name="dt"></param>
/// <param name="tableName"></param>
public static void BulkInsert(DataTable dt, string tableName)
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        conn.Open();

        //事务锁
        SqlTransaction bulkTrans = conn.BeginTransaction();
        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, bulkTrans)
        { BatchSize = 1000, DestinationTableName = tableName })
        {
            if (dt != null)
            {
                try
                {
                    foreach (DataColumn dc in dt.Columns)
                    {
                        ///映射列
                        bulkCopy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                    }
                    bulkCopy.WriteToServer(dt);
                    bulkTrans.Commit();
                }
                catch (Exception ex)
                {
                    bulkTrans.Rollback();
                    //SQLLog.Error("Execute BulkCopy failed with exception:" + ex.Message + "\r\nTableName:" + tableName + "DateTime:" + DateTime.Now);
                    throw ex;
                }
            }
        }
    }
}        