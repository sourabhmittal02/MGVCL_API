namespace CallCenterCoreAPI.Models.QueryModel
{
    public class RemarkModel
    {
        public string ComplaintNo { get; set; }
        public string UserID { get; set; }
        public string Remark { get; set; }
        public int status { get; set; }
        public string Image { get; set; }
    }

    public class CloseComplaintModel
    {
        public string ComplaintNo { get; set; }
        public string ClosingAction { get; set; }
        public string Remark { get; set; }
        public Int64 UserID { get; set; }
    }
}
