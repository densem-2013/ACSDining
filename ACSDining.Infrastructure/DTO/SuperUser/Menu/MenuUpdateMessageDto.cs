namespace ACSDining.Infrastructure.DTO.SuperUser.Menu
{
    public class MenuUpdateMessageDto
    {
        public string DateTime { get; set; }
        public string Message { get; set; }
        public int[] UpdatedDayMenu { get; set; }
    }

    public class MenuCanBeOrderedMessageDto
    {
        public int WeekMenuId { get; set; }
        public string DateTime { get; set; }
    }
}
