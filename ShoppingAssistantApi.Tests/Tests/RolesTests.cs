using ShoppingAssistantApi.Tests.TestExtentions;
using Newtonsoft.Json.Linq;
using ShoppingAssistantApi.Application.Paging;
using ShoppingAssistantApi.Application.Models.Dtos;

namespace ShoppingAssistantApi.Tests.Tests;

// TODO: make errors test more descrptive
public class RolesTests : TestsBase
{
    public RolesTests(TestingFactory<Program> factory)
        : base(factory)
    { }

    [Fact]
    public async Task AddRole_ValidName_ReturnsCreatedRole()
    {
        await LoginAsync("admin@gmail.com", "Yuiop12345");
        var mutation = new
        {
            query = "mutation AddRole ($dto: RoleCreateDtoInput!){ addRole (roleDto: $dto) { id, name }} ",
            variables = new
            {
                dto = new
                {
                    name = "NewRole"
                }
            }
        };
        
        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var role = jsonObject?.data?.addRole?.ToObject<RoleDto>();

        Assert.NotNull(role);
        Assert.Equal("NewRole", role.Name);
    }

    [Fact]
    public async Task AddRole_Unauthorized_ReturnsErrors()
    {
        var mutation = new
        {
            query = "mutation AddRole ($dto: RoleCreateDtoInput!){ addRole (roleDto: $dto) { id, name }} ",
            variables = new
            {
                dto = new
                {
                    name = "NewRole"
                }
            }
        };
        
        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task AddRole_ExistingRoleName_ReturnsErrors()
    {
        await LoginAsync("admin@gmail.com", "Yuiop12345");
        var mutation = new
        {
            query = "mutation AddRole ($dto: RoleCreateDtoInput!){ addRole (roleDto: $dto) { id, name }} ",
            variables = new
            {
                dto = new
                {
                    name = "User"
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetRolesPageAsync_ValidPageNumberAndSize_ReturnsRolesPagedList()
    {
        await LoginAsync("admin@gmail.com", "Yuiop12345");
        var query = new
        {
            query = "query RolesPage($pageNumber: Int!, $pageSize: Int!) { rolesPage(pageNumber: $pageNumber, pageSize: $pageSize) { items { id, name } }}",
            variables = new
            {
                pageNumber = 1,
                pageSize = 3
            }
        };
        
        var jsonObject = await SendGraphQlRequestAsync(query);
        var pagedList = (PagedList<RoleDto>?) jsonObject?.data?.rolesPage?.ToObject<PagedList<RoleDto>>();

        Assert.NotNull(pagedList);
        Assert.NotEmpty(pagedList.Items);
    }
}