using MongoDB.Bson;
using ShoppingAssistantApi.Domain.Common;

namespace ShoppingAssistantApi.Application.Exceptions;

public class UnAuthorizedException<TEntity> : Exception where TEntity : EntityBase
{
    public UnAuthorizedException() { }

    public UnAuthorizedException(ObjectId id) : base(String.Format($"Access to object {id} of type {typeof(TEntity).Name} denied.")) { }
}
