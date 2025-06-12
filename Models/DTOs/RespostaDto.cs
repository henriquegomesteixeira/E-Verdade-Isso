using System.Text.Json.Serialization;

namespace everdadeisso.Models.DTOs
{
    public class RespostaDto
    {
        [JsonPropertyName("classificacao")]
        public string Classificacao { get; set; }

        [JsonPropertyName("explicacao")]
        public string Explicacao { get; set; }

        [JsonPropertyName("referencias")]
        public List<Referencia> Referencias { get; set; }
    }
}
