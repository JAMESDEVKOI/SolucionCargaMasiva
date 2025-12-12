using System.Data;

namespace Auth.Application.Abstractions.Interfaces.Data
{
    public interface ISqlConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
