using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace expireScheduler.EssentialClass
{
    public class DataGather
    {
        private OracleDatabaseManager ODM = new OracleDatabaseManager();
        public DataTable getEmailCredentials(string name)
        {
            string qry = @"SELECT SERVER_ID,SERVER_PORT, SENDERACCOUNT, SENDERPASSWORD,DISPLAYNAME, DISPLAYEMAIL FROM BG.EMAIL_CREDENTIALS WHERE MNAME = '" + name + "'";
            return ODM.ExecuteQuery(qry);
        }
        public List<emailID> emailListByMenu(string menu, string bg)
        {
            string Sql;
            if (bg.Substring(0, 10) == "BGPR2022XX")
            {
                Sql = string.Format(@"SELECT U.USER_TEXT, U.USER_MAIL, MAIL_TYPE
                                          FROM BG_EMAIL  U
                                               INNER JOIN BG_USERS_MENUC P ON P.USER_TEXT = U.USER_TEXT AND P.ACT = 1
                                         WHERE P.CMNU_TEXT = '" + menu + @"' AND U.ACT = 1
                                         UNION ALL
                                        SELECT E.USER_TEXT, E.USER_MAIL, 'TO' MAIL_TYPE
                                          FROM BG_BREQ  BG
                                               INNER JOIN BG_BREQ_TICK TICK ON BG.BREQ_OID = TICK.FILE_BREQ
                                               INNER JOIN BG_EMAIL E ON TICK.CREATE_USER = E.USER_TEXT
                                         WHERE BG.BREQ_OID = '" + bg + "'");
            }
            else
            {
                Sql = string.Format(@"select E.USER_TEXT, E.USER_MAIL, 'TO' MAIL_TYPE
                                            from BG_BREQ BG 
                                            INNER JOIN BG_BREQ_TICK TICK ON BG.BREQ_OID = TICK.FILE_BREQ
                                            INNER join BG_MUSERS U on TICK.CREATE_USER = U.USER_TEXT
                                            inner join BG_department D on U.USER_DEPT = D.OID
                                            INNER join BG_MUSERS DU on D.OID = DU.USER_DEPT
                                            INNER JOIN BG_USERS_MENUC P ON DU.USER_TEXT = P.USER_TEXT AND P.ACT = 1
                                            INNER JOIN BG_EMAIL E ON P.USER_TEXT = E.USER_TEXT
                                            WHERE BG.BREQ_OID = '" + bg + "' and P.CMNU_TEXT = '" + menu + @"'
                                            UNION ALL
                                            SELECT E.USER_TEXT, E.USER_MAIL, 'CC' MAIL_TYPE
                                              FROM BG_BREQ  BG
                                                   INNER JOIN BG_BREQ_TICK TICK ON BG.BREQ_OID = TICK.FILE_BREQ
                                                   INNER JOIN BG_EMAIL E ON TICK.CREATE_USER = E.USER_TEXT
                                             WHERE BG.BREQ_OID = '" + bg + "'");
            }

            var data = ODM.ExecuteQuery(Sql);
            List<emailID> mailList = ConvertDataTable<emailID>(data);
            return mailList;
        }
        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
        public class emailID
        {
            public string USER_TEXT { get; set; }
            public string USER_MAIL { get; set; }
            public string MAIL_TYPE { get; set; }
        }
    }
}
