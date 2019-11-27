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
        public DataBaseClient()
        {
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection("Server=192.168.0.218; Port=5432; User Id=postgres; Password=1234; Database=uchet_tool_pribor");

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "rf-qa-db1.zkpsl3bk22wuvfspx0uhovklae.bx.internal.cloudapp.net";
                
                builder.UserID = "camunda";
                builder.Password = "camunda";
                builder.InitialCatalog = "rfprojects_umbrella";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Debug.WriteLine("\nQuery data example:");
                    Debug.WriteLine("=========================================\n");

                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT TOP 20 id ");
                    sb.Append("FROM projects; ");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Debug.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                Debug.WriteLine(e.ToString());
            }
            
        }
    }
}
