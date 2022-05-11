using System.Xml.Linq;
using System.Threading;
// handle database requests
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using NeveServer.Models;
using MySqlConnector;


namespace Neve.Server.Services
{

    public class DatabaseManager
    {
        private readonly string? _connectionString;
        private readonly User _user;
        private string? Id { get; set; }
        private string? Email { get; set; }
        private string? Password { get; set; }
        private string? Username { get; set; }
        private DateTime? RegTime { get; set; }
        private DateTime? LastLogInTime { get; set; }
        private bool? Status { get; set; }
        //playerdata
        public bool? LoginStatus { get; set; }
        public int? UseingDeckId { get; set; }
        public string? DeckList { get; set; }


        public MySqlConnection Connection { get; }


        public DatabaseManager(string connectionString, User user)
        {
            _connectionString = connectionString;
            Connection = new MySqlConnection(connectionString);
            _user = user;
            Id = _user.id;
            Email = _user.Email;
            Password = _user.Password;
            Username = _user.Username;
            RegTime = _user.RegTime;
            LastLogInTime = _user.LastLogInTime;
            Status = _user.Status;
        }
        public async Task InsertAsync()
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO `userdb`.`user` (`Id`, `Email`, `Password`, `Username`, `RegTime`, `LastLogInTime`, `Status`) VALUES (@Id,@Email,@Password,@Username,@RegTime,@LastLogInTime,@Status);";
            BindId(cmd);
            BindParams(cmd);
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task InsertPlayerAsync()
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO `userdb`.`player` (`Email`, `LoginStatus`, `UseingDeckId`, `DeckList`) VALUES (@Email, @LoginStatus, @UseingDeckId, @DeckList);";
            BindParams(cmd);
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task UpdateAsync()
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = @"UPDATE `userdb`.`user` SET `Password`=@Password,`LastLogInTime`=@LastLogInTime,`Status`=@Status WHERE `Email`=@Email;";
            BindParams(cmd);
            await cmd.ExecuteReaderAsync();
        }

        public async Task UpdataPlayerAsync()
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = @"UPDATE `userdb`.`player` SET `LoginStatus`=@LoginStatus WHERE `Email`=@Email;";
            BindParams(cmd);
            await cmd.ExecuteReaderAsync();
        }
        public async Task<bool> UpdataPlayerDackListAsync()
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = @"UPDATE `userdb`.`player` SET `UseingDeckId`=@UseingDeckId,`DeckList`=@DeckList WHERE `Email`=@Email;";
            BindParams(cmd);
            try{
                await cmd.ExecuteReaderAsync();
                return true;
            }
            catch{
                return false;
            }
            
        }

        public async Task<string[]?> GetDeckListAsync()
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = @"SELECT `UseingDeckId`,`DeckList` FROM userdb.player WHERE `Email`=@Email limit 1;";
            BindParams(cmd);
            DbDataReader reader = await cmd.ExecuteReaderAsync();
            string[] ReturnList = new string[2];
            while (await reader.ReadAsync())
            {
                if(await reader.IsDBNullAsync(0) || await reader.IsDBNullAsync(1))
                {
                    ReturnList[0] = "0";
                }
                else
                {
                    ReturnList[0] = reader.GetInt32(0).ToString();
                    ReturnList[1] = reader.GetString(1);
                    
                }
                
            }
            return ReturnList;
        }

        public async Task<User> GetPlayerAsync()
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM userdb.user WHERE `Email`=@Email limit 1;";
            BindParams(cmd);
            DbDataReader reader = await cmd.ExecuteReaderAsync();
            User newuser = new User();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    newuser.id = reader.GetString(0);
                    newuser.Email = reader.GetString(1);
                    newuser.Username = reader.GetString(3);
                    newuser.RegTime = reader.GetDateTime(4);
                    newuser.LastLogInTime = reader.GetDateTime(5);
                    newuser.Status = reader.GetBoolean(6);
                }
            }
            return newuser;
        }
        public async Task<bool> EmailExist()
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = @"SELECT Email FROM userdb.user WHERE `Email`=@Email limit 1;";
            BindParams(cmd);
            DbDataReader reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync();

        }

        public async Task<bool> CheckLoginAsync()
        {
            bool re = false;
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = @"SELECT LoginStatus FROM userdb.player WHERE `Email`=@Email limit 1;";
            BindParams(cmd);
            DbDataReader reader = await cmd.ExecuteReaderAsync();
            while(await reader.ReadAsync()){
               re =  reader.GetBoolean(0);
            }
            return re;
        }

        public async Task<bool> CheckPasswordAsync()
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = @"SELECT Password FROM userdb.user WHERE `Email`=@Email limit 1;";
            BindParams(cmd);
            DbDataReader reader = await cmd.ExecuteReaderAsync();
            try
            {
                while (await reader.ReadAsync())
                {
                    if (string.Equals(Password, reader.GetString(0)))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
            
        }

        private void BindId(MySqlCommand cmd)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "id",
                DbType = DbType.String,
                Value = Id,
            });
        }

        private void BindParams(MySqlCommand cmd)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@Email",
                DbType = DbType.String,
                Value = Email,
            });

            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@Password",
                DbType = DbType.String,
                Value = Password,
            });

            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@Username",
                DbType = DbType.String,
                Value = Username,
            });

            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@RegTime",
                DbType = DbType.DateTime,
                Value = RegTime,
            });

            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@LastLogInTime",
                DbType = DbType.DateTime,
                Value = LastLogInTime,
            });

            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@Status",
                DbType = DbType.Boolean,
                Value = Status,
            });

            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@LoginStatus",
                DbType = DbType.Boolean,
                Value = LoginStatus,
            });

            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@UseingDeckId",
                DbType = DbType.Int32,
                Value = UseingDeckId,
            });

            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@DeckList",
                DbType = DbType.String,
                Value = DeckList,
            });

        }
    }
}
