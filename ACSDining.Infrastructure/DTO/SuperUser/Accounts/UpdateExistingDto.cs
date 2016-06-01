namespace ACSDining.Infrastructure.DTO.SuperUser.Accounts
{
    public class UpdateExistingDto
    {
        public string UserId { get; set; }
        //Пользователь существует( не удалён из Active Directory)
        public bool IsExisting { get; set; }
    }
}
