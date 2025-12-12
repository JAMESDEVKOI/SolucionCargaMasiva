using Auth.Domain.Primitives;
using MediatR;

namespace Auth.Application.Abstractions.Messaging
{
    namespace Application.Abstractions.Messaging
    {
        public interface ICommand : IRequest<Result>, IBaseCommand { }

        public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand { }

        public interface IBaseCommand { }
    }
}
