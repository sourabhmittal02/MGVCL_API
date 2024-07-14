using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CallCenterCoreAPI.Models.QueryModel
{
    public class ModelComplaintType
    {

        public int ComplaintTypeId { get; set; }
        public string ComplaintType { get; set; }

        public int SubComplaintTypeId { get; set; }
        public string SubComplaintType { get; set; }
        public string ComplaintTileColor { get; set; }
        public bool Status { get; set; }

        public bool IS_ACTIVE { get; set; }
        public bool IS_DELETED { get; set; }
        public string COMPLAINT_COUNT { get; set; }
        public DateTime TIME_STAMP { get; set; }


        public List<ModelOfficeCode> lstComplaint { get; set; }

    }

    public class ModelComplaintTypeAPI
    {

        public int ComplaintTypeId { get; set; }
        public int SubComplaintTypeId { get; set; }
        public string SubComplaintType { get; set; }
    }

    public class ModelComplaintTypeList
    {

        public int ComplaintTypeId { get; set; }
        public string ComplaintType { get; set; }


    }

    public class ModelComplaintSourceList
    {

        public int ComplaintSourceId { get; set; }
        public string ComplaintSource { get; set; }


    }

    public class ModelComplaintSendToCMS
    {
        public string compl_number { get; set; }
        public string cons_no { get; set; }
        public string compl_category { get; set; }
        public string compl_subcategory { get; set; }
        public string complaint_source { get; set; }
        public string compl_Details { get; set; }
        public string consumer_mobile { get; set; }
    }

    public class ModelComplaintSendNonConsumerToCMS
    {
        public string compl_number { get; set; }
        public string compl_category { get; set; }
        public string compl_subcategory { get; set; }
        public string complaint_source { get; set; }
        public string compl_Details { get; set; }
        public string consumer_mobile { get; set; }
        public string email_id { get; set; }
        public string consumer_name { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string VlgID { get; set; }
        public string OfficeCode { get; set; }
    }

    public class ModelComplaintSendStatusToCMS
    {
        public string compl_number { get; set; }
        public string compl_status { get; set; }
        public string compl_action_reason { get; set; }
        public string compl_action_description { get; set; }
    }

    public class ModelComplaintTagChangeToCMS
    {
        public string compl_number { get; set; }
        public string compl_category { get; set; }
        public string compl_subcategory { get; set; }
    }

    public class ModelCnsumerNo
    {
        public string cons_no { get; set; }
    }


    public class ModelHelpDesk
    {
        
        public string HDticketID { get; set; }
        public string HDTicketDate { get; set; }
        public string HDTicketType { get; set; }
        public string HDTicketDescription { get; set; }
        
        public string ConsumerID { get; set; }
        public string Meter_No { get; set; }
        public string Complaint_raised_by { get; set; }
        public string Raised_by_mobile_No { get; set; }
        public string accessToken { get; set; }
        
    }

    public class ModelHelpDeskResponse
    {
        public string status { get; set; }
        public string description { get; set; } 
        public dataHd data { get; set; }
    }

    public class dataHd
    {
        public string ConsumerID { get; set; }
        public string HDticketID { get; set; }
        public string SMOticketID { get; set; }
    }

}