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
    public partial class account_control : System.Web.UI.Page
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
            string aSqlStr = "select * from accountdata";

            List<AccountData> aListData = new List<AccountData>();
            aListData.Clear();

            int aKind = 0;

            try
            {
                aKind = int.Parse(Request.QueryString["Kind"].ToString());
            }
            catch
            {
                Response.Write("99");
            }

            //取得所有人員資料
            if (aKind == 0)
            {
                aSqlStr = "select * from accountdata where loginkind != " + Common._ManagerID;

                try
                {
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
                                aData.ID = int.Parse(aDR["ID"].ToString());
                                aData.account = aDR["account"].ToString();
                                aData.password = aDR["password"].ToString();
                                aData.nickname = aDR["nickname"].ToString();
                                aData.loginkind = int.Parse(aDR["loginkind"].ToString());

                                aListData.Add(aData);
                            }
                            aCon.Close();
                            aDR.Dispose();

                            AccountData[] aResult = aListData.ToArray();

                            string json_data = JsonConvert.SerializeObject(aResult, Formatting.Indented);
                            Response.Write(json_data);
                        }

                    }
                }
                catch
                {
                    Response.Write("99");
                }
            }
            else if (aKind == 1)//新增玩家資料
            {
                try
                {
                    aSqlStr = "INSERT INTO dokkansuper.accountdata (account, password, nickname, loginkind) VALUES ('{0}', '{1}', '{2}', '{3}');";

                    AccountData aData = new AccountData();

                    aData.account = Request.Form["account"];
                    aData.password = Request.Form["password"];
                    aData.nickname = Request.Form["nickname"];
                    aData.loginkind = 0;

                    //如果是管理人員 權限設定為"1"
                    if (Request.Form["loginkind"] != null && Request.Form["loginkind"] == "on")
                    {
                        aData.loginkind = 1;
                    }

                    //判斷重複帳號
                    if (CheckCanCreate(aData.account, out byte ioutkind) != 0)
                    {
                        Response.Write(ioutkind.ToString());
                        return;
                    }

                    aSqlStr = string.Format(aSqlStr, aData.account, aData.password, aData.nickname, aData.loginkind);

                    using (MySqlConnection aCon = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySQL_Admin"].ConnectionString))
                    {
                        aCon.Open();

                        using (MySqlCommand aCommand = new MySqlCommand(aSqlStr, aCon))
                        {
                            aCommand.ExecuteNonQuery();
                        }
                        aCon.Close();
                    }

                    Response.Write("0");
                }
                catch
                {
                    Response.Write("99");
                }
            }
            else if (aKind == 2)//修改玩家資料
            {
                aSqlStr = "UPDATE dokkansuper.accountdata SET account = '{0}', password = '{1}' ,nickname = '{2}' , loginkind = {3} WHERE (ID = {4});";
                try
                {
                    AccountData aData = new AccountData();

                    aData.ID = int.Parse(Request.Form["ID"]);
                    aData.account = Request.Form["account"];
                    aData.password = Request.Form["password"];
                    aData.nickname = Request.Form["nickname"];
                    aData.loginkind = 0;

                    //如果是管理人員 權限設定為"1"
                    if (Request.Form["loginkind"] != null && Request.Form["loginkind"] == "on")
                    {
                        aData.loginkind = 1;
                    }

                    if (Request.Form["loginkind"] == null)
                    {
                        aData.loginkind = 0;
                    }

                    //判斷重複帳號
                    if (CheckCanCreate(aData.account, out byte ioutkind, aData.ID) != 0)
                    {
                        Response.Write(ioutkind.ToString());
                        return;
                    }

                    aSqlStr = string.Format(aSqlStr, aData.account, aData.password, aData.nickname, aData.loginkind, aData.ID);

                    using (MySqlConnection aCon = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySQL_Admin"].ConnectionString))
                    {
                        aCon.Open();

                        using (MySqlCommand aCommand = new MySqlCommand(aSqlStr, aCon))
                        {
                            aCommand.ExecuteNonQuery();
                        }
                        aCon.Close();
                    }
                    Response.Write("0");
                }
                catch
                {
                    Response.Write("99");
                }
            }
            else if (aKind == 3)//移除玩家資料
            {
                aSqlStr = "DELETE FROM dokkansuper.accountdata WHERE (ID = {0});";

                try
                {
                    AccountData aData = new AccountData();
                    aData.ID = int.Parse(Request.QueryString["ID"]);
                    aSqlStr = string.Format(aSqlStr, aData.ID);

                    using (MySqlConnection aCon = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySQL_Admin"].ConnectionString))
                    {
                        aCon.Open();

                        using (MySqlCommand aCommand = new MySqlCommand(aSqlStr, aCon))
                        {
                            aCommand.ExecuteNonQuery();
                        }
                        aCon.Close();
                    }
                    Response.Write("0");
                }
                catch
                {
                    Response.Write("99");
                }
            }
        }

        //判斷是否有重複帳號的問題
        private byte CheckCanCreate(string iaccount, out byte ioutkind, int iID = 0)
        {
            string aSqlStr = "select account from dokkansuper.accountdata where account = '{0}'";

            if (iID != 0)
                aSqlStr += "And ID != " + iID;

            aSqlStr = string.Format(aSqlStr, iaccount);
            try
            {
                using (MySqlConnection aCon = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySQL_Admin"].ConnectionString))
                {
                    aCon.Open();

                    using (MySqlCommand aCommand = new MySqlCommand(aSqlStr, aCon))
                    {
                        MySqlDataReader aDR = aCommand.ExecuteReader();

                        if (aDR.HasRows)
                            return ioutkind = 1;
                        else
                            return ioutkind = 0;
                    }
                }
            }
            catch
            {
                return ioutkind = 99;
            }

        }
    }
}