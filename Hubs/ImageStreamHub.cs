using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreRealtimeBinary.Models.Services.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCoreRealtimeBinary.Hubs
{
    public class ImageStreamHub : Hub<IImageStreamClient>
    {
        private readonly ITaskStartStop taskStartStop;

        public ImageStreamHub(ITaskStartStop taskStartStop)
        {
            this.taskStartStop = taskStartStop;
        }

        public Task Start()
        {
            var (success, currentStatus) = taskStartStop.Start();
            return NotifyClientsIfNeeded(success, currentStatus);
        }

        public Task Stop()
        {
            var (success, currentStatus) = taskStartStop.Stop();
            return NotifyClientsIfNeeded(success, currentStatus);
        }

        private static int connectedClientCount = 0;
        public override async Task OnConnectedAsync()
        {
            Interlocked.Increment(ref connectedClientCount);
            await Clients.Caller.NotifyStatusChange(taskStartStop.GetCurrentStatus());
        }

        public override Task OnDisconnectedAsync(Exception exc)
        {
            int newValue = Interlocked.Decrement(ref connectedClientCount);
            if (newValue == 0)
            {
                //Stop automatico se non ci sono più client in ascolto
                Stop();
            }
            return Task.CompletedTask;
        }

        private Task NotifyClientsIfNeeded(bool success, string currentStatus)
        {
            if (!success)
            {
                return Task.CompletedTask;
            }
            return Clients.All.NotifyStatusChange(currentStatus);
        }
    }
}