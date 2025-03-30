using System;
using System.Reflection;
using System.Security.Claims;
using HomeControllerHUB.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HomeControllerHUB.Infra.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public Guid? UserId
        {
            get
            {
                Guid? userId = null;
                var id = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (!string.IsNullOrEmpty(id))
                {
                    userId = new Guid(id);
                }
                return userId;
            }
        }
        
        public string Login
        {
            get
            {
                var login = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
                
                if (string.IsNullOrEmpty(login))
                {
                    login = Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
                }
                return login;
            }
        }

        public Guid EstablishmentId
        {
            get
            {
                var id = _httpContextAccessor.HttpContext?.User?.FindFirstValue("EstablishmentId");
                if (string.IsNullOrEmpty(id))
                {
                    throw new InvalidOperationException("EstablishmentId claim not found");
                }
                
                return new Guid(id);
            }
        }
    }
}