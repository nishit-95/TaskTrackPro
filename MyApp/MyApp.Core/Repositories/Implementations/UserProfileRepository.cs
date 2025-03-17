using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MyApp.Core.Models;
using MyApp.Core.Repositories.Interfaces;
using Npgsql;

namespace MyApp.Core.Repositories.Implementations
{
    public class UserProfileRepository : IUserProfileInterface
    {

        private readonly NpgsqlConnection _conn;

        public UserProfileRepository(NpgsqlConnection conn)
        {
            _conn = conn;
        }
        public async Task<vm_UserProfile> GetOneUser(string email)
        {
            vm_UserProfile user = null;
            try
            {
                await _conn.CloseAsync();
                await _conn.OpenAsync();
                string query = "SELECT c_userid,c_username,c_email,c_password,c_mobile,c_gender,c_address,c_image,c_role FROM t_user WHERE c_email=@c_email";
                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@c_email", email);

                var reader = await cmd.ExecuteReaderAsync();

                if (reader.Read())
                {
                    user = new vm_UserProfile
                    {
                        c_UserId = Convert.ToInt32(reader["c_userid"]),
                        c_UserName = reader["c_username"].ToString(),
                        c_Email = reader["c_email"].ToString(),
                        c_Password = reader["c_password"].ToString(),
                        c_Mobile = reader["c_mobile"].ToString(),
                        c_Gender = reader["c_gender"].ToString(),
                        c_Address = reader["c_address"].ToString(),
                        c_Image = reader["c_image"].ToString(),
                        c_Role=reader["c_role"].ToString(),
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching employee with email {email}: {ex.Message}");
                throw;
            }
            finally
            {
                await _conn.CloseAsync();
            }
            return user;
        }

        public async Task<int> Update(vm_UserProfile user)
        {
                await using var con = new NpgsqlConnection("Server=cipg01; Port=5432; Database=Group_E_TaskTrack; User Id=postgres; Password=123456;");
            try
            {
                await con.CloseAsync();
                await con.OpenAsync();
                string query = "UPDATE t_user SET c_username=@c_username, c_mobile=@c_mobile, c_gender=@c_gender, c_address=@c_address, c_image=@c_image WHERE c_email=@c_email";

                var cmd = new NpgsqlCommand(query, con);
                cmd.Parameters.AddWithValue("@c_username", user.c_UserName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@c_mobile", user.c_Mobile ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@c_gender", user.c_Gender ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@c_address", user.c_Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@c_image", user.c_Image ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@c_email", user.c_Email ?? (object)DBNull.Value);
                await con.OpenAsync();
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine("------------> " + rowsAffected);
                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while updating the user profile: {ex.Message}");
                throw;
            }
            finally
            {
                await con.CloseAsync();
            }
        }

    
        // public async Task<int> Update(vm_UserProfile user)
        // {
        //     try{
        //         await _conn.CloseAsync();
        //         await _conn.OpenAsync();

        //         string query="UPDATE t_user SET c_username=@c_username, c_mobile=@c_mobile,c_gender=@c_gender,c_address=@c_address,c_image=@c_image";
        //         using var cmd=new NpgsqlCommand(query,_conn);
        //         cmd.Parameters.AddWithValue("@c_username",user.c_UserName);
        //         cmd.Parameters.AddWithValue("@c_mobile",user.c_Mobile);
        //         cmd.Parameters.AddWithValue("@c_gender",user.c_Gender);
        //         cmd.Parameters.AddWithValue("@c_address",user.c_Address);
        //         cmd.Parameters.AddWithValue("@c_image",user.c_Image);

        //         int rowsAffected=await cmd.ExecuteNonQueryAsync();
        //         Console.WriteLine("------------>"+rowsAffected);
        //         return rowsAffected;
        //     }
        //     catch(Exception ex){
        //         Console.WriteLine($"Error while updating the user profile: {ex.Message}");
        //         throw;
        //     }
        //     finally{
        //         await _conn.CloseAsync();
        //     }
        // }
    }
}