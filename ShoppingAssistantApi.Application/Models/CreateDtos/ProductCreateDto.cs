namespace ShoppingAssistantApi.Application.Models.CreateDtos;

public class ProductCreateDto
{
    public required string Url { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required double Rating { get; set; }

    public required string[] ImagesUrls { get; set; }

    public required bool WasOpened { get; set; }
}
