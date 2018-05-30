using System;
using System.Reflection;
using System.Xml.Serialization;

namespace AWFLib
{
    public static class myConsts
    {
        public const int prinergyIteratorNumber = 1;
    }

    public enum productType
    {
        Static,
        Variable
    };

    public struct itemSize
    {
        public Size width;
        public Size height;
    }

    public class Size
    {
        private const double toPointsConversion = 2.83464567;
        private double value;
        public double Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public double toPoints
        {
            get { return this.Value * toPointsConversion; }
        }

        public Size(int sizeValue)
        {
            Value = Convert.ToDouble(sizeValue);
        }
    }
    
    public static class GetXmlAttribValue
    {
        public static string GetAttribue(Type type, string itemname)
        {

            FieldInfo vFieldInfo = type.GetField(itemname);
            Attribute vAttribute = Attribute.GetCustomAttribute(

                vFieldInfo, typeof(System.Xml.Serialization.XmlEnumAttribute));

            if (vAttribute != null)
                return ((System.Xml.Serialization.XmlEnumAttribute)vAttribute).Name;
            else return itemname;

        }
    }
}