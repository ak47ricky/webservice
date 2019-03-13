using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Configuration;

namespace webservice
{
    public partial class login : System.Web.UI.Page
    {
        public class AccountData
        {
            public int ID;
            public string account;
            public string password;
            public string nickname;
            public int loginkind;

            public AccountData()
            {
                ID = 0;
                account = string.Empty;
                password = string.Empty;
                nickname = string.Empty;
                loginkind = 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string aAccount = string.Empty;
            string aPassword = string.Empty;

            string aSqlStr = "select * from accountdata where loginkind = " + Common._ManagerID;

            List<AccountData> aListData = new List<AccountData>();
            aListData.Clear();

            try
            {
                aAccount = Request.QueryString["loginname"];
                aPassword = Request.QueryString["password"];

                using (MySqlConnection aCon = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySQL_Admin"].ConnectionString))
                {
                    aCon.Open();

                    using (MySqlCommand aCommand = new MySqlCommand(aSqlStr, aCon))
                    {
                        MySqlDataReader aDR = aCommand.ExecuteReader();

                        AccountData aData = new AccountData();

                        while (aDR.Read())
                        {
                            aData = new AccountData();

                            if (aAccount != aDR["account"].ToString())
                            {
                                Response.Write("1");
                                return;
                            }
                            if (aPassword != aDR["password"].ToString())
                            {
                                Response.Write("1");
                                return;
                            }

                            aData.account = aDR["account"].ToString();
                            aData.password = aDR["password"].ToString();
                            aData.nickname = aDR["nickname"].ToString();
                            aData.loginkind = int.Parse(aDR["loginkind"].ToString());

                            aListData.Add(aData);
                        }
                        aCon.Close();
                        aDR.Dispose();

                        string json_data = JsonConvert.SerializeObject(aListData, Formatting.Indented);
                        Response.Write(json_data);
                    }

                }
            }
            catch
            {
                Response.Write("99");
            }
        }
    }
}