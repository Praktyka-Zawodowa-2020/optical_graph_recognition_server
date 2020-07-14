using Api.Entities;
using Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        AuthenticateResponse RefreshToken(string token);
        bool RevokeToken(string token);
        IEnumerable<User> GetAll();
        User GetById(int id);
    }
}
