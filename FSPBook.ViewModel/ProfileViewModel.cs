namespace FSPBook.ViewModel;

public class ProfileViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public List<PostViewModel> Posts { get; set; } = new();
}
