namespace CallCenterCoreAPI.Models.QueryModel
{
    public class ModelPowerOutage
    {
        public Int64 OFFICE_CODE { get; set; }
        public string START_TIME { get; set; }
        public string END_TIME { get; set; }
        public string COLONIES { get; set; }
        public string SHUT_DOWN_INFORMATION { get; set; }
        public string INFORMATION_SOURCE { get; set; }
    }
}
