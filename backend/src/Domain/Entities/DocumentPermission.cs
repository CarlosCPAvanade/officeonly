namespace Domain.Entities;

public class DocumentPermission
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public bool CanRead { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
