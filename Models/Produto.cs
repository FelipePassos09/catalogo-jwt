using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json.Serialization;

namespace APICatalogo_minimal.Models
{
    public class Produto
    {
        public int ProuctId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Immage { get; set; }
        public DateTime BuyDate { get; set; }
        public int Stock { get; set; }
        public int CategoriaId { get; set; }

        [JsonIgnore]
        public Categoria? Categoria { get; set; }

    }
}
