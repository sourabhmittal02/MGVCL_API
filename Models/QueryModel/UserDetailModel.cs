namespace CallCenterCoreAPI.Models.QueryModel
{
    public class UserDetailModel
    {
            public int User_Id { get; set; }
            public string Name { get; set; }
            public long Phone { get; set; }
            public string Email { get; set; }
            public string Address { get; set; }
    }
    public class UserRegisterModel
    {
        public string USER_NAME { get; set; }
        public string PASSWORD { get; set; }
        public string NAME { get; set; }
        public string ADDRESS { get; set; }
        public string MOBILE_NO { get; set; }
        public string EMAIL_ID { get; set; }
    }
}
