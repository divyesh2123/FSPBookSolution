using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace FSPBook.Data.Entities;
public class Profile
{    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string JobTitle { get; set; }

    [NotMapped]
    public string FullName => FirstName + " " + LastName;

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
