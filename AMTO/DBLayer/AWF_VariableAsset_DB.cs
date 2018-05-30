using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace AWFLib.DBLayer
{
    static internal class AWF_VariableAsset_DB
    {

        internal static itemSize getSize(string stockCode)
        {

            string sql = @"SELECT imposition.sizex As ""SizeX"",  imposition.sizey As ""SizeY"" FROM imposition " +
                            "LEFT JOIN productgroup " + 
                            "ON imposition.id = productgroup.impid " + 
                            "LEFT JOIN productgroupmembers " + 
                            "ON productgroup.id = productgroupmembers.groupid " + 
                            "WHERE stockcode = :item_code " + 
                            "LIMIT 1; ";
            string[] parameterNames = { "item_code" };
            string[] parameterVals = { stockCode };

            DataTable rtTable;
            try
            {
                rtTable = AWFPostgresDataLayer.GetDataTable(sql, parameterNames, parameterVals);
            }
            catch
            {
                throw;
            }

            if (rtTable.Rows.Count == 1)
            {
                return new itemSize { width = new Size(Convert.ToInt32((rtTable.Rows[0]["SizeX"]))), height = new Size(Convert.ToInt32((rtTable.Rows[0]["SizeY"]))) };
            }
            else
            {
                throw new Exception(String.Format("No Sizes found for the item: {0}", stockCode));
            }
        }
    }
}