using System;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;

namespace ChiliCookoff2.App_Start.Providers
{
    public class RefreshTokenProvider : AuthenticationTokenProvider
    {
        public RefreshTokenProvider()
        {
            OnCreate = CreateRefreshToken;
            OnReceive = ReceiveRefreshToken;
        }

        private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
            context.SetToken(context.SerializeTicket());
        }

        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            context.DeserializeTicket(context.Token);
            context.Ticket.Properties.ExpiresUtc = DateTime.Now.AddYears(1);
        }
    }
}