using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using AGV_Scanner;
using System.Xml;
using Const;

namespace AGV_Scanner
{
    class ReadExpressList
    {
        public  static  void readXml()
        {
            AGVConstDefine.pathExpressFile = ConfigurationManager.AppSettings["ExpressXml"].ToString(); ;
            AGVConstDefine.expressFile= new XmlDocument();
            AGVConstDefine.expressFile.Load(AGVConstDefine.pathExpressFile);
            AGVConstDefine.expressList = AGVConstDefine.expressFile.SelectSingleNode("ExpressList").ChildNodes;
            AGVConstDefine.sumExpress = AGVConstDefine.expressList.Count;
        }
       

    }
}
