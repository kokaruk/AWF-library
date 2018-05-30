using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using AWFLib.DBLayer;
using RBA_Static;

namespace AWFLib
{
    public class AWForder
    {
        private AWF myAWF { get; set; }
        private int itemSequence { get; set; }
        private RBA_Variables _variables;
        
        
        // constructor with params
        public AWForder(string pathToXML, int itemSequence)
        {
            this.AWForder_init(pathToXML, itemSequence);
        }

        public AWForder(string pathToXML, int itemSequence, RBA_Variables variables)
        {
            this._variables = variables;
            this.AWForder_init(pathToXML, itemSequence);
        }

        private void AWForder_init(string pathToXML, int itemSequence)
        {
            using (StreamReader str = new StreamReader(pathToXML))
            {
                XmlSerializer xSerializer = new XmlSerializer(typeof(AWF));
                this.myAWF = (AWF)xSerializer.Deserialize(str);
            }
            //prinergy iteration start from 1, not 0, need to decrease iteration number by constant value
            this.itemSequence = itemSequence - myConsts.prinergyIteratorNumber;
        }

        // Check status
        public bool isActive()
        {
            return AWFAWFDB.isActive(myAWF, this.itemSequence, this._variables);            
        }
        // my job path
        public string newJobPath()
        {
            return AWFAWFDB.newJobPath(myAWF.Products.Item[this.itemSequence].ItemCust);
        }
        // product type
        public productType getProductType()
        {
            return AWFAWFDB.getProductType(myAWF, this.itemSequence, this._variables);
        }
        //prinergy customer
        public string getPrinergyCust()
        {
            return AWFAWFDB.prinergyCust(myAWF.Products.Item[this.itemSequence].ItemCust);
        }
        // update AAUDIT
        public int updateAaudit(string actionString, string eventId)
        {
            if (this._variables == null)
            {
                throw new Exception("RBA Varibles are not set for: " + myAWF.JobNumber.ToString());
            }
            return AWFAWFDB.updateAaudit(myAWF, actionString, eventId, itemSequence, _variables);
        }

        // impositionProcess
        public string impositionProcess()
        {

            string xmlWorkStyleEnumValue = GetXmlAttribValue.GetAttribue(typeof(AWFWorkstyle), myAWF.Workstyle.ToString());

            return AWFAWFDB.impositionProcess(xmlWorkStyleEnumValue,
                                                myAWF.Machine,
                                                myAWF.Products.Item[this.itemSequence].ItemCode);
        }

        // jdf file destination
        public string jdfDestination()
        {
            return AWFAWFDB.jdfDestination(myAWF.Machine);
        }

        //imposition stepping ingo
        public int impoSteppingInfo()
        {
            return AWFAWFDB.impoSteppingInfo(myAWF.Machine, myAWF.Products.Item[this.itemSequence].ItemCode);
        }

        // digital / offset
        public bool isDigital()
        {
            return AWFAWFDB.isDigital(myAWF.Machine);
        }

        public string processType()
        {
            return AWFAWFDB.processType(myAWF.Machine);
        }

        public string impositionMethod()
        {
            return AWFAWFDB.impoMethod(myAWF.Machine, myAWF.Products.Item[this.itemSequence].ItemCode);
        }

        // update approved asset, usind custom function in postgres
        public static int updateAsset(string itemCode){
            return AWFAWFDB.updateApprovedAsset(itemCode);   
        }

    }
}



