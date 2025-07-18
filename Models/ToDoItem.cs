using System.ComponentModel.DataAnnotations;

public class ToDoItem
{
    public int Id { get; set; }

    [Required]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public string? TranslatedDescription { get; set; }

    public string? Language { get; set; }
}
