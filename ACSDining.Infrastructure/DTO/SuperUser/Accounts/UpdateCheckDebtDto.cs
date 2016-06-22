namespace ACSDining.Infrastructure.DTO.SuperUser.Accounts
{
    public class UpdateCheckDebtDto
    {
        public string UserId { get; set; }
        //Пользователь может делать заказ
        public bool CheckDebt { get; set; }
    }
}
