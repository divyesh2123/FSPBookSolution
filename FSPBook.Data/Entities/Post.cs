using System;

namespace FSPBook.Data.Entities;
public class Post
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTimeOffset DateTimePosted {get;set;}
    public int AuthorId { get; set; }

    public virtual Profile Author { get; set; }
}
