using CallCenterCoreAPI.Database.Repository;
using CallCenterCoreAPI.Models;
using CallCenterCoreAPI.Models.QueryModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CallCenterCoreAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ComplaintController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public ComplaintController(ILogger<ComplaintController> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        [HttpPost]
        [Route("SaveComplaint")]
        public async Task<IActionResult> SaveComplaint(COMPLAINT modelComplaint)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            Int64 retStatus = 0;
            string msg = string.Empty;
            DataSet dsResponse = modelComplaintRepository.SearchComplaint(modelComplaint.KNO, modelComplaint.ComplaintTypeId);
            if (dsResponse.Tables[0].Rows.Count > 0)
            {
                if (Convert.ToInt16(dsResponse.Tables[0].Rows[0]["COMPLAINT_status"].ToString()) != 2)
                {
                    _logger.LogInformation("You Already Have Complaint No." + dsResponse.Tables[0].Rows[0]["COMPLAINT_NO"].ToString() + " with Status " + dsResponse.Tables[0].Rows[0]["cst"]);
                    returnStatus.response = 0;
                    returnStatus.status = "You Already Have Complaint No. " + dsResponse.Tables[0].Rows[0]["COMPLAINT_NO"].ToString() + " with Status " + dsResponse.Tables[0].Rows[0]["cst"];
                    return BadRequest(returnStatus);
                }
            }
            else
            {
                retStatus = await modelComplaintRepository.SaveComplaint(modelComplaint);
            }
            if (retStatus > 0)
            {
                returnStatus.response = 1;
                returnStatus.status = "Complaint Successfully Registered With Complaint No. " + retStatus.ToString();
                return Ok(returnStatus);
            }
                
            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Saving Complaint";
                return BadRequest(returnStatus);
            }
                
           
        }

        [HttpPost]
        [Route("SaveComplaintAPI")]
        public async Task<IActionResult> SaveComplaintAPI(COMPLAINT_API modelComplaint)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            Int64 retStatus = 0;
            string msg = string.Empty;
            DataSet dsResponse = modelComplaintRepository.SearchComplaint(modelComplaint.KNO, modelComplaint.ComplaintTypeId);
            if (dsResponse.Tables[0].Rows.Count > 0)
            {
                if (Convert.ToInt16(dsResponse.Tables[0].Rows[0]["COMPLAINT_status"].ToString()) != 2)
                {
                    _logger.LogInformation("You Already Have Complaint No." + dsResponse.Tables[0].Rows[0]["COMPLAINT_NO"].ToString() + " with Status " + dsResponse.Tables[0].Rows[0]["cst"]);
                    returnStatus.response = 0;
                    returnStatus.status = "You Already Have Complaint No. " + dsResponse.Tables[0].Rows[0]["COMPLAINT_NO"].ToString() + " with Status " + dsResponse.Tables[0].Rows[0]["cst"];
                    return BadRequest(returnStatus);
                }
            }
            else
            {
                retStatus = await modelComplaintRepository.SaveComplaintAPI(modelComplaint);
            }
            if (retStatus > 0)
            {
                returnStatus.response = 1;
                returnStatus.status = "Complaint Successfully Registered With Complaint No. " + retStatus.ToString();
                return Ok(returnStatus);
            }

            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Saving Complaint";
                return BadRequest(returnStatus);
            }


        }

        [HttpPost]
        [Route("SaveComplaintIntegrationAPI")]
        public async Task<IActionResult> SaveComplaintIntegrationAPI(COMPLAINT_Integration modelComplaint)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            Int64 retStatus = 0;
            string msg = string.Empty;
            _logger.LogInformation("Test");

            DataSet dsResponse = modelComplaintRepository.SearchComplaintInti(modelComplaint.KNO,modelComplaint.ComplaintType);

            _logger.LogInformation("SearchComplaintInti");
            if (dsResponse.Tables[0].Rows.Count > 0)
            {
                _logger.LogInformation("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                if (Convert.ToInt16(dsResponse.Tables[0].Rows[0]["COMPLAINT_status"].ToString()) != 2)
                {
                    _logger.LogInformation("You Already Have Complaint No." + dsResponse.Tables[0].Rows[0]["COMPLAINT_NO"].ToString() + " with Status " + dsResponse.Tables[0].Rows[0]["cst"]);
                    returnStatus.response = 0;
                    returnStatus.status = "You Already Have Complaint No. " + dsResponse.Tables[0].Rows[0]["COMPLAINT_NO"].ToString() + " with Status " + dsResponse.Tables[0].Rows[0]["cst"];
                    return Ok(returnStatus);
                }
                else
                {
                    _logger.LogInformation("Not Exist complaint");
                    retStatus = await modelComplaintRepository.SaveComplaintIntegrationAPI(modelComplaint);
                }
            }
            else
            {
                _logger.LogInformation("Not Exist complaint");
                retStatus = await modelComplaintRepository.SaveComplaintIntegrationAPI(modelComplaint);
            }
            if (retStatus > 0)
            {
                returnStatus.response = 1;
                returnStatus.status = "Complaint Successfully Registered With Complaint No. " + retStatus.ToString();
                return Ok(returnStatus);
            }

            else
            {
                _logger.LogInformation("Error in Saving Complaint");

               returnStatus.response = 0;
                returnStatus.status = "Error in Saving Complaint";
                return BadRequest(returnStatus);
            }


        }

        [HttpPost]
        [Route("SaveComplaintIVR")]
        public async Task<IActionResult> SaveComplaintIVR(COMPLAINTIVR modelComplaint)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            Int64 retStatus = 0;
            string msg = string.Empty;
            DataSet dsResponse = modelComplaintRepository.SearchComplaintIVR(modelComplaint);
            if (dsResponse.Tables[0].Rows.Count > 0)
            {
                if (Convert.ToInt16(dsResponse.Tables[0].Rows[0]["COMPLAINT_status"].ToString()) != 2)
                {
                    _logger.LogInformation("You Already Have a pending Complaint No." + dsResponse.Tables[0].Rows[0]["COMPLAINT_NO"].ToString());
                    returnStatus.response = 0;
                    returnStatus.status = "You Already Have a pending Complaint No. " + dsResponse.Tables[0].Rows[0]["COMPLAINT_NO"].ToString();
                    return BadRequest(returnStatus);
                }
            }
            else
            {
                retStatus = await modelComplaintRepository.SaveComplaintDetailIVR(modelComplaint);
            }
            if (retStatus > 0)
            {
                returnStatus.response = 1;
                returnStatus.status = "Complaint Successfully Registered With Complaint No. " + retStatus.ToString();
                return Ok(returnStatus);
            }

            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Saving Complaint";
                return BadRequest(returnStatus);
            }


        }

        [HttpPost]
        [Route("SearchComplaintByKNO")]
        public IActionResult SearchComplaintByKNO(ComplaintSearchQueryModel complaintSearchQueryModel)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            List<COMPLAINT_SEARCH_API> lstComplaints = modelComplaintRepository.GetPreviousComplaintByKno(complaintSearchQueryModel.KNO);
            return Ok(lstComplaints);
        }

        [HttpPost]
        [Route("SearchComplaintByComplaintNo")]
        public IActionResult SearchComplaintByComplaintNo(ComplaintSearchQueryModel complaintSearchQueryModel)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            List<COMPLAINT_SEARCH_API> lstComplaints = modelComplaintRepository.GetPreviousComplaintNo(complaintSearchQueryModel.ComplaintNo);
            return Ok(lstComplaints);
        }

        [HttpPost]
        [Route("SearchComplaintStatusByComplaintNo")]
        public IActionResult SearchComplaintStatusByComplaintNo(ComplaintSearchQueryModelComplaintNo complaintSearchQueryModel)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            List<COMPLAINT_STATUS> lstComplaints = modelComplaintRepository.GetPendingComplaintNo(complaintSearchQueryModel.ComplaintNo);
            return Ok(lstComplaints);
        }

        [HttpPost]
        [Route("SearchComplaintStatusByKNo")]
        public IActionResult SearchComplaintStatusByKNo(ComplaintSearchQueryModelKNO complaintSearchQueryModel)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            List<COMPLAINT_STATUS> lstComplaints = modelComplaintRepository.GetPendingComplaintNoByKNO(complaintSearchQueryModel.KNO);
            return Ok(lstComplaints);
        }



        [HttpPost]
        [Route("GetOfficeList")]
        public IActionResult GetOfficeList()
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            List<ModelOfficeCode> lst;
            lst = modelComplaintRepository.GetOfficeList();
            return Ok(lst);
        }

        [HttpPost]
        [Route("CheckPowerOutage")]
        public IActionResult CheckPowerOutage(ModelKNO KNo)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            List<ModelPowerOutage> lst;
            lst = modelComplaintRepository.CheckPowerOutageList(KNo);
            return Ok(lst);
        }

        [HttpPost]
        [Route("GetComplaintTypeList")]
        public IActionResult GetComplaintTypeList(string officeid)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);

            List<ModelComplaintType> obj;
            obj = modelComplaintRepository.GetComplaintTypeList(officeid);
            return Ok(obj);
        }

        [HttpPost]
        [Route("GetComplaintType")]
        public IActionResult GetComplaintType()
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);

            List<ModelComplaintTypeList> obj;
            obj = modelComplaintRepository.GetComplaintTypeDetail();
            return Ok(obj);
        }

        [HttpPost]
        [Route("GetComplaintClosingActions")]
        public IActionResult GetComplaintClosingActions()
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);

            List<ModelComplaintOutageTypeList> obj;
            obj = modelComplaintRepository.GetComplaintClosingActions();
            return Ok(obj);
        }

        [HttpPost]
        [Route("GetComplaintSource")]
        public IActionResult GetComplaintSource()
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);

            List<ModelComplaintSourceList> obj;
            obj = modelComplaintRepository.GetComplaintSourceDetail();
            return Ok(obj);
        }

        [HttpPost]
        [Route("CheckMobileAvailable")]
        public IActionResult CheckMobileAvailable(ModelMobile mobileno)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);

            List<KnoList> obj;
            obj = modelComplaintRepository.CheckMobileAvailableDetail(mobileno);
            return Ok(obj);
        }

        [HttpPost]
        [Route("GetSubComplaintTypeList")]
        public IActionResult GetSubComplaintTypeList(ComplaintTypeIDModel ComplaintTypeId)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);

            List<ModelComplaintTypeAPI> obj;
            obj = modelComplaintRepository.GetSubComplaintTypeList(ComplaintTypeId.ComplaintTypeId);
            return Ok(obj);
        }

        [HttpPost]
        [Route("Add_KNO")]
        public async Task<IActionResult> Add_KNO(KNOMODEL KnoDetail)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            int retStatus = await modelComplaintRepository.AddKNO(KnoDetail);
            if (retStatus ==1)
            {
                returnStatus.response = 1;
                returnStatus.status = "Kno Has been added Successfully";
                return Ok(returnStatus);
            }
            else if (retStatus == 2)
            {
                returnStatus.response = 1;
                returnStatus.status = "Invalid Kno";
                return Ok(returnStatus);
            }

            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Adding Kno or KNo Aready mapped with another User";
                return BadRequest(returnStatus);
            }

        }

        [HttpPost]
        [Route("ConsumerStatus")]
        public async Task<IActionResult> ConsumerStatus(ModelKNO KnoDetail)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            int retStatus = await modelComplaintRepository.ConsumerStatusCheck(KnoDetail);
            if (retStatus == 1)
            {
                returnStatus.response = 1;
                returnStatus.status = "Kno Exists";
                return Ok(returnStatus);
            }
            else if (retStatus == 0)
            {
                returnStatus.response = 0;
                returnStatus.status = "Invalid Kno";
                return Ok(returnStatus);
            }

            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Checking Kno";
                return BadRequest(returnStatus);
            }

        }


        [HttpPost]
        [Route("ListKNO")]
        public IActionResult ListKNO(KNOMODEL KnoDetail)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);

            List<KNOMODEL> obj;
            obj = modelComplaintRepository.ListKNO(KnoDetail.userid);
            return Ok(obj);
        }
        [HttpPost]
        [Route("UpdateDetail")]
        public async Task<IActionResult> UpdateDetail(UserDetailModel UserDetail)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            int retStatus = await modelComplaintRepository.UpdateDetail(UserDetail);
            if (retStatus > 0)
            {
                returnStatus.response = 1;
                returnStatus.status = "Detail Updated Successfully";
                return Ok(returnStatus);
            }

            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Updating Details";
                return BadRequest(returnStatus);
            }

        }
        [HttpPost]
        [Route("GetDetail")]
        public IActionResult GetDetail(KNOMODEL UserDetail)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);

            List<ModelUser> obj;
            obj = modelComplaintRepository.GetDetail(UserDetail.userid);
            return Ok(obj);
        }

        [HttpPost]
        [Route("GetKNODetail")]
        public IActionResult GetKNODetail(KNOMODEL UserDetail)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);

            List<COMPLAINT> obj;
            obj = modelComplaintRepository.GetKNODetailS(UserDetail.kno);
            return Ok(obj);
        }
        [HttpPost]
        [Route("GetFRTWiseComplaint")]
        public IActionResult GetFRTWiseComplaint(FRTWiseComplaintModel frtWiseComplaintModel)
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            List<COMPLAINT_SEARCH> lstComplaints = modelComplaintRepository.GetPendingComplaintFRTWise(frtWiseComplaintModel.OfficeId);
            return Ok(lstComplaints);
        }

        [HttpPost]
        [Route("GetComplaintCurrentStatusList")]
        public IActionResult GetFRTWiseComplaint()
        {
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            List<ComplaintCurrentStatusList> lstComplaints = modelComplaintRepository.GetComplaintCurrentStatus_List();
            return Ok(lstComplaints);
        }

        [HttpPost]
        [Route("SaveRemark")]
        public async Task<IActionResult> SaveRemark(RemarkModel modelRemark)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            Int64 retStatus = 0;
            string msg = string.Empty;
            string fileName = string.Empty;
            if (modelRemark.Image != "")
            {
                byte[] imageBytes = Convert.FromBase64String(modelRemark.Image);
                fileName = modelRemark.ComplaintNo + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".png";
                //string filePath = Path.GetFullPath(Path.Combine(Environment.WebRootPath, "Signature",fileName));
                string filePath = Path.Combine("Signature", fileName);

                // Save the byte array as a PNG file
                System.IO.File.WriteAllBytes(filePath, imageBytes);
            }
            retStatus = await modelComplaintRepository.SaveRemark(modelRemark, fileName);
            if (retStatus > 0)
            {
                returnStatus.response = 1;
                returnStatus.status = "Remark has been saved";
                return Ok(returnStatus);
            }

            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Saving Remark";
                return BadRequest(returnStatus);
            }


        }

        [HttpPost]
        [Route("CloseComplaint")]
        public async Task<IActionResult> CloseComplaint(CloseComplaintModel modelcloseComplaint)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            Int64 retStatus = 0;
            retStatus = await modelComplaintRepository.CloseComplaint(modelcloseComplaint);
            if (retStatus > 0)
            {
                returnStatus.response = 1;
                returnStatus.status = "Complaint Closed Successfully";
                return Ok(returnStatus);
            }

            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Closing Complaint";
                return BadRequest(returnStatus);
            }


        }

        [HttpPost]
        [Route("CheckUserAvailability")]
        public async Task<IActionResult> CheckUserAvailabile(CheckUserAvailableModel modelUser)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            Int64 retStatus = 0;
            string msg = string.Empty;
            retStatus = await modelComplaintRepository.CheckUser(modelUser);
            if (retStatus > 0)
            {
                returnStatus.response = 1;
                returnStatus.status = "User Already Exist";
                return Ok(returnStatus);
            }
            else if (retStatus == 0)
            {
                returnStatus.response = 1;
                returnStatus.status = "User Does Not Exist";
                return Ok(returnStatus);
            }

            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Exception";
                return BadRequest(returnStatus);
            }


        }

        [HttpPost]
        [Route("SignUP")]
        public async Task<IActionResult> SignUP(SignUPModel UserDetail)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            int retStatus = await modelComplaintRepository.AddUser(UserDetail);
            if (retStatus == 1)
            {
                returnStatus.response = 1;
                returnStatus.status = "User has been Successfully";
                return Ok(returnStatus);
            }
            else if (retStatus == 0)
            {
                returnStatus.response = 0;
                returnStatus.status = "User Name already exist";
                return Ok(returnStatus);
            }

            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Registeration Please try again";
                return BadRequest(returnStatus);
            }

        }

        [HttpPost]
        [Route("CallDetails")]
        public async Task<IActionResult> CallDetails(CallDetailModel modelRemark)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            Int64 retStatus = 0;
            string msg = string.Empty;
            retStatus = await modelComplaintRepository.SaveCallDetail(modelRemark);
            if (retStatus > 0)
            {
                returnStatus.response = 1;
                returnStatus.status = "Detail has been saved";
                return Ok(returnStatus);
            }

            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Saving Detail";
                return BadRequest(returnStatus);
            }


        }



        [HttpPost]
        [Route("SendSms")]
        public async Task<IActionResult> SendSms(SMSModel smsmodel)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            string retStatus = "0";
            string msg = string.Empty;
            retStatus = await modelComplaintRepository.SendSmsRep(smsmodel);
            if (retStatus == "0")
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Sending SMS";
                return BadRequest(returnStatus);
                
            }

            else
            {
                returnStatus.response = 1;
                returnStatus.status = retStatus;
                return Ok(returnStatus);
            }


        }

    }
    
}
