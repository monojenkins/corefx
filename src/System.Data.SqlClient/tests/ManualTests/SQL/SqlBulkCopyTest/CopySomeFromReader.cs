// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace System.Data.SqlClient.ManualTesting.Tests
{
    public class CopySomeFromReader
    {
        public static void Test(string srcConstr, string dstConstr, string dstTable)
        {
            using (SqlConnection dstConn = new SqlConnection(dstConstr))
            using (SqlCommand dstCmd = dstConn.CreateCommand())
            {
                dstConn.Open();

                try
                {
                    Helpers.Execute(dstCmd, "create table " + dstTable + " (col1 int, col2 nvarchar(20), col3 nvarchar(10), col4 datetime)");

                    using (SqlConnection srcConn = new SqlConnection(srcConstr))
                    using (SqlCommand srcCmd = new SqlCommand("select * from employees", srcConn))
                    {
                        srcConn.Open();

                        using (DbDataReader reader = srcCmd.ExecuteReader())
                        {
                            using (SqlBulkCopy bulkcopy = new SqlBulkCopy(dstConn))
                            {
                                bulkcopy.DestinationTableName = dstTable;
                                bulkcopy.BatchSize = 6;

                                SqlBulkCopyColumnMappingCollection ColumnMappings = bulkcopy.ColumnMappings;

                                ColumnMappings.Add("EmployeeID", "col1");
                                ColumnMappings.Add("BirthDate", "col4");
                                ColumnMappings.Add("FirstName", "col2");
                                ColumnMappings.Add("LastName", "col3");

                                bulkcopy.WriteToServer(reader);
                            }
                            Helpers.VerifyResults(dstConn, dstTable, 4, 9);
                        }
                    }
                }
                finally
                {
                    Helpers.TryExecute(dstCmd, "drop table " + dstTable);
                }
            }
        }
    }
}
