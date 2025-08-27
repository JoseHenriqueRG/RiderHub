namespace RiderHub.Domain.Exceptions
{
    public class DuplicateCnpjException : Exception
    {
        public DuplicateCnpjException() : base("A CNPJ with this number already exists.")
        {
        }
    }
}
