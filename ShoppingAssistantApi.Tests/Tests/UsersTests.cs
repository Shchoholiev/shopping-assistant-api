using ShoppingAssistantApi.Tests.TestExtentions;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.Identity;
using Newtonsoft.Json.Linq;
using ShoppingAssistantApi.Application.Paging;

namespace ShoppingAssistantApi.Tests.Tests;

// TODO: make errors test more descrptive
public class UsersTests : TestsBase
{
    public UsersTests(TestingFactory<Program> factory)
        : base(factory)
    { }

    [Fact]
    public async Task UpdateUserAsync_ValidUserModel_ReturnsUpdateUserModel()
    {
        await LoginAsync("test@gmail.com", "Yuiop12345");
        var user = await GetCurrentUserAsync();
        var mutation = new
        {
            query = @"
                mutation UpdateUser($userDto: UserDtoInput!) { 
                    updateUser(userDto: $userDto) { 
                        tokens { accessToken, refreshToken }, 
                        user { email, phone } 
                    }
                }",
            variables = new
            {
                userDto = new
                {
                    id = user.Id,
                    guestId = user.GuestId,
                    roles = user.Roles.Select(r => new { id = r.Id, name = r.Name }),
                    email = user.Email,
                    phone = "+12345678902",
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var tokens = (TokensModel?) jsonObject?.data?.updateUser?.tokens?.ToObject<TokensModel>();
        var updatedUser = (UserDto?) jsonObject?.data?.updateUser?.user?.ToObject<UserDto>();

        Assert.NotNull(tokens);
        Assert.NotNull(tokens.AccessToken);
        Assert.NotNull(tokens.RefreshToken);
        Assert.NotNull(updatedUser);
        Assert.NotNull(updatedUser.Email);
        Assert.Equal(user.Email, updatedUser.Email);
        Assert.Equal("+12345678902", updatedUser.Phone);
    }

    [Fact]
    public async Task UpdateUserByAdminAsync_ValidUserModel_ReturnsUpdateUserModel()
    {
        await LoginAsync("test@gmail.com", "Yuiop12345");
        var user = await GetCurrentUserAsync();
        var mutation = new
        {
            query = @"
                mutation UpdateUserByAdmin($id: String!, $userDto: UserDtoInput!) { 
                    updateUserByAdmin(id: $id, userDto: $userDto) { 
                        email, 
                        phone 
                    }
                }",
            variables = new
            {
                id = user.Id,
                userDto = new
                {
                    id = user.Id,
                    guestId = user.GuestId,
                    roles = user.Roles.Select(r => new { id = r.Id, name = r.Name }),
                    email = user.Email,
                    phone = "+12345678903",
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var updatedUser = (UserDto?) jsonObject?.data?.updateUserByAdmin?.ToObject<UserDto>();

        Assert.NotNull(updatedUser);
        Assert.NotNull(updatedUser.Email);
        Assert.Equal(user.Email, updatedUser.Email);
        Assert.Equal("+12345678903", updatedUser.Phone);
    }

    [Fact]
    public async Task GetUserAsync_ValidUserId_ReturnsUser()
    {
        await LoginAsync("admin@gmail.com", "Yuiop12345");
        var query = new
        {
            query = @"
                query User($id: String!) { 
                    user(id: $id) { 
                        id, 
                        email 
                    }
                }",
            variables = new
            {
                id = "652c3b89ae02a3135d6409fc",
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var user = (UserDto?) jsonObject?.data?.user?.ToObject<UserDto>();

        Assert.NotNull(user);
        Assert.Equal("652c3b89ae02a3135d6409fc", user.Id);
        Assert.Equal("test@gmail.com", user.Email);
    }

    [Fact]
    public async Task GetUserAsync_InvalidUserId_ReturnsInternalServerError()
    {
        await LoginAsync("admin@gmail.com", "Yuiop12345");
        var query = new
        {
            query = "query User($id: String!) { user(id: $id) { id }}",
            variables = new
            {
                id = "invalid",
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetCurrentUserAsync_Authorized_ReturnsCurrentUser()
    {
        await LoginAsync("admin@gmail.com", "Yuiop12345");
        var query = new
        {
            query = "query CurrentUser { currentUser { id, email, phone }}"
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var user = (UserDto?) jsonObject?.data?.currentUser?.ToObject<UserDto>();

        Assert.NotNull(user);
        Assert.Equal("652c3b89ae02a3135d6408fc", user.Id);
        Assert.Equal("admin@gmail.com", user.Email);
        Assert.Equal("+12345678901", user.Phone);
    }

    [Fact]
    public async Task GetUsersPageAsync_ValidPageNumberAndSize_ReturnsUsersPage()
    {
        await LoginAsync("admin@gmail.com", "Yuiop12345");
        var query = new
        {
            query = @"
                query UsersPage($pageNumber: Int!, $pageSize: Int!) { 
                    usersPage(pageNumber: $pageNumber, pageSize: $pageSize) { 
                        items { id, email, phone }
                    }
                }",
            variables = new
            {
                pageNumber = 1,
                pageSize = 10
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var pagedList = (PagedList<UserDto>?) jsonObject?.data?.usersPage?.ToObject<PagedList<UserDto>>();

        Assert.NotNull(pagedList);
        Assert.NotEmpty(pagedList.Items);
    }


    [Fact]
    public async Task AddToRoleAsync_ValidRoleName_ReturnsTokensModel()
    {
        await LoginAsync("admin@gmail.com", "Yuiop12345");
        var mutation = new
        {
            query = @"
                mutation AddToRole($roleName: String!, $userId: String!) { 
                    addToRole(roleName: $roleName, userId: $userId) { 
                        id, email, roles {
                            name
                        }
                    }
                }",
            variables = new
            {
                roleName = "Admin",
                userId = "652c3b89ae02a3135d6409fc",
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var user = (UserDto?) jsonObject?.data?.addToRole?.ToObject<UserDto>();

        Assert.NotNull(user);
        Assert.Equal("652c3b89ae02a3135d6409fc", user.Id);
        Assert.Equal("test@gmail.com", user.Email);
        Assert.Contains(user.Roles, r => r.Name == "Admin");
    }

    [Fact]
    public async Task AddToRoleAsync_NonExistingRole_ReturnsErrors()
    {
        await LoginAsync("admin@gmail.com", "Yuiop12345");
        var mutation = new
        {
            query = @"
                mutation AddToRole($roleName: String!, $userId: String!) { 
                    addToRole(roleName: $roleName, userId: $userId) { 
                        id, email, roles {
                            name
                        }
                    }
                }",
            variables = new
            {
                roleName = "NonExistingRole",
                id = "652c3b89ae02a3135d6409fc",
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }


    [Fact]
    public async Task RemoveFromRoleAsync_ValidRoleName_ReturnsTokensModel()
    {
        await LoginAsync("admin@gmail.com", "Yuiop12345");
        var mutation = new
        {
            query = @"
                mutation RemoveFromRole($roleName: String!, $userId: String!) { 
                    removeFromRole(roleName: $roleName, userId: $userId) { 
                        id, email, roles {
                            name
                        }
                    }
                }",
            variables = new
            {
                roleName = "User",
                userId = "652c3b89ae02a3135d6409fc",
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var user = (UserDto?) jsonObject?.data?.removeFromRole?.ToObject<UserDto>();

        Assert.NotNull(user);
        Assert.Equal("652c3b89ae02a3135d6409fc", user.Id);
        Assert.Equal("test@gmail.com", user.Email);
        Assert.DoesNotContain(user.Roles, r => r.Name == "User");
    }

    [Fact]
    public async Task RemoveFromRoleAsync_NonExistingRole_ReturnsErrors()
    {
        await LoginAsync("admin@gmail.com", "Yuiop12345");
        var mutation = new
        {
            query = @"
                mutation RemoveFromRole($roleName: String!, $userId: String!) { 
                    removeFromRole(roleName: $roleName, userId: $userId) { 
                        id, email, roles {
                            name
                        }
                    }
                }",
            variables = new
            {
                roleName = "NonExistingRole",
                userId = "652c3b89ae02a3135d6409fc",
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }
}
