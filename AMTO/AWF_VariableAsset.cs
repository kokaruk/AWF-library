using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AWFLib.DBLayer;
using RBA_Static;


namespace AWFLib
{
    public class AWF_VariableAsset
    {
        private Order myVasset { get; set; }
        private int itemSequence { get; set; }
        private readonly string myType = "1"; //variable
        private RBA_Variables _variables;


        private itemSize mySize;
        public itemSize MySize
        {
            get { return mySize; }
        }
        
        // constructor
        public AWF_VariableAsset(string pathToXML, int itemSequence)
        {
            AWF_VariableAsset_init(pathToXML, itemSequence);
        }

        public AWF_VariableAsset(string pathToXML, int itemSequence, RBA_Variables variables)
        {
            this._variables = variables;
            AWF_VariableAsset_init(pathToXML, itemSequence);
        }
        
        private void AWF_VariableAsset_init(string pathToXML, int itemSequence)
        {
            using (StreamReader str = new StreamReader(pathToXML))
            {
                XmlSerializer xSerializer = new XmlSerializer(typeof(Order));
                this.myVasset = (Order)xSerializer.Deserialize(str);
            }
            //prinergy iteration start from 1, not 0, need to decrease iteration number by constant value
            this.itemSequence = itemSequence - myConsts.prinergyIteratorNumber;
            this.mySize = AWF_VariableAsset_DB.getSize(this.myVasset.Products.Item[this.itemSequence].Part);
        }
        // end Constructor


        public string getPrinergyCustName()
        {
            return AWFAWFDB.prinergyCust(myVasset.Customer);
        }

        public string getOrderItemJobGroup()
        {
            return AWFAWFDB.newJobPath(myVasset.Customer);
        }       

        // order items questions
        public string getItemJobName()
        {
            if (this._variables == null)
            {
                throw new Exception("Varibles are not set for: " + myVasset.Orderno);
            }
            return string.Format("{0}{1} {2} {3}",this._variables.VASSET_PREFIX, myVasset.Orderno, myVasset.Products.Item[this.itemSequence].ID, myVasset.Products.Item[this.itemSequence].Part);
        }

        public string getItemDescription()
        {
            string itemDesc = myVasset.Products.Item[this.itemSequence].Desc;
            string[] itemDescriptionInfo = itemDesc.Split(new string[] { ")" }, 2, StringSplitOptions.RemoveEmptyEntries) ;
            
            if (itemDescriptionInfo.Length > 1)
            {
                itemDesc = Regex.Replace(itemDescriptionInfo[1].Trim(), @"[^\w\s]", String.Empty);
            }
            else
            {
                itemDesc = myVasset.Products.Item[this.itemSequence].Desc.Trim();
            }

            return itemDesc;
        }

        // database logs
        public string createNewVariableItemRecordInDb()
        {
            return AWF_DB_Log.createNewAsset(this.getItemJobName(), myVasset.Customer, this.myType);
        }

        public int updateItemRecordInDb(string assetId, string updateInfo, string eventId)
        {
            return AWF_DB_Log.updateItemRecordInDb(assetId, updateInfo, eventId);
        }


    }
}
