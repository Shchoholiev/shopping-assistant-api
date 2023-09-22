using AutoMapper;
using MongoDB.Bson;
using ShoppingAssistantApi.Application.Exceptions;
using ShoppingAssistantApi.Application.GlobalInstances;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Paging;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Infrastructure.Services;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _repository;

    private readonly IMapper _mapper;

    public UsersService(IUsersRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task AddUserAsync(UserDto dto, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<User>(dto);
        await _repository.AddAsync(entity, cancellationToken);
    }

    public async Task<PagedList<UserDto>> GetUsersPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetPageAsync(pageNumber, pageSize, cancellationToken);
        var dtos = _mapper.Map<List<UserDto>>(entities);
        var count = await _repository.GetTotalCountAsync();
        return new PagedList<UserDto>(dtos, pageNumber, pageSize, count);
    }

    public async Task<UserDto> GetUserAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }

        var entity = await _repository.GetUserAsync(objectId, cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException<User>();
        }

        return _mapper.Map<UserDto>(entity);
    }

    public async Task UpdateUserAsync(UserDto dto, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<User>(dto);
        entity.LastModifiedById = GlobalUser.Id.Value;
        entity.LastModifiedDateUtc = DateTime.UtcNow;
        await _repository.UpdateUserAsync(entity, cancellationToken);
    }
}
