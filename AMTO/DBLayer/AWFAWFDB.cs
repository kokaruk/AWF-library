using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using RBA_Static;

namespace AWFLib.DBLayer
{
    static internal class AWFAWFDB
    {
        private struct asset
        {
            internal string Id;
            internal string Name;
        }

        // First check if items is Active variable
        internal static bool isActive(AWF myAWF, 
                                      int itemSequence,
                                       RBA_Variables variables) 
        {
            string sql = "SELECT status " +
                          "FROM asset " +
                          "WHERE name = :asset_name; ";
            string[] parameterNames = { "asset_name" };
            string[] parameterVals = { string.Format("{0}{1} {2} {3}", variables.VASSET_PREFIX,
                                                                    myAWF.Products.Item[itemSequence].SourcePDNumber, 
                                                                    myAWF.Products.Item[itemSequence].SourcePDLine,
                                                                    myAWF.Products.Item[itemSequence].ItemCode) };
            
            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);
            return String.IsNullOrEmpty(scalarRequestString) ? (isStaticActive(sql, parameterNames, myAWF, itemSequence))
                                                        : (0 == Convert.ToInt32(scalarRequestString));
        }
       
        //check if static item is active, i.e. exists or not disabled
        private static bool isStaticActive(string sql, string[] parameterNames, AWF myAWF, int itemSequence)
        {
            string[] parameterVals = { myAWF.Products.Item[itemSequence].ItemCode };

            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);
            
