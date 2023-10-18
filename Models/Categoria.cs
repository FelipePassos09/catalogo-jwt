using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace APICatalogo_minimal.Models
{
    public class Categoria
    { 
        public int CategoriaId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        [JsonIgnore]
        public ICollection<Produto> Produtos { get; set; }

        public Categoria() { Produtos = new Collection<Produto>(); }
    }
}
