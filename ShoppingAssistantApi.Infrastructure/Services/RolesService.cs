using AutoMapper;
using MongoDB.Bson;
using ShoppingAssistantApi.Application.Exceptions;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Paging;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Infrastructure.Services;

public class RolesService : IRolesService
{
    private readonly IRolesRepository _repository;

    private readonly IMapper _mapper;

    public RolesService(IRolesRepository repository, IMapper mapper)
    {
        this._repository = repository;
        this._mapper = mapper;
    }

    public async Task<RoleDto> AddRoleAsync(RoleCreateDto dto, CancellationToken cancellationToken)
    {
        var role = await this._repository.GetRoleAsync(r => r.Name == dto.Name, cancellationToken);
        if (role != null)
        {
            throw new EntityAlreadyExistsException<Role>();
        }
        var entity = this._mapper.Map<Role>(dto);
        entity.CreatedDateUtc = DateTime.UtcNow;
        entity.LastModifiedDateUtc = DateTime.UtcNow;
        await this._repository.AddAsync(entity, cancellationToken);

        return this._mapper.Map<RoleDto>(entity);
    }

    public async Task<PagedList<RoleDto>> GetRolesPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var entities = await this._repository.GetPageAsync(pageNumber, pageSize, cancellationToken);
        var count = await this._repository.GetTotalCountAsync();
        var dtos = this._mapper.Map<List<RoleDto>>(entities);

        return new PagedList<RoleDto>(dtos, pageNumber, pageSize, count);
    }
}
