using Microsoft.AspNetCore.Mvc.RazorPages;
using FSPBook.Service.Interfaces;
using FSPBook.ViewModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FSPBook.Pages;
public class ProfileModel : PageModel
{
    private readonly IProfileService _profileService;

    public ProfileModel(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public ProfileViewModel? Profile { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Id = id ?? Id;
        if (Id == 0) return NotFound();

        Profile = await _profileService.GetProfileWithPostsAsync(Id);
        if (Profile == null) return NotFound();

        return Page();
    }
}
