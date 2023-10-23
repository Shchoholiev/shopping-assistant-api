using AutoMapper;
using MongoDB.Bson;
using ShoppingAssistantApi.Application.Exceptions;
using ShoppingAssistantApi.Application.GlobalInstances;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Application.Paging;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;

namespace ShoppingAssistantApi.Infrastructure.Services;

public class WishlistsService : IWishlistsService
{
    private readonly IWishlistsRepository _wishlistsRepository;

    private readonly IMessagesRepository _messagesRepository;

    private readonly IProductsRepository _productsRepository;

    private readonly IMapper _mapper;

    private readonly IOpenAiService _openAiService;

    public WishlistsService(IWishlistsRepository wishlistRepository, IMessagesRepository messageRepository,
            IProductsRepository productRepository, IMapper mapper, IOpenAiService openAiService)
    {
        _wishlistsRepository = wishlistRepository;
        _messagesRepository = messageRepository;
        _productsRepository = productRepository;
        _mapper = mapper;
        _openAiService = openAiService;
    }

    public async Task<WishlistDto> StartPersonalWishlistAsync(WishlistCreateDto dto, CancellationToken cancellationToken)
    {
        var newWishlist = _mapper.Map<Wishlist>(dto);

        if (!Enum.TryParse<WishlistTypes>(newWishlist.Type, true, out var enumValue) ||
            !Enum.GetValues<WishlistTypes>().Contains(enumValue))
        {
            throw new InvalidDataException("Provided type is invalid.");
        }

        newWishlist.CreatedById = (ObjectId) GlobalUser.Id;
        newWishlist.CreatedDateUtc = DateTime.UtcNow;
        newWishlist.Name = $"{newWishlist.Type} Search";

        var createdWishlist = await _wishlistsRepository.AddAsync(newWishlist, cancellationToken);

        var newMessage = new Message
        {
            Text = dto.FirstMessageText,
            Role = MessageRoles.User.ToString(),
            CreatedById = (ObjectId) GlobalUser.Id,
            CreatedDateUtc = DateTime.UtcNow,
            WishlistId = createdWishlist.Id
        };
        var createdMessage = await _messagesRepository.AddAsync(newMessage, cancellationToken);

        return _mapper.Map<WishlistDto>(createdWishlist);
    }

    public async Task<WishlistDto> GenerateNameForPersonalWishlistAsync(string wishlistId, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(wishlistId, out var wishlistObjectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }

        var wishlist = await TryGetPersonalWishlist(wishlistObjectId, cancellationToken);

        var firstUserMessage = (await _messagesRepository.GetPageAsync(1, 1, x => x.WishlistId == wishlistObjectId && x.Role == MessageRoles.User.ToString(), cancellationToken)).First();

        var chatCompletionRequest = new ChatCompletionRequest
        {
            Messages = new List<OpenAiMessage>(2)
            {
                new OpenAiMessage
                {
                    Role = OpenAiRole.System.RequestConvert(),
                    Content = "You will be provided with a general information about some product and your task is to generate general (not specific to any company or brand) chat name where recommendations on which specific product to buy will be given. Only name he product without adverbs and adjectives\nExamples:\n  - Prompt: Hub For Macbook. Answer: Macbook Hub\n  - Prompt: What is the best power bank for MacBook with capacity 20000 mAh and power near 20V? Answer: Macbook Powerbank"
                },
                new OpenAiMessage
                {
                    Role = OpenAiRole.User.RequestConvert(),
                    Content = firstUserMessage.Text
                }
            }
        };

        var openAiMessage = await _openAiService.GetChatCompletion(chatCompletionRequest, cancellationToken);

        wishlist = await _wishlistsRepository.UpdateWishlistNameAsync(wishlist.Id, openAiMessage.Content, cancellationToken);

        return _mapper.Map<WishlistDto>(wishlist);
    }

