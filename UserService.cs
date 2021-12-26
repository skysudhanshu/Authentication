using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Authentication.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Authentication
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        UserCreationStatus CreateUser(User user);
    }
    public class UserService : IUserService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // users hardcoded
        public static List<User> _users = new List<User>
        {
            new User { Id = 991, FirstName = "Admin", LastName = "User", Username = "admin", Password = "admin", Role = Role.Admin },
            new User { Id = 992, FirstName = "Jaiyesh", LastName = "V", Username = "Jaiyesh", Password = "admin", Role = Role.Admin },
            new User { Id = 101, FirstName = "Sudhanshu", LastName = "Kumar", Username = "sudhanshu", Password = "password", Role = Role.User },
            new User { Id = 102, FirstName = "Vinay", LastName = "K", Username = "vinay", Password = "password", Role = Role.User },
            new User { Id = 103, FirstName = "Swati", LastName = "Saini", Username = "swati", Password = "password", Role = Role.User },
            new User { Id = 104, FirstName = "Abhijit", LastName = "Kumar", Username = "abhijit", Password = "password", Role = Role.User },
            new User { Id = 105, FirstName = "Prem", LastName = "Nune", Username = "prem", Password = "password", Role = Role.User }
        };

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public User Authenticate(string username, string password)
        {
            var user = _users.SingleOrDefault(x => x.Username == username && x.Password == password);

            // return null if user not found
            if (user == null)
            {
                log.Error("User not Found");
                return null;
            }
              
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
            log.Info("token Generated");
            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _users;
        }

        public User GetById(int id)
        {
            var user = _users.FirstOrDefault(x => x.Id == id);
            return user;
        }

        public UserCreationStatus CreateUser(User user)
        {
            try
            {
                log.Info("Creating new User");
                var notAdminList = _users.Where(a => a.Role == Role.User);
                user.Id = notAdminList.Max(a => a.Id) + 1;
                _users.Add(new User { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName, Role = Role.User, Username = user.Username, Password = user.Password });
            }
            catch (Exception e)
            {
                log.Error(e+"in User Creation");
                Console.WriteLine(e);
            }
            var userCreationStatus = new UserCreationStatus { Id= user.Id, Message="User Created Successfully" };

            return userCreationStatus;
        }
    }
}