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
        public static Dictionary<string, string> LogManager = new Dictionary<string, string>();

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
            DisconnectLogout(Context.ConnectionId);
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
                await databaseManager.Connection.CloseAsync();
                await databaseManager2.Connection.CloseAsync();
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
                    await databaseManager.Connection.CloseAsync();
                    return true;
                }
                else
                {
                    Console.WriteLine("没有重复注册");
                    await databaseManager.Connection.CloseAsync();
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
            await databaseManager2.UpdatePlayerAsync();
            await databaseManager.Connection.CloseAsync();
            await databaseManager2.Connection.CloseAsync();

            LogManager.Add(Context.ConnectionId,email);
        }

        public async Task Logout(string email)
        {
            var newUser = new User(null, email, null, null, null, null, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newUser);
            await databaseManager.Connection.OpenAsync();
            databaseManager.LoginStatus = false;
            await databaseManager.UpdatePlayerAsync();
            await databaseManager.Connection.CloseAsync();
        }

        public async void DisconnectLogout(string Id){
            await Logout(LogManager[Id]);
            LogManager.Remove(Id);
        }

        public async Task<bool> CheckPassword(string email, string password)
        {
            var newUser = new User(null, email, password, null, null, null, true);
            DatabaseManager databaseManager = new DatabaseManager(_connectionstring, newUser);
            await databaseManager.Connection.OpenAsync();
            if (await databaseManager.CheckPasswordAsync())
            {
                Console.WriteLine("密码正确");
                await databaseManager.Connection.CloseAsync();
                return true;
            }
            else
            {
                Console.WriteLine("密码错误");
                await databaseManager.Connection.CloseAsync();
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
            await databaseManager.Connection.CloseAsync();
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
                    await databaseManager.Connection.CloseAsync();
                    return true;
                }
                else
                {
                    Console.WriteLine("储存失败");
                    await databaseManager.Connection.CloseAsync();
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //GameRoom
        public async Task<bool> CreateRoom(string roomName, string roomPassword, string username)
        {
            Random random = new Random();
            bool result = false;
            string id = random.Next(10000, 1000000).ToString();
            Room newRoom = new Room(id, roomName, roomPassword, 1, DateTime.Now, true);
            newRoom.RoomMemberList.Add(username);
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
            await databaseManager.Connection.CloseAsync();
            return result;
        }

        public async Task<bool> AddToRoom(string roomName, string roomPassword, string username)
        {
            bool result = false;
            List<Room> RoomList = await GetRoomList();
            foreach (var room in RoomList)
            {
                if (room.RoomName == roomName)
                {
                    room.RoomMemberList.Add(username);
                    room.RoomNumberOfPeople++;
                    DatabaseManager databaseManager = new DatabaseManager(_connectionstring, room);
                    await databaseManager.Connection.OpenAsync();
                    if (await RoomPasswordCheck(roomName,roomPassword))
                    {
                        await databaseManager.RoomUpdateAsync();
                        result = true;
                        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                        GroupsManager.Add(Context.ConnectionId, roomName);
                        await Clients.Group(roomName).SendAsync("Check", "加入成功");
                    }
                    await databaseManager.Connection.CloseAsync();

                    break;
                }
            }

            return result;
        }

        public async Task<bool> RemoveFromRoom(string roomName, string username)
        {
            bool result = false;
            List<Room> RoomList = await GetRoomList();
            foreach (var room in RoomList)
            {
                if (room.RoomName == roomName)
                {
                    room.RoomMemberList.Remove(username);
                    room.RoomNumberOfPeople--;
                    DatabaseManager databaseManager = new DatabaseManager(_connectionstring, room);
                    await databaseManager.Connection.OpenAsync();
                    if (await RoomNameExist(roomName))
                    {
                        if (room.RoomNumberOfPeople == 0)
                        {
                            try
                            {
                                await databaseManager.RemoveRoom();
                                result = true;
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
                    await databaseManager.Connection.CloseAsync();
                    break;
                }
            }

            return result;
        }

        public async void DropFromRoom(string roomName)
        {
            List<Room> RoomList = await GetRoomList();
            foreach (var room in RoomList)
            {
                if (room.RoomName == roomName)
                {
                    room.RoomNumberOfPeople--;
                    DatabaseManager databaseManager = new DatabaseManager(_connectionstring, room);
                    await databaseManager.Connection.OpenAsync();
                    if (await RoomNameExist(roomName))
                    {
                        if (room.RoomNumberOfPeople == 0)
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
                        }
                    }
                    await databaseManager.Connection.CloseAsync();
                    break;
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
        public async Task<bool> RoomPasswordCheck(string RoomName,string RoomPassword){
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
