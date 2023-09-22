using ShoppingAssistantApi.Domain.Common;

namespace ShoppingAssistantApi.Application.Exceptions;
public class EntityAlreadyExistsException<TEntity> : Exception where TEntity : EntityBase
{
    public EntityAlreadyExistsException()
        : base($"\"{typeof(TEntity).Name}\" already exists.") { }

    public EntityAlreadyExistsException(string message, Exception innerException)
        : base(message, innerException) { }

    public EntityAlreadyExistsException(string paramName, string paramValue)
        : base($"\"{typeof(TEntity).Name}\" with {paramName}: \"{paramValue}\" already exists.") { }
}
