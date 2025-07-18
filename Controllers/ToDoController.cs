using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.Text;

public class ToDoController : Controller
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    public ToDoController(AppDbContext context)
    {
        _context = context;
        _httpClient = new HttpClient();
    }

   public async Task<IActionResult> Index(string filter = "all")
{
    IQueryable<ToDoItem> query = _context.ToDoItems;

    switch (filter.ToLower())
    {
        case "completed":
            query = query.Where(t => t.IsCompleted);
            break;
        case "incomplete":
            query = query.Where(t => !t.IsCompleted);
            break;
        // "all" or any other unrecognized value defaults to all
    }

    ViewBag.Filter = filter; // Pass the current filter to the view
    return View(await query.ToListAsync());
}



    [HttpPost]
    public async Task<IActionResult> Create(string description, string language)
    {
        string translated;

        try
        {
            translated = await TranslateText(description, language);

            // If translation is same as original, clear out translated to avoid confusing UI
            if (translated.Trim().Equals(description.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                translated = null;
            }
        }
        catch
        {
            translated = null;  // fallback if translation fails or API error
        }

        var item = new ToDoItem
        {
            Description = description,
            Language = language,
            TranslatedDescription = translated,
            IsCompleted = false
        };

        _context.ToDoItems.Add(item);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        var item = await _context.ToDoItems.FindAsync(id);
        if (item != null)
        {
            item.IsCompleted = true;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    private async Task<string> TranslateText(string text, string language)
{
    using var httpClient = new HttpClient();
    var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair=en|{language}";

    var response = await httpClient.GetAsync(url);
    response.EnsureSuccessStatusCode();

    var jsonString = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(jsonString);

    var translatedText = doc.RootElement
                            .GetProperty("responseData")
                            .GetProperty("translatedText")
                            .GetString();

    return translatedText;
}

}
