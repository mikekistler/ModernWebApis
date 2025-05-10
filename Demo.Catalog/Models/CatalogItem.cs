using System.ComponentModel.DataAnnotations;

namespace Catalog.Models;

/// <summary>
/// Represents an item in the catalog with detailed product information.
/// </summary>
public class CatalogItem
{
    /// <summary>
    /// Unique identifier for the catalog item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the product.
    /// </summary>
    [Required]
    [MaxLength(50)]
    [MinLength(3, ErrorMessage = "{0} must be at least 3 characters long.")]
    public string? Name { get; set; }

    /// <summary>
    /// Detailed description of the product.
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string? Description { get; set; }

    /// <summary>
    /// Current price of the product.
    /// </summary>
    [Required]
    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    /// <summary>
    /// Filename of the product image.
    /// </summary>
    public string? PictureFileName { get; set; }

    /// <summary>
    /// Category or type of the product.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string? CatalogType { get; set; }

    /// <summary>
    /// Brand of the product.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string? CatalogBrand { get; set; }

    /// <summary>
    /// Current quantity available in stock.
    /// </summary>
    public int AvailableStock { get; set; }

    /// <summary>
    /// Available stock at which we should reorder.
    /// </summary>
    public int RestockThreshold { get; set; }

    /// <summary>
    /// Maximum number of units that can be in-stock at any time (due to physical/logistical constraints in warehouses).
    /// </summary>
    public int MaxStockThreshold { get; set; }

    /// <summary>
    /// Indicates whether the item is currently in the process of being restocked.
    /// </summary>
    public bool OnReorder { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogItem"/> class.
    /// </summary>
    public CatalogItem() { }
}