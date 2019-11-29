using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Clients
{
    public class DataBaseClient
         
    {
        private NpgsqlConnection _conn;
        private NpgsqlCommand _command;
        private NpgsqlDataReader _dr;

        public DataBaseClient()
        {   
        }

        private void _dbConnect()
        {
            try
            {
                _conn = new NpgsqlConnection("Server=rf-qa-db1.zkpsl3bk22wuvfspx0uhovklae.bx.internal.cloudapp.net;" +
               "Port=5432;" +
               "User Id=camunda;" +
               "Password=camunda;" +
               "Database=rfprojects_umbrella;");
                _conn.Open();
            }
            catch (SqlException e)
            {
                Debug.WriteLine(e.ToString());
            }

        }

        private bool isExistRowDB(string id)
        {
            bool isExist = false;
            try
            {
                string _selectRequest = String.Format(@"SELECT id FROM projects WHERE id='{0}'", id);
                // Connect to a PostgreSQL database
                _dbConnect();
                // Define a query returning a single row result set
                _command = new NpgsqlCommand(_selectRequest, _conn);
                // Execute the query and obtain a result set
                _dr = _command.ExecuteReader();
                if (!_dr.Read())
                    isExist = false;
                else
                    isExist = true;
                _conn.Close();
            } catch (SqlException e)
            {
                Debug.WriteLine(e.ToString());
            }
            return isExist;
        }
        public void DeleteProjectInDB(string id)
        {
            try
            {
                string _deleteRequest = String.Format(@"DELETE FROM projects WHERE id='{0}'", id);
                // Connect to a PostgreSQL database
                _dbConnect();
                // Define a query returning a single row result set
                _command = new NpgsqlCommand(_deleteRequest, _conn);
                // Execute the query and obtain a result set
                _dr = _command.ExecuteReader();
                if (!isExistRowDB(id))
                    Debug.WriteLine("Project is deleted in database");
                else
                    Debug.WriteLine("=>"+ _dr.Read().ToString());
                _conn.Close();
            }
            catch (SqlException e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
