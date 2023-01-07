using MassTransit;
using System;
using System.Threading.Tasks;

namespace SkyApm.Diagnostics.MassTransit.Observers
{
    public class MasstransitBusObserver : IBusObserver
    {
        private readonly IPublishObserver publishObserver;
        private readonly ISendObserver sendObserver;

        public MasstransitBusObserver(IPublishObserver publishObserver, ISendObserver sendObserver)
        {
            this.publishObserver = publishObserver;
            this.sendObserver = sendObserver;
        }


        public void CreateFaulted(Exception exception)
        {
            return;
        }

        public void PostCreate(IBus bus)
        {
            bus.ConnectPublishObserver(this.publishObserver);
            bus.ConnectSendObserver(this.sendObserver);
        }

        public Task PostStart(IBus bus, Task<BusReady> busReady)
        {
            return Task.CompletedTask;
        }

        public Task PostStop(IBus bus)
        {
            return Task.CompletedTask;
        }

        public Task PreStart(IBus bus)
        {
            return Task.CompletedTask;
        }

        public Task PreStop(IBus bus)
        {
            return Task.CompletedTask;
        }

        public Task StartFaulted(IBus bus, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task StopFaulted(IBus bus, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}
