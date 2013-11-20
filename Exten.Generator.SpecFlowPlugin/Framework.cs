using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;

namespace Exten.Generator.SpecFlowPlugin
{
    class Framework
    {
        public readonly string xlFile = "\\TestData\\TestData.xls";
        public OleDbConnection con;

        public void ConnectToExcel()
        {

            var s1 = AppDomain.CurrentDomain.BaseDirectory;

            var sourceLocation = string.Concat(s1.Substring(0, s1.LastIndexOf("\\bin")), xlFile);

            string conString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data source=" + sourceLocation + ";Extended Properties=Excel 8.0";

            con = new OleDbConnection(conString);

            con.Open();

        }

        public OleDbDataReader RunReadQuery(string qry)
        {
            try
            {
                if (con == null) ConnectToExcel();
                OleDbCommand cmd = new OleDbCommand(qry, con);

                OleDbDataReader oleRead = cmd.ExecuteReader();

                /* while (oleRead.Read())
                 {

                     Console.WriteLine(oleRead[0] + " " + oleRead[1] + " " + oleRead[2]);

                 }*/
                return oleRead;

            }
            finally
            {
                //con.Close();

            }
        }
    }
}
