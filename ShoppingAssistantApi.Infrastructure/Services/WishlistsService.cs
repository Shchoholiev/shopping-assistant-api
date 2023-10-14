using AutoMapper;
using MongoDB.Bson;
using ShoppingAssistantApi.Application.Exceptions;
using ShoppingAssistantApi.Application.GlobalInstances;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Paging;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;

namespace ShoppingAssistantApi.Infrastructure.Services;

public class WishlistsService : IWishlistsService
{
    private readonly IWishlistsRepository _wishlistsRepository;

    private readonly IMessagesRepository _messagesRepository;

    private readonly IMapper _mapper;

    public WishlistsService(IWishlistsRepository wishlistRepository, IMessagesRepository messageRepository, IMapper mapper)
    {
        _wishlistsRepository = wishlistRepository;
        _messagesRepository = messageRepository;
        _mapper = mapper;
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
            WishlistId = createdWishlist.Id
        };
        var createdMessage = await _messagesRepository.AddAsync(newMessage, cancellationToken);

        return _mapper.Map<WishlistDto>(createdWishlist);
    }

    public async Task<MessageDto> AddMessageToPersonalWishlistAsync(string wishlistId, MessageCreateDto dto, CancellationToken cancellationToken)
    {
        var newMessage = _mapper.Map<Message>(dto);

        if (!ObjectId.TryParse(wishlistId, out var wishlistObjectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }
        newMessage.WishlistId = wishlistObjectId;
        newMessage.Role = MessageRoles.User.ToString();
        newMessage.CreatedById = (ObjectId) GlobalUser.Id;
        newMessage.CreatedDateUtc = DateTime.UtcNow;

        await TryGetPersonalWishlist(wishlistObjectId, cancellationToken);

        var createdMessage = await _messagesRepository.AddAsync(newMessage, cancellationToken);

        return _mapper.Map<MessageDto>(createdMessage);
    }

    public async Task<PagedList<WishlistDto>> GetPersonalWishlistsPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var entities = await _wishlistsRepository.GetPageAsync(pageNumber, pageSize, cancellationToken);
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
