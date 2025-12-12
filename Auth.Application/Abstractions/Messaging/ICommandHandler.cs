
using Auth.Application.Abstractions.Messaging.Application.Abstractions.Messaging;
using Auth.Domain.Primitives;
using MediatR;

namespace Auth.Application.Abstractions.Messaging
{
    public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
 where TCommand : ICommand
    { }

    public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
    { }
}
