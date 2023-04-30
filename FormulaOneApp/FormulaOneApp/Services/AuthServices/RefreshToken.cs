
namespace FormulaOneApp.Services.AuthServices
{
    internal class RefreshToken
    {
        public RefreshToken()
        {
        }

        public string JwtId { get; set; }
        public bool IsUsed { get; set; }
        public bool IsReworked { get; set; }
        public string UserId { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Token { get; set; }
    }
}