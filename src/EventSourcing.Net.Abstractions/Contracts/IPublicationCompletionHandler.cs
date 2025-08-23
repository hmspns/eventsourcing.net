namespace EventSourcing.Net.Abstractions.Contracts;

using System.Threading.Tasks;

public interface IPublicationCompletionHandler
{
    Task PublicationDone();
}