using System;

namespace NeveServer.Models
{
    /// <summary>
    /// 用户基类
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户id
        /// </summary>
        /// <value></value>
        public string? id{get;set;}

        /// <summary>
        /// 用户邮箱
        /// </summary>
        /// <value></value>
         public string? Email { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
        /// <value></value>
         public string?  Password { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        /// <value></value>
         public string?  Username { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        /// <value></value>
         public  DateTime? RegTime { get; set; }

         /// <summary>
         /// 最后登陆时间
         /// </summary>
         /// <value></value>
         public  DateTime? LastLogInTime { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        /// <value></value>
         public bool Status { get; set; }
        public User(){
            
        }
        public User(string? id, string? email, string? password, string? username, DateTime? regTime, DateTime? lastLogInTime, bool status)
        {
            this.id = id;
            Email = email;
            Password = password;
            Username = username;
            RegTime = regTime;
            LastLogInTime = lastLogInTime;
            Status = status;
        }
    }
}