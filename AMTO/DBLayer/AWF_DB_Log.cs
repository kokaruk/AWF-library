using System;
using System.Collections.Generic;
using System.Text;

namespace AWFLib.DBLayer
{
    static internal class AWF_DB_Log
    {
        // Create new Asset, return asset id
        static internal string createNewAsset(string itemName, string prismCust, string itemType)
        {
            string sql = "INSERT INTO asset(name, type, status, tlc, custid ) "
                       + "VALUES ( :item_code , :item_type, 0,  current_timestamp, "
                       + "(SELECT id FROM customer WHERE rparent = :prism_customer)); "
                       + "SELECT currval('aid'::regclass) as asset_id;";
            string[] parameterNames = new string[] { "item_code", "item_type", "prism_customer" };
            string[] parameterVals = new string[] { itemName, itemType, prismCust };
            string assetId = AWFPostgresDataLayer.insertSelectTransaction(sql, parameterNames, parameterVals);
            string eventID = "1"; // create event
            insertAauditRecordInDb(assetId, eventID, itemName + ": CREATE");
            return assetId;
        }

        // updateItemRecordInDb for variable item only!!! AWFAWFBD static class contains combined method for static and variable 
        static internal int updateItemRecordInDb(string assetId, string updateInfo, string eventId)
        {
            string sql = "UPDATE asset "
                       + "SET tlc=(SELECT DISTINCT tc "
                       + "FROM aaudit WHERE id = :asset_id "
                       + "AND seq = (SELECT DISTINCT MAX(seq) "
                       + "FROM aaudit WHERE id = :asset_id ) LIMIT 1) "
                       + (updateInfo.Contains("DESTROY/CANCEL") ? ", status = 3" : String.Empty)
                       + "WHERE id = :asset_id; ";
            string[] parameterNames = { "asset_id" };
            string[] parameterVals = { assetId };
            insertAauditRecordInDb(assetId, eventId, updateInfo);
            return AWFPostgresDataLayer.ExecuteNonQuery(sql, parameterNames, parameterVals);
        }


        static internal int insertAauditRecordInDb(string assetId, string eventId, string updateInfo)
        {
            string sql = "INSERT INTO aaudit "
                       + "VALUES (:asset_id, coalesce((select max(seq)+1 "
                       + "FROM aaudit where id = :asset_id),1), :event_id, :update_info); ";
            string[] parameterNames = { "asset_id", "event_id", ":update_info" };
            string[] parameterVals = { assetId, eventId, updateInfo };
            return AWFPostgresDataLayer.updateTransaction(sql, parameterNames, parameterVals);
        }
    }
}
