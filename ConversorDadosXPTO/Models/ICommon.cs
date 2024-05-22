namespace ConversorDadosXPTO.Models
{
    public interface ICommon
    {
        public int CityCode { get; set; }
        public string CityName { get; set; }
        public string StateAcronym { get; set; }
        public string Cpf { get; set; }
        public string Nis { get; set; }
        public string Person { get; set; }
    }
}