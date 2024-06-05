namespace PlusNine.Entities.Dtos.Responses
{
    public class UserSearchResponse
    {
        public Guid Id { get; set; }
        public required string UserName { get; set; }
    }
}