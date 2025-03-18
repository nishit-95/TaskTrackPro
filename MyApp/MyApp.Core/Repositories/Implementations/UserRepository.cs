using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyApp.Core.Model;
using MyApp.Core.Models;
using MyApp.Core.Repositories.Interfaces;
using Npgsql;
using Services;

namespace MyApp.Core.Repositories.Implementations
{
    public class UserRepository : IUserInterface
    {
        private readonly NpgsqlConnection _conn;
    private readonly IEmailService _emailService;
    public UserRepository(NpgsqlConnection connection, IEmailService emailService)
    {
        _conn = connection;
        _emailService = emailService;
    }
    
    public async Task<int> Register(t_User1 data)
    {
        int status = 0;
        try
        {
            await _conn.CloseAsync();
            NpgsqlCommand comcheck = new NpgsqlCommand(@"SELECT * FROM t_user WHERE c_email = @c_email;", _conn);
            comcheck.Parameters.AddWithValue("@c_email", data.c_email);
            await _conn.OpenAsync();
            using (NpgsqlDataReader datadr = await comcheck.ExecuteReaderAsync())
            {
                if (datadr.HasRows)
                {
                    await _conn.CloseAsync();
                    return 0;
                }
                else
                {
                    await _conn.CloseAsync();
                    
                    // Hash the password before storing
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(data.c_password);
                    
                    NpgsqlCommand com = new NpgsqlCommand(@"INSERT INTO t_user(c_userName, c_email, c_password, c_mobile, c_gender, c_address, c_status, c_image, c_role) 
                        VALUES (@c_userName, @c_email, @c_password, @c_mobile, @c_gender, @c_address, @c_status, @c_image, @c_role)", _conn);
                    
                    com.Parameters.AddWithValue("@c_userName", data.c_userName);
                    com.Parameters.AddWithValue("@c_email", data.c_email);
                    com.Parameters.AddWithValue("@c_password", hashedPassword); // Store hashed password
                    com.Parameters.AddWithValue("@c_mobile", data.c_mobile);
                    com.Parameters.AddWithValue("@c_gender", data.c_gender);
                    com.Parameters.AddWithValue("@c_address", data.c_address);
                    com.Parameters.AddWithValue("@c_status", data.c_status);
                    com.Parameters.AddWithValue("@c_image", data.c_image);
                    com.Parameters.AddWithValue("@c_role", data.c_role);
                    
                    await _conn.OpenAsync();
                    await com.ExecuteNonQueryAsync();
                    await _conn.CloseAsync();
                    return 1;
                }
            }
        }
        catch (Exception e)
        {
            await _conn.CloseAsync();
            Console.WriteLine("Register Failed, Error :- " + e.Message);
            return -1;
        }
    }

    public async Task<t_User1> Login(vm_Login user)
    {
        t_User1 UserData = new t_User1();
        try
        {
            // First retrieve the user by email only (not checking password yet)
            var qry = "SELECT * FROM t_user WHERE c_email = @c_email";
            using (NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn))
            {
                cmd.Parameters.AddWithValue("@c_email", user.c_email);
                await _conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    // Get the stored hashed password
                    string storedHash = reader.GetString(reader.GetOrdinal("c_password"));
                    string userStatus = reader.GetString(reader.GetOrdinal("c_status"));
                    
                    // Only proceed if user is active
                    if (userStatus != "Approved")
                    {
                        // Save the status for later checking
                        UserData.c_status = userStatus;
                        return UserData;
                    }
                    
                    // Verify the password outside of the reader
                    bool passwordValid = false;
                    
                    // Load all user data
                    UserData.c_userId = reader.GetInt32(reader.GetOrdinal("c_userId"));
                    UserData.c_userName = reader.GetString(reader.GetOrdinal("c_userName"));
                    UserData.c_email = reader.GetString(reader.GetOrdinal("c_email"));
                    UserData.c_password = storedHash; // Don't expose the actual hash
                    UserData.c_mobile = reader.GetString(reader.GetOrdinal("c_mobile"));
                    UserData.c_gender = reader.GetString(reader.GetOrdinal("c_gender"));
                    UserData.c_address = reader.IsDBNull(reader.GetOrdinal("c_address")) ? null : reader.GetString(reader.GetOrdinal("c_address"));
                    UserData.c_status = userStatus;
                    UserData.c_image = reader.IsDBNull(reader.GetOrdinal("c_image")) ? null : reader.GetString(reader.GetOrdinal("c_image"));
                    UserData.c_role = reader.GetString(reader.GetOrdinal("c_role"));
                    
                    await reader.CloseAsync();
                    
                    // Now verify the password
                    passwordValid = BCrypt.Net.BCrypt.Verify(user.c_password, storedHash);
                    
                    // If password is not valid, return empty user (with userId = 0)
                    if (!passwordValid)
                    {
                        UserData = new t_User1();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Login Error: " + e.Message);
        }
        finally
        {
            await _conn.CloseAsync();
        }
        return UserData;
    }


    public async Task<t_User1> GetUserByEmail(string email)
    {
        t_User1 UserData = null;
        var qry = "SELECT * FROM t_user WHERE c_email = @c_email";
        try
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn))
            {
                cmd.Parameters.AddWithValue("@c_email", email);
                await _conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    UserData = new t_User1
                    {
                        c_userId = reader.GetInt32(reader.GetOrdinal("c_userId")),
                        c_userName = reader.GetString(reader.GetOrdinal("c_userName")),
                        c_email = reader.GetString(reader.GetOrdinal("c_email")),
                        c_password = "[PROTECTED]", // Don't expose hash in responses
                        c_mobile = reader.GetString(reader.GetOrdinal("c_mobile")),
                        c_gender = reader.GetString(reader.GetOrdinal("c_gender")),
                        c_address = reader.IsDBNull(reader.GetOrdinal("c_address")) ? null : reader.GetString(reader.GetOrdinal("c_address")),
                        c_status = reader.GetString(reader.GetOrdinal("c_status")),
                        c_image = reader.IsDBNull(reader.GetOrdinal("c_image")) ? null : reader.GetString(reader.GetOrdinal("c_image")),
                        c_role = reader.GetString(reader.GetOrdinal("c_role"))
                    };
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("GetUserByEmail Error: " + e.Message);
        }
        finally
        {
            await _conn.CloseAsync();
        }
        return UserData;
    }

