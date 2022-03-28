namespace Cep.Domain.Models
{
    public record CepModel
    {
        public string Cep { get; init; }
        public string Logradouro { get; init; }
        public string Complemento { get; init; }
        public string Bairro { get; init; }
        public string Localidade { get; init; }
        public string Uf { get; init; }
        public string Ibge { get; init; }
        public string Gia { get; init; }
        public string Ddd { get; init; }
        public string Siafi { get; init; }

        public CepModel() { }

        public CepModel(string cep, string logradouro, string complemento, string bairro, string localidade,
            string uf, string ibge, string gia, string ddd, string siafi) =>
            (Cep, Logradouro, Complemento, Bairro, Localidade, Uf, Ibge, Gia, Ddd, Siafi) =
            (cep, logradouro, complemento, bairro, localidade, uf, ibge, gia, ddd, siafi);
    }
}
