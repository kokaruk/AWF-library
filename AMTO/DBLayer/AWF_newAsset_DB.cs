using System;
using System.Collections.Generic;
using System.Text;
using System.Data;


namespace AWFLib.DBLayer
{
    static internal class AWF_newAsset_DB
    {
        // Asset Exists boolean method
        static internal bool assetExists(string itemCode)
        {
            string sql = "SELECT id FROM asset where name= :stock_code";
            string[] parameterNames = new string[] { "stock_code" };
            string [] parameterVals = new string[] { itemCode };
            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);
            return !string.IsNullOrEmpty(scalarRequestString);
        }



    }
}
