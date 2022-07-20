using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace expireScheduler.EssentialClass
{
    public class MailCenter
    {
        DataGather dg = new DataGather();
        private bool sendMail(string mailSubject, string mailBody, List<string> toUser, List<string> ccUser)
        {
            var information = dg.getEmailCredentials("ISMS");
            string Server_ID = information.Rows[0]["SERVER_ID"].ToString(); //"172.17.2.12";
            int Server_Port = Convert.ToInt32(information.Rows[0]["SERVER_PORT"]); //587;
            string senderDisplayName = information.Rows[0]["DISPLAYNAME"].ToString(); //"Bank Guarantee-Notification";
            string senderDisplayEmail = information.Rows[0]["DISPLAYEMAIL"].ToString(); //"BankGuarantee@Prangroup.com";
            string senderAccount = information.Rows[0]["SENDERACCOUNT"].ToString(); //"lsms@prangroup.com";
            //string senderCredential = "mis@W3lc0m3";
            string senderCredential = information.Rows[0]["SENDERPASSWORD"].ToString(); //"@dm!n@#sysmail";
            MailMessage email = new MailMessage();
            email.IsBodyHtml = true;
            email.From = new MailAddress(senderDisplayEmail, senderDisplayName);
            try
            {
                foreach (var item in toUser)
                {
                    email.To.Add(item);
                }
                foreach (var item in ccUser)
                {
                    email.CC.Add(item);
                }
            }
            catch (Exception)
            {
                return false;
            }

            //Common Id
            //List<Email_Id> EMAIL_LIST = new List<Email_Id>();
            //List<string> GR1 = new List<string>();
            //GR1.Add("GR0001");
            //EMAIL_LIST = RP.EmailListByGroupType(Group: GR1, TypeS: "ADMIN");
            //if (EMAIL_LIST.Count > 0)
            //{
            //    foreach (var item in EMAIL_LIST)
            //    {
            //        mail.CC.Add(item.SMTP);
            //    }
            //}
            //Common Id END
            //email.From = new MailAddress("noreply@prg.com", senderDisplayName);
            email.Subject = mailSubject;
            email.Body = mailBody;
            email.Body += "<br>Thanks & Regards,<br><b>CS | SW | MIS</b> <br/><br/>";
            email.Body += "<b style='color: red;font-style: italic;'>This is a system Generated Mail. Please do not reply to it.</b> <br/>";
            var mailSend = new System.Net.Mail.SmtpClient();
            {
                mailSend.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                mailSend.Host = Server_ID;
                mailSend.Port = Server_Port;
                mailSend.UseDefaultCredentials = false;
                mailSend.Credentials = new NetworkCredential(senderAccount, senderCredential);
                mailSend.EnableSsl = true;
                mailSend.Timeout = 600000;
            }
            try
            {
                certificateValidation();
                mailSend.Send(email);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        static void certificateValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                )
                {
                    return true;
                };
        }
        public async Task<bool> sendExpireMail(string BGID, string CMP, string BREQ_OUCMP, string BANK_NAME, string BREQ_BANKBRANCH, string BREQ_BGN, string BREQ_AMNT, string IDATE, string EDATE, string BREQ_NOTE, string REMAINING, string CREATE_USER, string APPROVE_USER)
        {
            string mailSubject = "BG ID ~ " + BGID + " is going to expire in "+ REMAINING +" days. ";
            string mailBody = string.Format(@"<table role='presentation' style='width:100%;border-collapse:collapse;border:0;border-spacing:0;background:#ffffff;'>
                                                <tr>
                                                <td align='center' style='padding:0;'>
                                                <table role='presentation' style='height:10px;width:602px;border-collapse:collapse;border:1px solid #cccccc;border-spacing:0;text-align:left;'>
                                                <tr>
                                                <td align='center' style='padding:40px 0 30px 0;background:#9400d3;'>
                                                <img src='http://hrms.prangroup.com:8283/bg/logo/bg.png' alt='' width='65' style='height:auto;display:block;' />
                                                </td>
                                                </tr>
                                                <tr>
                                                <td style='padding:36px 30px 42px 30px;'>
                                                <table role='presentation' style='width:100%;border-collapse:collapse;border:0;border-spacing:0;'>
                                                <tr>
                                                <td style='padding:0 0 36px 0;color:#153643;'>
                                                <h1 style='font-size:24px;margin:0 0 20px 0;font-family:Arial,sans-serif;'>Dear concern,</h1>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'> BG No. <strong>{0}</strong></p>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>&emsp;&emsp;&emsp;Company: <strong>{1}</strong></p>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>&emsp;&emsp;&emsp;Beneficiary Company: <strong>{2}</strong></p>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>&emsp;&emsp;&emsp;Bank: <strong>{3}</strong></p>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>&emsp;&emsp;&emsp;Branch: <strong>{4}</strong></p>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>&emsp;&emsp;&emsp;Transaction ID: <strong>{5}</strong></p>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>&emsp;&emsp;&emsp;Amount: <strong>{6}</strong></p>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>&emsp;&emsp;&emsp;Created By: <strong>{11}</strong></p>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>&emsp;&emsp;&emsp;Approved By: <strong>{12}</strong></p>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>&emsp;&emsp;&emsp;Issue Date: <strong>{7}</strong></p>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>&emsp;&emsp;&emsp;Expire Date: <strong>{8}</strong></p>
                                                <br/>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>This BG is going to expire in <strong>{10}</strong> Days.</p>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>And it is waiting for you to take action.</p>
                                                <br/>
                                                <p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>Application Link- <a href='http://hrms.prangroup.com:8283/bg/'>Bank Guarantee</a> </p>
                                                </td>
                                                </tr>
                                                </table>
                                                </td>
                                                </tr>
                                                </table>
                                                </td>
                                                </tr>
                                                </table>"
                                             , BGID, CMP, BREQ_OUCMP, BANK_NAME, BREQ_BANKBRANCH, BREQ_BGN, BREQ_AMNT, IDATE, EDATE, BREQ_NOTE, REMAINING, CREATE_USER, APPROVE_USER);//<p style='margin:0 0 12px 0;font-size:16px;line-height:24px;font-family:Arial,sans-serif;'>&emsp;&emsp;&emsp;Available Balance:  <strong>{7}</strong> </p>

            List<string> TO = new List<string>();
            List<string> CC = new List<string>();
            var emailList = await Task.Run(() => dg.emailListByMenu("202", BGID));
            var toList = emailList.Where(t => t.MAIL_TYPE == "TO");
            var ccList = emailList.Where(t => t.MAIL_TYPE == "CC");
            //if (emailList.Count < 1)
            //{
            //    return false;
            //}
            //if (toList.Count() < 1)
            //{
            //    foreach (var item in ccList)
            //    {
            //        TO.Add(item.USER_MAIL);
            //    }
            //}
            //else
            //{
            //    foreach (var item in toList)
            //    {
            //        TO.Add(item.USER_MAIL);
            //    }
            //    foreach (var item in ccList)
            //    {
            //        CC.Add(item.USER_MAIL);
            //    }
            //}
            TO.Add("mis90@prangroup.com");
            bool IsSend = sendMail(mailSubject, mailBody, TO, CC);
            if (IsSend)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
