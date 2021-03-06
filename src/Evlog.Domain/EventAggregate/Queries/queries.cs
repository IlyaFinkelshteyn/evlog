
using System.Collections.Generic;
using Evlog.Domain.EventAggregate;

namespace Evlog.Domain.EventAggregate.Queries
{
    public interface IAllEventsQuery : IEvlogQuery<IList<EventPost>> { }
    public interface IUpcomingEventsQuery : IEvlogQuery<IList<EventPost>> { }
    public interface IPastEventsQuery : IEvlogQuery<IList<EventPost>> { }

}
