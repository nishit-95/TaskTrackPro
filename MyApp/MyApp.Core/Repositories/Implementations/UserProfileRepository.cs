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
                        c_Role = reader["c_role"].ToString(),
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


        public async Task<int> ResetPassword(vm_UserProfile user, string currentPassword)
        {
            try
            {
                await _conn.OpenAsync(); // Open database connection asynchronously

                // Validate input parameters
                if (string.IsNullOrEmpty(user.c_Email) || string.IsNullOrEmpty(user.c_Password) || string.IsNullOrEmpty(currentPassword))
                {
                    throw new ArgumentException("Email, current password, and new password must be provided.");
                }

                // Check if the user with the provided email exists and verify the current password
                using (var checkCmd = new NpgsqlCommand("SELECT c_password FROM t_user WHERE c_email = @email", _conn))
                {
                    checkCmd.Parameters.AddWithValue("@email", user.c_Email);

                    using (var reader = await checkCmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            throw new ArgumentException("User with provided email does not exist.");
                        }

                        var storedPassword = reader.GetString(0);

                        if (currentPassword != storedPassword)
                        {
                            throw new ArgumentException("Incorrect current password.");
                        }
                    }
                }
                Console.WriteLine("current password in repo "+currentPassword);
                // Update the user's password
                using (var updateCmd = new NpgsqlCommand("UPDATE t_user SET c_password = @password WHERE c_email = @email AND c_password=@currentpassword" , _conn))
                {
                    updateCmd.Parameters.AddWithValue("@password", user.c_Password);
                    updateCmd.Parameters.AddWithValue("@currentpassword", currentPassword);
                    updateCmd.Parameters.AddWithValue("@email", user.c_Email);
                    return await updateCmd.ExecuteNonQueryAsync(); // Execute the update command asynchronously
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }
            finally
            {
                await _conn.CloseAsync(); // Close database connection asynchronously
            }
        }


        public async Task<int> Update(vm_UserProfile user)
        {
            try
            {
                await _conn.CloseAsync();
                await _conn.OpenAsync();
                string query = "UPDATE t_user SET c_username=@c_username, c_mobile=@c_mobile, c_address=@c_address, c_image=@c_image WHERE c_email=@c_email";

                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@c_username", user.c_UserName);
                cmd.Parameters.AddWithValue("@c_mobile", user.c_Mobile);
                cmd.Parameters.AddWithValue("@c_address", user.c_Address);
                cmd.Parameters.AddWithValue("@c_image", user.c_Image);
                cmd.Parameters.AddWithValue("@c_email", user.c_Email);
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine("------------> " + rowsAffected);
                Console.WriteLine("------------>address " + user.c_Address);
                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while updating the user profile: {ex.Message}");
                throw;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }



    }
}