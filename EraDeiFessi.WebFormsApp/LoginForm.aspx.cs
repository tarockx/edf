using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Xml;
using System.IO;
using EraDeiFessi.WebFormsApp.Helpers;

namespace EraDeiFessi.WebFormsApp
{
    public partial class LoginForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (!IsCallback)
            //{
            //    var FormsAuthCookie = Response.Cookies[FormsAuthentication.FormsCookieName];
            //    var ExistingTicket = FormsAuthentication.Decrypt(FormsAuthCookie.Value);
            //    if (!ExistingTicket.Expired)
            //        Response.Redirect("SearchForm.aspx");
            //}
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                //Redirect to Default page
                Response.Redirect("SearchForm.aspx");
            }
        }

        protected void Logon_Click(object sender, EventArgs e)
        {
            string user = ((this as Control).FindControlRecursive("txtUser") as HtmlInputControl).Value;
            string password = ((this as Control).FindControlRecursive("txtPassword") as HtmlInputControl).Value;
            bool remember = chkRemember.Checked;

            if (CheckCredentials(user, password))
            {
                FormsAuthentication.RedirectFromLoginPage(user, remember);
            }
            else
            {
                lblWrongCredentials.Visible = true;
                lblHtmlContentTitle.Visible = false;
            }
        }

        private bool CheckCredentials(string u, string p){
            try
            {
                string file = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "App_Data") + "\\repo.xml";
                XmlDocument doc = new XmlDocument();
                XmlReader reader = XmlReader.Create(new System.IO.StreamReader(file));
                doc.Load(reader);
                var res = doc.SelectSingleNode("//user[@username='" + u + "' and @password='" + p + "']");

                reader.Close();

                return res != null;
            }
            catch (Exception)
            {
                return false;
            }

            

            //ConnectionStringSettings mySetting = ConfigurationManager.ConnectionStrings["depot"];
            //if (mySetting == null || string.IsNullOrEmpty(mySetting.ConnectionString))
            //    throw new Exception("Fatal error: missing connecting string in web.config file");
            //string conString = mySetting.ConnectionString;
            //string query = "SELECT * FROM Users WHERE username = '" + u +"' AND password = '" + p + "';";

            //DataTable results = new DataTable();
            //using (SqlConnection conn = new SqlConnection(conString))
            //    using (SqlCommand command = new SqlCommand(query, conn))
            //        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
            //            dataAdapter.Fill(results);

            //return results != null && results.Rows.Count > 0;
        }
    }
}