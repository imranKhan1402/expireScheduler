using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace expireScheduler.EssentialClass
{
    public class OracleDatabaseManager : IDisposable
    {
        private String conString;
        private OracleConnection _cn;
        private DataTable _dt;
        private OracleCommand _cmd;
        private OracleDataReader _reader;
        private Dictionary<string, object> ParamList;

        private static OracleConnection _con;
        private static string connectionString;
        public OracleDatabaseManager()
        {
            conString = "User Id=BG;Password=bg;Data Source=172.17.4.199:9107/PRAN;pooling=false;";
        }
        public static OracleConnection OpenCon
        {
            get
            {
                if (_con == null)
                {
                    _con = new OracleConnection(connectionString);
                    _con.Open();
                    return _con;
                }
                else if (_con.State != ConnectionState.Open)
                {
                    _con.Open();
                    return _con;
                }
                else
                {
                    return _con;
                }
            }

        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cn != null)
                {
                    _cn.Dispose();
                    _cn = null;
                }
                if (_dt != null)
                {
                    _dt.Dispose();
                    _dt = null;
                }
                if (_cmd != null)
                {
                    _cmd.Dispose();
                    _cmd = null;
                }
                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }
            }
        }

        ~OracleDatabaseManager()
        {
            Dispose(false);
        }

        private static readonly Lazy<OracleDatabaseManager> VLazy = new Lazy<OracleDatabaseManager>(() => new OracleDatabaseManager());

        public static OracleDatabaseManager Instance
        {
            get { return VLazy.Value; }
        }

        public int ClearParameters()
        {
            if (ParamList != null)
            {
                if (ParamList.Count > 0)
                    ParamList.Clear();
            }
            if (_cmd != null)
            {
                if (_cmd.Parameters.Count > 0)
                    _cmd.Parameters.Clear();
            }
            return 1;
        }

        public void AddParameteres(string param, object value)
        {
            ParamList.Add(param, value);
        }

        public int ExecuteNonQuery(string strSql)
        {
            try
            {
                int intResult;
                using (_cn = new OracleConnection(conString))
                {
                    _cn.Open();
                    using (_cmd = new OracleCommand(strSql, _cn))
                    {
                        _cmd.CommandType = CommandType.Text;

                        intResult = _cmd.ExecuteNonQuery();
                    }
                }
                return intResult;
            }
            catch (Exception ex)
            {
                throw (new Exception(ex.Message));

            }
            finally
            {
                _cn.Dispose();
                _cn.Close();
                OracleConnection.ClearPool(_cn);
            }
        }
        public int ExecuteNonQuery(string strSql, bool isProcedure)
        {
            try
            {
                int intResult;
                using (_cn = new OracleConnection(conString))
                {
                    _cn.Open();
                    using (_cmd = new OracleCommand(strSql, _cn))
                    {
                        _cmd.CommandType = CommandType.StoredProcedure;

                        intResult = _cmd.ExecuteNonQuery();
                    }
                }
                return intResult;
            }
            catch (Exception)
            {
                return 0;

            }
            finally
            {
                _cn.Dispose();
                _cn.Close();
                OracleConnection.ClearPool(_cn);
            }
        }
        public DataTable ExecuteQuery(string strSql)
        {
            try
            {
                using (_cn = new OracleConnection(conString))
                {
                    _cn.Open();


                    using (_cmd = new OracleCommand(strSql, _cn))
                    {
                        _cmd.CommandType = CommandType.Text;

                        using (_reader = _cmd.ExecuteReader())
                        {
                            _dt = new DataTable();
                            try
                            {
                                _dt.Load(_reader);
                            }
                            catch (Exception ex)
                            {
                                string error = ex.Message;
                            }
                        }

                    }
                }

                return _dt;
            }
            catch (Exception ex)
            {
                throw ex;
                return null;
            }
            finally
            {
                _cn.Dispose();
                _cn.Close();
                OracleConnection.ClearPool(_cn);
            }
        }

        public string ExecuteScalar(string strSql)
        {
            string res = "";
            try
            {
                using (_cn = new OracleConnection(conString))
                {
                    _cn.Open();


                    using (_cmd = new OracleCommand(strSql, _cn))
                    {
                        _cmd.CommandType = CommandType.Text;
                        var firstColumn = _cmd.ExecuteScalar();
                        if (firstColumn != null)
                        {
                            res = firstColumn.ToString();
                        }
                    }
                }

                return res;
            }
            catch (Exception)
            {
                return ""; // throw (new Exception(ex.Message));
            }
            finally
            {
                _cn.Dispose();
                _cn.Close();
                OracleConnection.ClearPool(_cn);
            }
        }
        //public OracleDbType OracleDbType { get; set; }
        public string ExecuteScalar(string strSql, string outParameterName)
        {
            string res = "";
            try
            {
                using (_cn = new OracleConnection(conString))
                {
                    _cn.Open();

                    using (_cmd = new OracleCommand(strSql, _cn))
                    {
                        _cmd.CommandType = CommandType.Text;
                        OracleParameter outPm = new OracleParameter(outParameterName, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 50) { Direction = ParameterDirection.Output };
                        _cmd.Parameters.Add(outPm);
                        _cmd.ExecuteNonQuery();
                        res = outPm.Value.ToString();
                    }
                }

                return res;
            }
            catch (Exception)
            {
                return "";  // throw (new Exception(ex.Message));
            }
            finally
            {
                _cn.Dispose();
                _cn.Close();
                OracleConnection.ClearPool(_cn);
            }

        }



        internal OracleConnection GetDbConnection()
        {
            OracleConnection myConnection = new OracleConnection(connectionString);
            return myConnection;
        }

        public static DataTable StaticDT(string strSql)
        {
            try
            {
                DataTable dt = new DataTable();
                using (_con = new OracleConnection(connectionString))
                {
                    _con.Open();


                    using (OracleCommand cmds = new OracleCommand(strSql, _con))
                    {
                        cmds.CommandType = CommandType.Text;
                        using (OracleDataReader reader = cmds.ExecuteReader())
                        {
                            dt = new DataTable();
                            try
                            {
                                dt.Load(reader);
                            }
                            catch { }
                        }
                    }
                }

                return dt;
            }
            catch (Exception)
            {
                return null;  //throw (new Exception(ex.Message));
            }
            finally
            {
                _con.Dispose();
                _con.Close();
                OracleConnection.ClearPool(_con);
            }
        }
        public static DataSet GetDataSet(string strSql)
        {
            DataSet _ds = new DataSet();
            // conString = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
            try
            {
                using (_con = new OracleConnection(connectionString))
                {
                    _con.Open();


                    using (OracleCommand cmds = new OracleCommand(strSql, _con))
                    {
                        cmds.CommandType = CommandType.Text;

                        using (OracleDataAdapter sda = new OracleDataAdapter(cmds))
                        {
                            try
                            {
                                sda.Fill(_ds);
                            }
                            catch (Exception) { }
                        }
                    }
                }

                return _ds;
            }
            catch (Exception)
            {
                return null; //throw (new Exception(ex.Message));
            }
            finally
            {
                // ClearParameters();
            }
        }

    }
}
