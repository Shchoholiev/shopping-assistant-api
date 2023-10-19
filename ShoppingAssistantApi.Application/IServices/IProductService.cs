using System.Collections.ObjectModel;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.IServices;

public interface IProductService
{
    Task<List<string>> StartNewSearchAndReturnWishlist(Message message, CancellationToken cancellationToken);
    
    Task<List<string>> GetProductFromSearch(Message message, CancellationToken cancellationToken);
}