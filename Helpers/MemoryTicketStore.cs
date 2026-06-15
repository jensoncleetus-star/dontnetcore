using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace QuickSoft.Web
{
    // Admin users have hundreds of roles; ASP.NET Core Identity serialises every role as a claim into
    // the auth cookie (~40 KB, chunked) -> heavy on every request and the cause of HTTP 431. A server-side
    // ITicketStore keeps the whole AuthenticationTicket in memory and puts ONLY a small key in the cookie.
    // NOTE: in-memory -> tickets are lost on app restart (users re-login) and it is single-server only;
    // for a multi-server production deployment swap IMemoryCache for an IDistributedCache (Redis/SQL).
    public class MemoryTicketStore : ITicketStore
    {
        private const string KeyPrefix = "auth-ticket-";
        private readonly IMemoryCache _cache;

        public MemoryTicketStore(IMemoryCache cache) { _cache = cache; }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = KeyPrefix + Guid.NewGuid().ToString("N");
            await RenewAsync(key, ticket);
            return key;
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var options = new MemoryCacheEntryOptions();
            var expiresUtc = ticket.Properties?.ExpiresUtc;
            if (expiresUtc.HasValue) options.SetAbsoluteExpiration(expiresUtc.Value);
            options.SetSlidingExpiration(TimeSpan.FromHours(8));
            _cache.Set(key, ticket, options);
            return Task.CompletedTask;
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            _cache.TryGetValue(key, out AuthenticationTicket ticket);
            return Task.FromResult(ticket);
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
