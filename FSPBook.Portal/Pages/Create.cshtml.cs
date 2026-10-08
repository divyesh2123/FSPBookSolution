using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FSPBook.Data;
using FSPBook.Data.Entities;
using FSPBook.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FSPBook.Pages;
public class CreateModel : PageModel
{
    private readonly Context _context;
    private readonly IProfileService _profileService;
    private readonly IPostService _postService;

    public List<Profile> Profiles { get; set; }

    public bool Success { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Choose a person to post on behalf of")]
    [Range(1, 10000, ErrorMessage = "Choose a person to post on behalf of")]
    public int ProfileId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Write a post")]
    [MinLength(1, ErrorMessage = "Post needs some content")]
    public string ContentInput { get; set; }

    public CreateModel(Context context, IProfileService profileService, IPostService postService)
    {
        _context = context;
        _profileService = profileService;
        _postService = postService;
    }

    public async Task OnGetAsync()
    {
        await LoadProfiles();
    }

    public async Task OnPostAsync()
    {
        if (ProfileId != -1)
        {
            await _postService.CreatePostAsync(ProfileId, ContentInput);
            Success = true;
            ContentInput = string.Empty;
        }

        await LoadProfiles();
    }

    private async Task LoadProfiles()
    {
        Profiles = await _context.Profile.ToListAsync();
    }
}

