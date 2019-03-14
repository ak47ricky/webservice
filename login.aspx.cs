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
            int aKind = 0;

            string aSqlStr = "select * from dokkansuper.accountdata";

            List<AccountData> aListData = new List<AccountData>();
            aListData.Clear();

            try
            {
                //判斷是前台還是後臺登陸
                aKind = int.Parse(Request.QueryString["kind"].ToString());

                if (aKind == 0)
                {
                    Response.Write("1");//登錄代碼錯誤
                    return;
                }

                //取得帳號密碼資料
                aAccount = Request.QueryString["loginname"];
                aPassword = Request.QueryString["password"];

                if (aKind == 1)//後臺判斷只需要判斷有權限的
                    aSqlStr = "select * from dokkansuper.accountdata where loginkind in ( 1 , " + Common._ManagerID + ")";

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

                            //先判斷帳號是否正確
                            if (aAccount != aDR["account"].ToString())
                                continue;//不正確直接判斷下一筆

                            //帳號正確後，判斷密碼是否正確
                            if (aPassword != aDR["password"].ToString())
                            {
                                //不正確就跳錯誤
                                Response.Write("3");//密碼錯誤
                                return;
                            }

                            aData.account = aDR["account"].ToString();
                            aData.password = aDR["password"].ToString();
                            aData.nickname = aDR["nickname"].ToString();
                            aData.loginkind = int.Parse(aDR["loginkind"].ToString());

                            //如果帳號密碼都符合塞一筆資料就跳出來。
                            aListData.Add(aData);
                            break;
                        }

                        aCon.Close();
                        aDR.Dispose();

                        //判斷是否有找到資料
                        if (aListData.Count == 0)
                        {
                            Response.Write("2");//帳號錯誤
                            return;
                        }

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