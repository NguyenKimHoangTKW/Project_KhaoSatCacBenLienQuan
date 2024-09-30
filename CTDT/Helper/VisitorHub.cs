using CTDT.Models;
using Google;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CTDT.Helper
{
    public class VisitorHub : Hub
    {
        private static int visitorCount = 0;

        public override Task OnConnected()
        {
            visitorCount++;
            Clients.All.updateVisitorCount(visitorCount);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            visitorCount--;
            Clients.All.updateVisitorCount(visitorCount);
            return base.OnDisconnected(stopCalled);
        }
    }
}