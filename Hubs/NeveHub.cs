using System.Text.RegularExpressions;
using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using NeveServer.Models;
using Neve.Server.Services;

namespace Neve.Server.Hubs
{
    public class NeveHub : Hub
    {
        private static IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        private string _connectionstring = Configuration["ConnectionStrings:Default"];
        public static Dictionary<string, string> GroupsManager = new Dictionary<string, string>();

        // Client Connection Debug
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("用户登陆：" + Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            bool isInRoom = false;
            Console.WriteLine("用户登出：" + Context.ConnectionId);
            foreach (string _ConnectionId in GroupsManager.Keys)
            {
                if (_ConnectionId == Context.ConnectionId)
                {
                    try
                    {
                        isInRoom = true;
                        Console.WriteLine("玩家因离线掉出房间");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("玩家离线操作失败");
                        Console.WriteLine(ex);
                    }
                    break;
                }
            }
            if (isInRoom)
            {
                DropFromRoom(GroupsManager[Context.ConnectionId]);
                GroupsManager.Remove(Context.ConnectionId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            string newMessage = user + "(" + DateTime.Now.ToString("hh:mm:ss") + "):" + message;
            await Clients.All.SendAsync("ReceiveMessage", newMessage);
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

        //GameRoom
        public async Task<bool> CreateRoom(string roomName, string roomPassword)
        {
            Random random = new Random();
            bool result = false;
            string id = random.Next(10000, 1000000).ToString();
            Room newRoom = new Room(id, roomName, roomPassword, 1, DateTime.Now, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newRoom);
            await databaseManager.Connection.OpenAsync();
            if (!await RoomNameExist(roomName))
            {
                try
                {
                    await databaseManager.RoomInsertAsync();
                    result = true;
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                    GroupsManager.Add(Context.ConnectionId, roomName);
                    await Clients.Group(roomName).SendAsync("Check", "创建成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return result;
        }

        public async Task<bool> AddToRoom(string roomName, string roomPassword)
        {
            bool result = false;
            Room newRoom = new Room(null, roomName, roomPassword, 0, null, true);
            DatabaseManager databaseManager1 = new DatabaseManager(_connectionstring, newRoom);
            await databaseManager1.Connection.OpenAsync();
            newRoom.RoomNumberOfPeople = await databaseManager1.GetRoomNumberOfPeople();
            newRoom.RoomNumberOfPeople++;
            await databaseManager1.Connection.CloseAsync();
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newRoom);
            await databaseManager.Connection.OpenAsync();
            if (await RoomNameExist(roomName) && await RoomPasswordCheck(roomName, roomPassword))
            {
                await databaseManager.RoomUpdateAsync();
                result = true;
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                GroupsManager.Add(Context.ConnectionId, roomName);
                await Clients.Group(roomName).SendAsync("Check", "加入成功");
            }

            return result;
        }

        public async Task<bool> RemoveFromRoom(string roomName)
        {
            bool result = false;
            Room newRoom = new Room(null, roomName, null, 0, null, true);
            DatabaseManager databaseManager1 = new DatabaseManager(_connectionstring, newRoom);
            await databaseManager1.Connection.OpenAsync();
            newRoom.RoomNumberOfPeople = await databaseManager1.GetRoomNumberOfPeople();
            newRoom.RoomNumberOfPeople--;
            await databaseManager1.Connection.CloseAsync();
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newRoom);
            await databaseManager.Connection.OpenAsync();
            if (await RoomNameExist(roomName))
            {
                if (newRoom.RoomNumberOfPeople == 0)
                {
                    try
                    {
                        await databaseManager.RemoveRoom();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else
                {
                    await databaseManager.RoomUpdateAsync();
                    result = true;
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
                    GroupsManager.Remove(Context.ConnectionId);
                    await Clients.Group(roomName).SendAsync("Check", "离开房间");
                }
            }

            return result;
        }

        public async void DropFromRoom(string roomName)
        {
            Room newRoom = new Room(null, roomName, null, 0, null, true);
            DatabaseManager databaseManager1 = new DatabaseManager(_connectionstring, newRoom);
            await databaseManager1.Connection.OpenAsync();
            newRoom.RoomNumberOfPeople = await databaseManager1.GetRoomNumberOfPeople();
            newRoom.RoomNumberOfPeople--;
            await databaseManager1.Connection.CloseAsync();
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newRoom);
            await databaseManager.Connection.OpenAsync();

            if (await RoomNameExist(roomName))
            {
                if (newRoom.RoomNumberOfPeople == 0)
                {
                    try
                    {
                        await databaseManager.RemoveRoom();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else
                {
                    await databaseManager.RoomUpdateAsync();
                    await Clients.Group(roomName).SendAsync("Check", "离开房间");
                }
            }
        }

        public async Task<List<Room>> GetRoomList()
        {
            Room room = new Room();
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, room);
            await databaseManager.Connection.OpenAsync();
            return await databaseManager.GetRoomListAsync();
        }

        public async Task<bool> RoomNameExist(string RoomName)
        {
            Room room = new Room(null, RoomName, null, null, null, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, room);
            await databaseManager.Connection.OpenAsync();
            return await databaseManager.RoomNameExist();
        }

        public async Task<bool> RoomPasswordCheck(string RoomName, string RoomPassword)
        {
            Room room = new Room(null, RoomName, RoomPassword, null, null, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, room);
            await databaseManager.Connection.OpenAsync();
            return await databaseManager.RoomPasswordCheck();
        }

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
