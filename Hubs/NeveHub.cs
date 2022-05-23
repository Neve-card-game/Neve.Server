using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using NeveServer.Models;
using Neve.Server.Services;

namespace Neve.Server.Hubs
{
    public class NeveHub : Hub<IClientProxy>
    {
        private static IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        private string _connectionstring = Configuration["ConnectionStrings:Default"];

        // Client Connection Debug
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("用户登陆：" + Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("用户登出：" + Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine("接收");
            await Clients.All.SendAsync("ReceiveMessage", user, message);
            Console.WriteLine("发送");
        }

        // User Information
        public async Task<bool> Register(string Email, string passWord, string userName)
        {
            var id = System.Guid.NewGuid().ToString();
            var RegTime = DateTime.Now;
            var LastLogInTime = DateTime.Now;
            var newUser = new User(id, Email, passWord, userName, RegTime, LastLogInTime, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newUser);
            DatabaseManager databaseManager2 = new DatabaseManager(_connectionstring, newUser);
            databaseManager2.LoginStatus = false;
            await databaseManager.Connection.OpenAsync();
            await databaseManager2.Connection.OpenAsync();
            try
            {
                await databaseManager.InsertAsync();
                await databaseManager2.InsertPlayerAsync();
            }
            catch (System.Exception)
            {
                Console.WriteLine("注册失败");
            }

            return true;
        }

        public async Task<bool> Emailexist(string email)
        {
            var newUser = new User(null, email, null, null, null, null, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newUser);
            await databaseManager.Connection.OpenAsync();
            try
            {
                if (await databaseManager.EmailExist())
                {
                    Console.WriteLine("重复注册");
                    return true;
                }
                else
                {
                    Console.WriteLine("没有重复注册");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public async Task Login(string email, string password)
        {
            var LastLogInTime = DateTime.Now;
            var newUser = new User(null, email, password, null, null, LastLogInTime, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newUser);
            await databaseManager.Connection.OpenAsync();
            DatabaseManager databaseManager2 = new DatabaseManager(_connectionstring, newUser);
            await databaseManager2.Connection.OpenAsync();
            databaseManager2.LoginStatus = true;
            await databaseManager.UpdateAsync();
            await databaseManager2.UpdataPlayerAsync();
        }

        public async Task Logout(string email)
        {
            var newUser = new User(null, email, null, null, null, null, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newUser);
            await databaseManager.Connection.OpenAsync();
            databaseManager.LoginStatus = false;
            await databaseManager.UpdataPlayerAsync();
        }

        public async Task<bool> CheckPassword(string email, string password)
        {
            var newUser = new User(null, email, password, null, null, null, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newUser);
            await databaseManager.Connection.OpenAsync();
            if (await databaseManager.CheckPasswordAsync())
            {
                Console.WriteLine("密码正确");
                return true;
            }
            else
            {
                Console.WriteLine("密码错误");
                return false;
            }
        }

        public async Task<bool> CheckLogin(string email)
        {
            var newUser = new User(null, email, null, null, null, null, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newUser);
            await databaseManager.Connection.OpenAsync();
            return await databaseManager.CheckLoginAsync();
        }

        public async Task<User> GetPlayerMessage(string email)
        {
            var newUser = new User(null, email, null, null, null, null, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newUser);
            await databaseManager.Connection.OpenAsync();
            List<string> returnMessage = new List<string>();
            newUser = await databaseManager.GetPlayerAsync();
            return newUser;
        }

        //DeckCollection
        public async Task<string[]?> LoadDeckList(string email)
        {
            var newUser = new User(null, email, null, null, null, null, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newUser);
            if (await CheckLogin(email))
            {
                await databaseManager.Connection.OpenAsync();
                return await databaseManager.GetDeckListAsync();
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> SaveDeckList(string email, string decklist, int deckid)
        {
            var newUser = new User(null, email, null, null, null, null, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newUser);
            if (await CheckLogin(email))
            {
                await databaseManager.Connection.OpenAsync();
                databaseManager.DeckList = decklist;
                databaseManager.UseingDeckId = deckid;
                if (await databaseManager.UpdataPlayerDackListAsync())
                {
                    Console.WriteLine("储存成功");
                    return true;
                }
                else
                {
                    Console.WriteLine("储存失败");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //RoomChat

        // Matchmaking
        public async Task StartMatch(string userName)
        {
            await Task.CompletedTask;
        }

        public async Task StopMatch(string userName)
        {
            await Task.CompletedTask;
        }

        public async Task StartViewMatch(string userName, string matchId)
        {
            await Task.CompletedTask;
        }

        public async Task StopViewMatch(string userName, string matchId)
        {
            await Task.CompletedTask;
        }

        // Game
    }
}
