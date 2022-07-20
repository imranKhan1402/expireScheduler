using expireScheduler.EssentialClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace expireScheduler
{
    public partial class frmES : Form
    {
        MailCenter MC = new MailCenter();
        OracleDatabaseManager odm = new OracleDatabaseManager();
        public frmES()
        {
            InitializeComponent();
        }
        private async void timer1_Tick(object sender, EventArgs e)
        {
            timerES.Stop();
            string qry;
            StringBuilder sb = new StringBuilder();
            try
            {
                qry = @"SELECT 
                                BGID, CMP, BREQ_OUCMP, 
                                   BANK_NAME, BREQ_BANKBRANCH, BREQ_BGN, 
                                   BREQ_AMNT, IDATE, EDATE, 
                                   BREQ_NOTE, REMAINING, CREATE_USER, 
                                   APPROVE_USER
                                FROM BG.BG_ES where ISSEND = '1'";
                DataTable data = await getESDataAsync(qry);
                if (data.Rows.Count>0) 
                {
                    sb.AppendFormat(DateTime.Now.ToString() + "{0}", Environment.NewLine);
                    //tbxES.AppendText(sb.ToString());
                    foreach (DataRow row in data.Rows)
                    {
                        string BGID = row["BGID"].ToString();
                        string CMP = row["CMP"].ToString();
                        string BREQ_OUCMP = row["BREQ_OUCMP"].ToString();
                        string BANK_NAME = row["BANK_NAME"].ToString();
                        string BREQ_BANKBRANCH = row["BREQ_BANKBRANCH"].ToString();
                        string BREQ_BGN = row["BREQ_BGN"].ToString();
                        string BREQ_AMNT = row["BREQ_AMNT"].ToString();
                        string IDATE = row["IDATE"].ToString();
                        string EDATE = row["EDATE"].ToString();
                        string BREQ_NOTE = row["BREQ_NOTE"].ToString();
                        string REMAINING = row["REMAINING"].ToString();
                        string CREATE_USER = row["CREATE_USER"].ToString();
                        string APPROVE_USER = row["APPROVE_USER"].ToString();

                        qry = @"UPDATE BG.BG_ES
                               SET ESDATE = TO_DATE('" + DateTime.Now.ToString("dd/MM/yyyy") + @"', 'DD/MM/YYYY'), ISSEND = '0'
                             WHERE BGID = '" + BGID + "' AND REMAINING = '" + REMAINING + "'";
                        odm.ExecuteNonQuery(qry);
                        bool MailOK = await MC.sendExpireMail(BGID, CMP, BREQ_OUCMP, BANK_NAME, BREQ_BANKBRANCH, BREQ_BGN, BREQ_AMNT, IDATE, EDATE, BREQ_NOTE, REMAINING, CREATE_USER, APPROVE_USER);
                        if (MailOK == true)
                        {
                            sb.AppendFormat("Mail Send For BG ~ {1} Expire in {2} Days.{0} ", Environment.NewLine, BGID, REMAINING);
                            //tbxES.AppendText(sb.ToString());
                        }
                        else
                        {
                            sb.AppendFormat("Mail Send Failed For BG ~ {1} Expire in {2} Days.{0} ", Environment.NewLine, BGID, REMAINING);
                            //tbxES.AppendText(sb.ToString());
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendFormat(ex.ToString() + "{0}", Environment.NewLine);
                //tbxES.AppendText(sb.ToString());
            }
            tbxES.AppendText(sb.ToString());
            timerES.Start();
        }

        private async Task<DataTable> getESDataAsync(string qry)
        {
            return await Task.Run(() => odm.ExecuteQuery(qry));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            timerES.Start();
            timerES.Interval = 500;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            timerES.Stop();
        }

        private void frmES_Load(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
        }
    }
}