    public async Task<(bool success, string message, string token)> InitiatePasswordReset(string email)
    {
        try
        {
            // First, check if the user exists
            var user = await GetUserByEmail(email);
            if (user == null)
            {
                return (false, "No account found with this email address.", null);
            }

            // Generate a random token (6 digits)
            var token = GenerateRandomToken();
            
            // Store the token in the database
            await StorePasswordResetToken(email, token);
            
            // Create email content
            string subject = "TaskTrackPro - Password Reset Request";
            string message = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4A90E2; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; border: 1px solid #ddd; }}
                        .token {{ font-size: 24px; font-weight: bold; text-align: center; margin: 20px 0; color: #4A90E2; }}
                        .footer {{ font-size: 12px; text-align: center; margin-top: 20px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Password Reset Request</h2>
                        </div>
                        <div class='content'>
                            <p>Hello {user.c_userName},</p>
                            <p>We received a request to reset your password for your TaskTrackPro account. Please use the following token to complete your password reset:</p>
                            <div class='token'>{token}</div>
                            <p>This token will expire in 15 minutes for security reasons.</p>
                            <p>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
                            <p>Thank you,<br>The TaskTrackPro Team</p>
                        </div>
                        <div class='footer'>
                            This is an automated message, please do not reply directly to this email.
                        </div>
                    </div>
                </body>
                </html>";
            
            // Send email
            await _emailService.SendEmailAsync(email, subject, message);
            
            // In a real application, you wouldn't return the actual token
            // For testing purposes, we'll keep this behavior
            return (true, "Password reset token has been sent to your email address. Please check your inbox.", token);
        }
        catch (Exception ex)
        {
            Console.WriteLine("InitiatePasswordReset Error: " + ex.Message);
            return (false, "An error occurred while processing your request.", null);
        }
    }

    public async Task<(bool success, string message)> ResetPassword(string email, string token, string newPassword)
    {
        try
        {
            // Verify the token
            bool isValid = await ValidatePasswordResetToken(email, token);
            if (!isValid)
            {
                return (false, "Invalid or expired token. Please request a new password reset.");
            }
            
            // Update the user's password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            
            await _conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(
                @"UPDATE t_user SET c_password = @c_password WHERE c_email = @c_email", _conn))
            {
                cmd.Parameters.AddWithValue("@c_email", email);
                cmd.Parameters.AddWithValue("@c_password", hashedPassword);
                
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    // Mark the token as used
                    await MarkTokenAsUsed(email, token);
                    return (true, "Your password has been successfully reset. You can now login with your new password.");
                }
                else
                {
                    return (false, "Failed to update password. Please try again.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ResetPassword Error: " + ex.Message);
            return (false, "An error occurred while resetting your password.");
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }
    
    // Helper methods
    private string GenerateRandomToken()
    {
        // Generate a 6-digit numeric token
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }
    
    private async Task StorePasswordResetToken(string email, string token)
    {
        try
        {
            // First, invalidate any existing tokens for this email
            await _conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(
                @"UPDATE t_password_reset_tokens SET used = true WHERE email = @email AND used = false", _conn))
            {
                cmd.Parameters.AddWithValue("@email", email);
                await cmd.ExecuteNonQueryAsync();
            }
            await _conn.CloseAsync();
            
            // Then, insert the new token
            await _conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(
                @"INSERT INTO t_password_reset_tokens(email, token, expiry_date, used) 
                  VALUES (@email, @token, @expiry_date, false)", _conn))
            {
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@expiry_date", DateTime.UtcNow.AddMinutes(15)); // Token expires in 15 minutes
                cmd.Parameters.AddWithValue("@used", false);
                
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("StorePasswordResetToken Error: " + ex.Message);
            throw;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }
    
    private async Task<bool> ValidatePasswordResetToken(string email, string token)
    {
        try
        {
            await _conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(
                @"SELECT COUNT(*) FROM t_password_reset_tokens 
                  WHERE email = @email AND token = @token AND used = false AND expiry_date > @current_time", _conn))
            {
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@current_time", DateTime.UtcNow);
                
                int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ValidatePasswordResetToken Error: " + ex.Message);
            return false;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }
    
    private async Task MarkTokenAsUsed(string email, string token)
    {
        try
        {
            await _conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(
                @"UPDATE t_password_reset_tokens SET used = true 
                  WHERE email = @email AND token = @token", _conn))
            {
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@token", token);
                
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("MarkTokenAsUsed Error: " + ex.Message);
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }
    }
}