    public async Task<MessageDto> AddMessageToPersonalWishlistAsync(string wishlistId, MessageCreateDto dto, CancellationToken cancellationToken)
    {
        var newMessage = _mapper.Map<Message>(dto);

        if (!ObjectId.TryParse(wishlistId, out var wishlistObjectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }

        newMessage.Role = MessageRoles.User.ToString();
        newMessage.CreatedById = (ObjectId) GlobalUser.Id;
        newMessage.CreatedDateUtc = DateTime.UtcNow;
        newMessage.WishlistId = wishlistObjectId;

        await TryGetPersonalWishlist(wishlistObjectId, cancellationToken);

        var createdMessage = await _messagesRepository.AddAsync(newMessage, cancellationToken);

        return _mapper.Map<MessageDto>(createdMessage);
    }

    public async Task<PagedList<WishlistDto>> GetPersonalWishlistsPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var entities = await _wishlistsRepository.GetPageAsync(pageNumber, pageSize, x => x.CreatedById == GlobalUser.Id, cancellationToken);
        var dtos = _mapper.Map<List<WishlistDto>>(entities);
        var count = await _wishlistsRepository.GetTotalCountAsync();
        return new PagedList<WishlistDto>(dtos, pageNumber, pageSize, count);
    }

    public async Task<WishlistDto> GetPersonalWishlistAsync(string wishlistId, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(wishlistId, out var wishlistObjectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }

        var entity = await TryGetPersonalWishlist(wishlistObjectId, cancellationToken);
        
        return _mapper.Map<WishlistDto>(entity);
    }

    public async Task<PagedList<MessageDto>> GetMessagesPageFromPersonalWishlistAsync(string wishlistId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(wishlistId, out var wishlistObjectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }

        await TryGetPersonalWishlist(wishlistObjectId, cancellationToken);

        var entities = await _messagesRepository.GetPageStartingFromEndAsync(pageNumber, pageSize, x => x.WishlistId == wishlistObjectId, cancellationToken);

        var dtos = _mapper.Map<List<MessageDto>>(entities);
        var count = await _messagesRepository.GetCountAsync(x => x.WishlistId == wishlistObjectId, cancellationToken);
        return new PagedList<MessageDto>(dtos, pageNumber, pageSize, count);
    }

    public async Task<ProductDto> AddProductToPersonalWishlistAsync(string wishlistId, ProductCreateDto dto, CancellationToken cancellationToken)
    {
        var newProduct = _mapper.Map<Product>(dto);

        if (!ObjectId.TryParse(wishlistId, out var wishlistObjectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }

        await TryGetPersonalWishlist(wishlistObjectId, cancellationToken);
        
        newProduct.CreatedById = (ObjectId) GlobalUser.Id;
        newProduct.CreatedDateUtc = DateTime.UtcNow;
        newProduct.WishlistId = wishlistObjectId;

        var createdProduct = await _productsRepository.AddAsync(newProduct, cancellationToken);

        return _mapper.Map<ProductDto>(createdProduct);
    }

    public async Task<PagedList<ProductDto>> GetProductsPageFromPersonalWishlistAsync(string wishlistId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(wishlistId, out var wishlistObjectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }

        await TryGetPersonalWishlist(wishlistObjectId, cancellationToken);

        var entities = await _productsRepository.GetPageAsync(pageNumber, pageSize, x => x.WishlistId == wishlistObjectId, cancellationToken);

        var dtos = _mapper.Map<List<ProductDto>>(entities);
        var count = await _productsRepository.GetCountAsync(x => x.WishlistId == wishlistObjectId, cancellationToken);
        return new PagedList<ProductDto>(dtos, pageNumber, pageSize, count);
    }

    public async Task<WishlistDto> DeletePersonalWishlistAsync(string wishlistId, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(wishlistId, out var wishlistObjectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }

        var entity = await TryGetPersonalWishlist(wishlistObjectId, cancellationToken);

        entity.LastModifiedById = GlobalUser.Id;
        entity.LastModifiedDateUtc = DateTime.UtcNow;

        await _wishlistsRepository.DeleteAsync(entity, cancellationToken);

        return _mapper.Map<WishlistDto>(entity);
    }

    private async Task<Wishlist> TryGetPersonalWishlist(ObjectId wishlistId, CancellationToken cancellationToken)
    {
        var entity = await _wishlistsRepository.GetWishlistAsync(x => x.Id == wishlistId, cancellationToken);

        if (entity.CreatedById != GlobalUser.Id)
        {
            throw new UnAuthorizedException<Wishlist>();
        }

        if (entity == null)
        {
            throw new EntityNotFoundException<Wishlist>();
        }

        return entity;
    }
}
