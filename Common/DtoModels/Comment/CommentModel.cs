﻿using System.ComponentModel.DataAnnotations;

namespace Common.DtoModels.Comment;

public class CommentModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime CreateTime { get; set; }
    public DateTime? ModifiedDate { get; set; }
    [Required]
    [MinLength(1)]
    public string Content { get; set; } = string.Empty;
    [Required]
    public Guid AuthorId { get; set; }
    [Required]
    [MinLength(1)]
    public string Author { get; set; }
    public Guid? ParentId { get; set; }
}