            if (!string.IsNullOrEmpty(scalarRequestString))
            {
                return (0 == Convert.ToInt32(scalarRequestString));
            }
            else
            {
                throw new FormatException(string.Format("job/pd#: {0}, item: {1}, returns no 'status' value from prinlut 'asset' table", myAWF.JobNumber.ToString(), myAWF.Products.Item[itemSequence].ItemCode));
            }
        }

        // product type
        internal static productType getProductType(AWF myAWF, int itemSequence, RBA_Variables variables)
        {
            string sql = "SELECT type " +
                          "FROM asset " +
                          "WHERE name = :asset_name; ";
            string[] parameterNames = { "asset_name" };
            string[] parameterVals = { string.Format("{0}{1} {2} {3}", variables.VASSET_PREFIX,
                                                                    myAWF.Products.Item[itemSequence].SourcePDNumber, 
                                                                    myAWF.Products.Item[itemSequence].SourcePDLine,
                                                                    myAWF.Products.Item[itemSequence].ItemCode) };

            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);
            return String.IsNullOrEmpty(scalarRequestString) ? productTypeStaticSearch(sql, parameterNames, myAWF, itemSequence)
                                                             : (productType)(Convert.ToInt32(scalarRequestString));
        }
        private static productType productTypeStaticSearch(string sql, string[] parameterNames, AWF myAWF, int itemSequence)
        {
            string[] parameterVals = { myAWF.Products.Item[itemSequence].ItemCode };

            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);
            if (!string.IsNullOrEmpty(scalarRequestString))
            {
                return (productType)(Convert.ToInt32(scalarRequestString));
            }
            else
            {
                throw new FormatException(string.Format("job/pd#: {0}, item: {1}, returns no 'type' value from DB", myAWF.JobNumber.ToString(), myAWF.Products.Item[itemSequence].ItemCode));
            }
        }

        // Path to job in Workshop
        internal static string newJobPath(string prismCustomer)
        {
            string sql = "SELECT prin_path "
                       + "FROM customer "
                       + "WHERE rparent = :prism_customer; ";
            string[] parameterNames = { "prism_customer" };
            string[] parameterVals = { prismCustomer };
            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);
            if (!string.IsNullOrEmpty(scalarRequestString))
            {
                return scalarRequestString;
            }
            else
            {
                throw new Exception("Customer: " + prismCustomer + " returns no 'prin_path' value from DB");
            }
        }

        // Find prinergy customer
        internal static string prinergyCust(string prismCustomer)
        {
            string sql = "SELECT prin_cust "
                       + "FROM customer "
                       + "WHERE rparent = :prism_customer;";
            string[] parameterNames = { "prism_customer" };
            string[] parameterVals = { prismCustomer };
            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);
            if (!string.IsNullOrEmpty(scalarRequestString))
            {
                return scalarRequestString;
            }
            else
            {
                throw new Exception("Customer: " + prismCustomer + " returns no 'prinergy customer' value from DB");
            }
        }


        // update aaudit
        internal static int updateAaudit(AWF myAWF, string actionString, string eventId, int itemSequence, RBA_Variables variables)
        {
            asset myAsset = getAsset(myAWF, itemSequence, variables); 

            switch (getProductType(myAWF, itemSequence, variables))
            {
                case productType.Static:
                    AWF_DB_Log.insertAauditRecordInDb(myAsset.Id, eventId, string.Format("{0}: {1} | Job Number: {2}", actionString, myAWF.Products.Item[itemSequence].ItemCode, myAWF.JobNumber.ToString()));
                    break;
                case productType.Variable:
                    AWF_DB_Log.insertAauditRecordInDb(myAsset.Id, eventId, string.Format("{0}: {1}", actionString, myAsset.Name));
                    break;
            }

            string sql = "UPDATE asset "
                       + "SET tlc=(SELECT DISTINCT tc "
                       + "FROM aaudit WHERE id = :asset_id "
                       + "AND seq = (SELECT DISTINCT MAX(seq) "
                       + "FROM aaudit WHERE id = :asset_id ) LIMIT 1) "
                       + (actionString.Contains("DESTROY/CANCEL") ? ", status = 3" : String.Empty)
                       + "WHERE id = :asset_id; ";
            string[] parameterNames = { "asset_id" };
            string[] parameterVals = { myAsset.Id }; 
            return AWFPostgresDataLayer.ExecuteNonQuery(sql, parameterNames, parameterVals);
        }
        private static asset getAsset(AWF myAWF, int itemSequence, RBA_Variables variables)
        {

            string sql = string.Empty;
            string itemCode = string.Format("%{0}%", myAWF.Products.Item[itemSequence].ItemCode);
            string PDNumber = string.Format("{0}%", variables.VASSET_PREFIX + myAWF.Products.Item[itemSequence].SourcePDNumber);
            string[] parameterNames = new string[1];
            string[] parameterVals = new string[1];
            switch (getProductType(myAWF, itemSequence, variables))
            {   
                case productType.Static:
                    sql = "SELECT id, name "
                        + "FROM asset "
                        + "WHERE name LIKE :stock_code "
                        + "LIMIT 1";
                    parameterNames = new string[] { "stock_code" };
                    parameterVals = new string[] { itemCode };
                    break;
                case productType.Variable:
                    sql = "SELECT id, name "
                        + "FROM asset "
                        + "WHERE name LIKE :printdirect_number "
                        + "AND name LIKE :stock_code "
                        + "LIMIT 1";
                    
                    parameterNames = new string[] { "printdirect_number", "stock_code" };
                    parameterVals = new string[] { PDNumber, itemCode };
                    break;
            }

            DataTable rtTable = AWFPostgresDataLayer.GetDataTable(sql, parameterNames, parameterVals);

            if (rtTable.Rows.Count == 1)
            {
                return new asset { Id = Convert.ToString(rtTable.Rows[0]["id"]), Name = Convert.ToString(rtTable.Rows[0]["name"]) }; 
            }
            else
            {
                throw new Exception("Asset not found in Asset Table");
            }
        }

        // impostion process
        internal static string impositionProcess(string workstyle, string machine, string itemCode)
        {
            string sql = "SELECT CONCAT(imposition.name, imposition.method) "
                       + "FROM  imposition "
                       + "RIGHT JOIN process "
                       + "ON CAST(COALESCE(imposition.process,'0') AS INTEGER ) = process.id "
                       + "RIGHT JOIN productgroup "
                       + "ON imposition.id = productgroup.impid "
                       + "RIGHT JOIN productgroupmembers "
                       + "ON productgroup.id = productgroupmembers.groupid "
                       + "WHERE imposition.workstyle = :workstyle "
                       + "AND process.process = :machine "
                       + "AND productgroupmembers.stockcode = :item_code; ";
            string[] parameterNames = { "workstyle", "machine" , "item_code" };
            string[] parameterVals = {workstyle, machine, itemCode };
            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);
            if (!string.IsNullOrEmpty(scalarRequestString))
            {
                return scalarRequestString;
            }
            else
            {
                throw new Exception("Can't Find Imposition For: " + itemCode);
            }
        }
        // jdf file destination
        internal static string jdfDestination(string machine)
        {
            string sql = "SELECT dest "
                       + "FROM process "
                       + "WHERE process = :machine";
            string[] parameterNames = { "machine" };
            string[] parameterVals = { machine };
            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);
            if (!string.IsNullOrEmpty(scalarRequestString))
            {
                return scalarRequestString;
            }
            else
            {
                throw new Exception("Can't find JDF destination for: " + machine);
            }
        }

        // imposition stepping info
        internal static int impoSteppingInfo(string machine, string itemCode)
        {
            string sql = "SELECT CAST(imposition.sqty AS FLOAT) / CAST( COALESCE(imposition.extrasteps, 1) AS FLOAT) as sqty "
                       + "FROM  imposition "
                       + "RIGHT JOIN process "
                       + "ON CAST(COALESCE(imposition.process,'0') AS INTEGER ) = process.id "
                       + "RIGHT JOIN productgroup "
                       + "ON imposition.id = productgroup.impid "
                       + "RIGHT JOIN productgroupmembers "
                       + "ON productgroup.id = productgroupmembers.groupid "
                       + "WHERE process.process = :machine "
                       + "AND productgroupmembers.stockcode = :item_code; ";
            string[] parameterNames = { "machine", "item_code" };
            string[] parameterVals = { machine, itemCode };
            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);

            if (!string.IsNullOrEmpty(scalarRequestString))
            {
                return Convert.ToInt32(scalarRequestString);
            }
            else
            {
                throw new FormatException("Can't find Imposition stepping for: " + itemCode);
            }
        }

        // search for imposition method (for T230 RBA Branch)
        internal static string impoMethod(string machine, string itemCode)
        {
            string sql = "SELECT imposition.method "
                       + "FROM  imposition "
                       + "RIGHT JOIN process "
                       + "ON CAST(COALESCE(imposition.process,'0') AS INTEGER ) = process.id "
                       + "RIGHT JOIN productgroup "
                       + "ON imposition.id = productgroup.impid "
                       + "RIGHT JOIN productgroupmembers "
                       + "ON productgroup.id = productgroupmembers.groupid "
                       + "WHERE process.process = :machine "
                       + "AND productgroupmembers.stockcode = :item_code; ";
            string[] parameterNames = { "machine", "item_code" };
            string[] parameterVals = { machine, itemCode };
            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);
            if (!string.IsNullOrEmpty(scalarRequestString))
            {
                return scalarRequestString;
            }
            else
            {
                throw new FormatException("Can't find Imposition stepping for: " + itemCode);
            }
        }
    
        // digital / offset
        internal static bool isDigital(string machine)
        {
                return processType(machine).Equals("Digital");           
        }

        internal static string processType(string machine)
        {
            string sql = "SELECT type "
                       + "FROM process "
                       + "WHERE process = :machine";
            string[] parameterNames = { "machine" };
            string[] parameterVals = { machine };
            string scalarRequestString = AWFPostgresDataLayer.SelectScalar(sql, parameterNames, parameterVals);
            if (!string.IsNullOrEmpty(scalarRequestString))
            {
                return scalarRequestString;
            }
            else
            {
                throw new Exception("Can't find process type for: " + machine);
            }
        }

        // update Asset
        internal static int updateApprovedAsset(String itemCode){
            string sql = "SELECT update_asset( :item_code )";
			string[] parameterNames = { "item_code" };
			string[] parameterVals = { itemCode };

            return AWFPostgresDataLayer.updateTransaction(sql, parameterNames, parameterVals);        
        }

    }
}
