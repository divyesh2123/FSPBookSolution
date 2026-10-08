namespace FSPBook.ViewModel;

public class PostViewModel
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset DateTimePosted { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
}
