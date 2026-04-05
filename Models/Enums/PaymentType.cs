namespace BankingAPI.Models.Enums
{
    /// <summary>
    /// Defines the types of payment methods available in the banking system
    /// </summary>
    public enum PaymentType
    {
        UPI = 1,
        InternetBanking = 2,
        CreditCard = 3,
        DebitCard = 4,
        NEFT = 5,
        RTGS = 6,
        IMPS = 7
    }
}