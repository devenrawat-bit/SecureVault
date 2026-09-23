using System;
using System.Collections.Generic;
using System.Text;

namespace Organization.Application.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event);
    }
}
