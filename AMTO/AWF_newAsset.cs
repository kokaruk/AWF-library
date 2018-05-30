using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using AWFLib.DBLayer;

namespace AWFLib
{
    public class AWF_newAsset
    {
        private Items myNewAsset { get; set; }
        private int itemSequence { get; set; }
        //prinergy iteration start from 1, no 0, need to decrease iteration number by constant value
        private const int prinergyIteratorNumber = 1;

        public AWF_newAsset(string pathToXML, int itemSequence)
        {
            using (StreamReader str = new StreamReader(pathToXML))
            {
                XmlSerializer xSerializer = new XmlSerializer(typeof(AWF));
                this.myNewAsset = (Items)xSerializer.Deserialize(str);
            }
            this.itemSequence = itemSequence - 1;
        }

        // Asset Exists boolean method
        public bool assetExists()
        {
            return AWF_newAsset_DB.assetExists(this.myNewAsset.Item[itemSequence].Stock);
        }

        // Create new Asset
        public string createNewAsset()
        {
            string myType = "0"; //static
            return AWF_DB_Log.createNewAsset(this.myNewAsset.Item[itemSequence].Stock, 
                                                   this.myNewAsset.Item[itemSequence].Customer,
                                                   myType);
        }

    }
}
