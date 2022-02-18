using Microsoft.AspNetCore.SignalR;

namespace Neve.Server.Hubs
{
    public class NeveHub : Hub
    {
        // User Information
        public async Task<bool> Register(string userName, string password)
        {
            await Task.CompletedTask;
            return true;
        }

        public async Task Login(string userName, string password)
        {
            await Task.CompletedTask;
        }

        public async Task Logout()
        {
            await Task.CompletedTask;
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
