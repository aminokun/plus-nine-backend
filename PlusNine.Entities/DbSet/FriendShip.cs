namespace PlusNine.Entities.DbSet
{
    public class Friendship : BaseEntity
    {
        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }
    }
}
