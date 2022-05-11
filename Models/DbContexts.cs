using System.Collections.Generic;
using NeveServer.Models;

namespace   NeveServer.Models.DbContexts
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public class DbContext
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        /// <typeparam name="User"></typeparam>
        /// <returns></returns>
        public static List<User> Users = new List<User>();

    }
}