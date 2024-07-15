using CallCenterCoreAPI.Database.Repository;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using CallCenterCoreAPI.Models.QueryModel;
using CallCenterCoreAPI.Models.ViewModel;
using CallCenterCoreAPI.Controllers;
using Serilog;
using Microsoft.Extensions.Logging;
using CallCenterCoreAPI.Filters;
using CallCenterCoreAPI.ExternalAPI.TextSmsAPI;
using CallCenterCoreAPI.Models;
using RestSharp;
using Newtonsoft.Json;
using System.DirectoryServices.Protocols;

namespace CallCenterCoreAPI.Database.Repository
{
    public class LoginRepository 
    {
   
        private readonly ILogger<LoginRepository> _logger;
        private string MGVCLApiURL = AppSettingsHelper.Setting(Key: "MGVCL_API:ApiURL");
        private string MGVCLsecret_key = AppSettingsHelper.Setting(Key: "MGVCL_API:secret_key");
        private string MGVCLtoken = AppSettingsHelper.Setting(Key: "MGVCL_API:token");
        private string MGVCLInfraApiURL = AppSettingsHelper.Setting(Key: "MGVCL_InfraAPI:ApiURL");
        private string MGVCLInfraAccessTokenURL = AppSettingsHelper.Setting(Key: "MGVCL_InfraAPI:GetTokenAPIURL");
        public LoginRepository(ILogger<LoginRepository> logger)
        {
            _logger = logger;
        }

        private string conn=AppSettingsHelper.Setting(Key: "ConnectionStrings:DevConn");


        public UserViewModel ValidateUser(UserRequestQueryModel user)
        {
            List<UserViewModel> userViewModel = new List<UserViewModel>();
            UserViewModel userViewModelReturn = new UserViewModel();  
            try
            {
                SqlParameter[] param ={new SqlParameter("@Username",user.LoginId.Trim()),new SqlParameter("@Password",Utility.EncryptText(user.Password.Trim()) )};
                DataSet dataSet = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "Validate_User_API_LOGIN", param);
                userViewModel = AppSettingsHelper.ToListof<UserViewModel>(dataSet.Tables[0]);
                userViewModelReturn = userViewModel[0];
                _logger.LogInformation(conn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

            }
            return userViewModelReturn;
        }

        public UserViewAPIModel ValidateUserAPI(UserRequestQueryModel user)
        {
            List<UserViewAPIModel> userViewModel = new List<UserViewAPIModel>();
            UserViewAPIModel userViewModelReturn = new UserViewAPIModel();
            try
            {
                SqlParameter[] param = { new SqlParameter("@Username", user.LoginId.Trim()), new SqlParameter("@Password", Utility.EncryptText(user.Password.Trim())) };
                DataSet dataSet = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "Validate_User_API", param);
                userViewModel = AppSettingsHelper.ToListof<UserViewAPIModel>(dataSet.Tables[0]);
                userViewModelReturn = userViewModel[0];
                _logger.LogInformation(conn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

            }
            return userViewModelReturn;
        }

        #region GetPendingComplaintNoByMobileNo
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="complaintNo"></param>
        /// <returns></returns>
        public List<COMPLAINT_SEARCH> GetPendingComplaintNoByMobileNo(string MobileNo)
        {
            List<COMPLAINT_SEARCH> obj = new List<COMPLAINT_SEARCH>();
            SqlParameter[] param ={
                    new SqlParameter("@MobileNo",MobileNo) };

            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SearchComplaintByMobileNo", param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new COMPLAINT_SEARCH
                    {
                        OFFICE_CODE = Convert.ToInt64(dr["OFFICE_CODE"]),
                        ComplaintType = Convert.ToString(dr["COMPLAINT_TYPE"]),
                        ComplaintNo = Convert.ToString(dr["COMPLAINT_NO"]),
                        NAME = Convert.ToString(dr["NAME"]),
                        FATHER_NAME = Convert.ToString(dr["FATHER_NAME"]),
                        KNO = Convert.ToString(dr["KNO"]),
                        LANDLINE_NO = Convert.ToString(dr["LANDLINE_NO"]),
                        MOBILE_NO = Convert.ToString(dr["MOBILE_NO"]),
                        ALTERNATE_MOBILE_NO = Convert.ToString(dr["ALTERNATE_MOBILE_NO"]),
                        source = Convert.ToString(dr["SOURCE_NAME"]),
                        ADDRESS = Convert.ToString(dr["ADDRESS"]),
                        Complaint_Status = Convert.ToString(dr["COMPLAINT_status"]),
                    }
                    );
            }
            return (obj);
        }
        #endregion
        


        #region SendComplaintRegisterToCMS
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="complaintNo"></param>
        /// <returns></returns>
        public async Task<String> SendComplaintRegisterToCMS(ModelComplaintSendToCMS modelComplaintSendTocms)
        {
            Int64 retStatus = 1;
            SqlParameter[] param1 ={
                    new SqlParameter("@COMPLAINT_TYPE",modelComplaintSendTocms.compl_category),
                    new SqlParameter("@SUB_COMPLAINT_TYPE",modelComplaintSendTocms.compl_subcategory),
                    new SqlParameter("@SourceID",modelComplaintSendTocms.complaint_source)};
            string complaintType = "";
            string subcomplainttype = "";
            string ComplaintSource = "";
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GET_COMPLAINT_TYPE_SUBTYPE", param1);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                complaintType = Convert.ToString(dr["COMPLAINT_TYPE"]);
                subcomplainttype = Convert.ToString(dr["SUB_COMPLAINT_TYPE"]);
                ComplaintSource = Convert.ToString(dr["COMPLAINT_SOURCE"]);
            }
            string MGVCLCMSComplaintURL = MGVCLApiURL;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, MGVCLCMSComplaintURL);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(MGVCLsecret_key), "secret_key");
            content.Add(new StringContent(MGVCLtoken), "token");
            content.Add(new StringContent("LaunchComplaint"), "tag");
            content.Add(new StringContent(modelComplaintSendTocms.compl_number), "p_compl_number");
            content.Add(new StringContent(modelComplaintSendTocms.cons_no), "cons_no");
            content.Add(new StringContent(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), "reg_date");
            content.Add(new StringContent(complaintType), "compl_category");
            content.Add(new StringContent(subcomplainttype), "compl_subcategory");
            content.Add(new StringContent(ComplaintSource), "complaint_source");
            content.Add(new StringContent(modelComplaintSendTocms.compl_Details), "compl_Details");
            content.Add(new StringContent(modelComplaintSendTocms.consumer_mobile), "consumer_mobile");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            String s= await response.Content.ReadAsStringAsync();
            SaveCMSResponse(modelComplaintSendTocms.compl_number, s,"Complaint Register");
            return (s);
        }
        #endregion

        #region SendComplaintRegisterToCMS
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="complaintNo"></param>
        /// <returns></returns>
        public async Task<String> SendComplaintRegisterNonConsumerToCMS(ModelComplaintSendNonConsumerToCMS modelComplaintSendTocms)
        {
            Int64 retStatus = 1;
            SqlParameter[] param1 ={
                    new SqlParameter("@COMPLAINT_TYPE",modelComplaintSendTocms.compl_category),
                    new SqlParameter("@SUB_COMPLAINT_TYPE",modelComplaintSendTocms.compl_subcategory),
                    new SqlParameter("@SourceID",modelComplaintSendTocms.complaint_source),
                    new SqlParameter("@OfficeID",modelComplaintSendTocms.OfficeCode)};
            string complaintType = "";
            string subcomplainttype = "";
            string ComplaintSource = "";
            string subdivision_code = "";
            string subdivision_name = "";
            string division_name = "";
            string circle_name = "";
            string village_name = "";
            string taluka_name = "";
            string district_name = "";
            string lt_loc_code = "";
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GET_COMPLAINT_DETAIL_FOR_NON_CONSUMER", param1);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                complaintType = Convert.ToString(dr["COMPLAINT_TYPE"]);
                subcomplainttype = Convert.ToString(dr["SUB_COMPLAINT_TYPE"]);
                ComplaintSource = Convert.ToString(dr["COMPLAINT_SOURCE"]);
                subdivision_code = Convert.ToString(dr["subdivision_code"]);
                subdivision_name = Convert.ToString(dr["subdivision_name"]);
                division_name = Convert.ToString(dr["division_name"]);
                circle_name = Convert.ToString(dr["circle_name"]);
                village_name = Convert.ToString(dr["village_name"]);
                taluka_name = Convert.ToString(dr["taluka_name"]);
                district_name = Convert.ToString(dr["district_name"]);
                lt_loc_code = Convert.ToString(dr["lt_loc_code"]);
            }
            string MGVCLCMSComplaintURL = MGVCLApiURL;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, MGVCLCMSComplaintURL);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(MGVCLsecret_key), "secret_key");
            content.Add(new StringContent(MGVCLtoken), "token");
            content.Add(new StringContent("LaunchComplaint_non_consumer"), "tag");
            content.Add(new StringContent(modelComplaintSendTocms.compl_number), "p_compl_number");
            content.Add(new StringContent(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), "reg_date");
            content.Add(new StringContent(complaintType), "compl_category");
            content.Add(new StringContent(subcomplainttype), "compl_subcategory");
            content.Add(new StringContent(ComplaintSource), "complaint_source");
            content.Add(new StringContent(modelComplaintSendTocms.compl_Details), "compl_Details");
            content.Add(new StringContent(modelComplaintSendTocms.consumer_mobile), "consumer_mobile");
            content.Add(new StringContent(modelComplaintSendTocms.email_id), "email_id");
            content.Add(new StringContent(modelComplaintSendTocms.consumer_name), "consumer_name");
            content.Add(new StringContent(modelComplaintSendTocms.address1), "address1");
            content.Add(new StringContent(modelComplaintSendTocms.address2), "address2");
            content.Add(new StringContent(subdivision_code), "subdivision_code");
            content.Add(new StringContent(subdivision_name), "subdivision_name");
            content.Add(new StringContent(division_name), "division_name");
            content.Add(new StringContent(circle_name), "circle_name");
            content.Add(new StringContent(village_name), "village_name");
            content.Add(new StringContent(taluka_name), "taluka_name");
            content.Add(new StringContent(district_name), "district_name");
            content.Add(new StringContent(lt_loc_code), "lt_loc_code");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            String s = await response.Content.ReadAsStringAsync();
            SaveCMSResponse(modelComplaintSendTocms.compl_number, s, "Complaint Register Non Consumer");
            return (s);
        }
        #endregion


        #region GetPaymentBillInfoFromCMS
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="complaintNo"></param>
        /// <returns></returns>
        public async Task<String> GetPaymentBillInfoFromCMS(ModelCnsumerNo cons_no)
        {
            Int64 retStatus = 1;
            
            string MGVCLCMSComplaintURL = MGVCLApiURL;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, MGVCLCMSComplaintURL);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(MGVCLsecret_key), "secret_key");
            content.Add(new StringContent(MGVCLtoken), "token");
            content.Add(new StringContent("get_payment_bill_info"), "tag");
            content.Add(new StringContent(cons_no.cons_no), "cons_no");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            String s = await response.Content.ReadAsStringAsync();
            return (s);
        }
        #endregion


        #region SendComplaintTagChangeToCMS
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="complaintNo"></param>
        /// <returns></returns>
        public async Task<String> SendComplaintTagChangeToCMS(ModelComplaintTagChangeToCMS modelComplaintSendTocms)
        {
            Int64 retStatus = 1;
            SqlParameter[] param1 ={
                    new SqlParameter("@COMPLAINT_TYPE",modelComplaintSendTocms.compl_category),
                    new SqlParameter("@SUB_COMPLAINT_TYPE",modelComplaintSendTocms.compl_subcategory),
                    new SqlParameter("@SourceID",1)};
            string complaintType = "";
            string subcomplainttype = "";
            string ComplaintSource = "";
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GET_COMPLAINT_TYPE_SUBTYPE", param1);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                complaintType = Convert.ToString(dr["COMPLAINT_TYPE"]);
                subcomplainttype = Convert.ToString(dr["SUB_COMPLAINT_TYPE"]);
                ComplaintSource = Convert.ToString(dr["COMPLAINT_SOURCE"]);
            }
            string MGVCLCMSComplaintURL = MGVCLApiURL;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, MGVCLCMSComplaintURL);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(MGVCLsecret_key), "secret_key");
            content.Add(new StringContent(MGVCLtoken), "token");
            content.Add(new StringContent("update_category_subcategory"), "tag");
            content.Add(new StringContent(modelComplaintSendTocms.compl_number), "p_compl_number");
            content.Add(new StringContent(complaintType), "compl_category");
            content.Add(new StringContent(subcomplainttype), "compl_subcategory");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            String s = await response.Content.ReadAsStringAsync();
            SaveCMSResponse(modelComplaintSendTocms.compl_number, s, "Complaint Tag Change");
            return (s);
        }
        #endregion

        #region SendComplaintRegisterToCMS
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="complaintNo"></param>
        /// <returns></returns>
        public async Task<String> SendComplaintStatusToCMS(ModelComplaintSendStatusToCMS modelComplaintSendTocms)
        {
            Int64 retStatus = 1;
            if(modelComplaintSendTocms.compl_status!= "In-Process" && modelComplaintSendTocms.compl_status!= "Closed")
            {
                modelComplaintSendTocms.compl_status = "In-Process";
            }
            string MGVCLCMSComplaintURL = MGVCLApiURL;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, MGVCLCMSComplaintURL);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(MGVCLsecret_key), "secret_key");
            content.Add(new StringContent(MGVCLtoken), "token");
            content.Add(new StringContent("update_ComplaintStatus"), "tag");
            content.Add(new StringContent(modelComplaintSendTocms.compl_number), "p_compl_number");
            content.Add(new StringContent(modelComplaintSendTocms.compl_status), "compl_status");
            content.Add(new StringContent(modelComplaintSendTocms.compl_action_reason), "compl_action_reason");
            content.Add(new StringContent(modelComplaintSendTocms.compl_action_description), "compl_action_description");
            content.Add(new StringContent(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), "compl_action_datetime");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            String s = await response.Content.ReadAsStringAsync();
            SaveCMSResponse(modelComplaintSendTocms.compl_number, s, modelComplaintSendTocms.compl_action_reason);
            return (s);
        }
        #endregion

        public int SaveCMSResponse(string ComplaintNo, string response, string Type)
        {
            int retStatus = 0;
            string retMsg = String.Empty; ;
            SqlParameter[] param =
                {
                new SqlParameter("@COMPLAINT_NO",ComplaintNo),
                new SqlParameter("@RESPONSE",response),
                new SqlParameter("@Type",Type)};
            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "SAVE_CMS_RESPONSE", param);
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }

            return retStatus;

        }
        #region SaveRegisterUser
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="modelComplaint"></param>
        /// <returns></returns>
        public async Task<Int64> SaveRegisterUser(UserRegisterModel modelUser)
        {
            Int64 retStatus = 0;
            string retMsg = String.Empty;

            SqlParameter parmretStatus = new SqlParameter();
            parmretStatus.ParameterName = "@retStatus";
            parmretStatus.DbType = DbType.Int32;
            parmretStatus.Size = 8;
            parmretStatus.Direction = ParameterDirection.Output;

            SqlParameter parmretMsg = new SqlParameter();
            parmretMsg.ParameterName = "@retMsg";
            parmretMsg.DbType = DbType.String;
            parmretMsg.Size = 8;
            parmretMsg.Direction = ParameterDirection.Output;

            SqlParameter[] param ={
                    new SqlParameter("@USER_NAME",modelUser.USER_NAME),
                    new SqlParameter("@PASSWORD",Utility.EncryptText(modelUser.PASSWORD)),
                    new SqlParameter("@NAME",modelUser.NAME),
                    new SqlParameter("@ADDRESS",modelUser.ADDRESS),
                    new SqlParameter("@MOBILE_NO",modelUser.MOBILE_NO),
                    new SqlParameter("@EMAIL_ID",modelUser.EMAIL_ID),
                    parmretStatus,parmretMsg};
            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "RegisterConsumer", param);

                if (param[6].Value != DBNull.Value)// status
                    retStatus = Convert.ToInt64(param[6].Value);
               
                else
                    retStatus = 2;
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }
            return retStatus;

        }
        #endregion



        #region CreateComplain 
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="complaintNo"></param>
        /// <returns></returns>
        public async Task<ModelHelpDeskResponse> PushHdTicketAPI(ModelHelpDesk  modelComplaintMgvcl)
        {
            ModelHelpDeskResponse apiResponse = new ModelHelpDeskResponse();
            var client = new RestClient(MGVCLInfraApiURL);
            var restRequest = new RestRequest();
            restRequest.Method = Method.POST;
            restRequest.AddHeader("Accept", "application/json");
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("Authorization", string.Format("Bearer {0}", modelComplaintMgvcl.accessToken));
            restRequest.AddJsonBody(modelComplaintMgvcl);
            var response = await client.ExecuteAsync(restRequest);
            {
                apiResponse = JsonConvert.DeserializeObject<ModelHelpDeskResponse>(response.Content);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return apiResponse;
            }
            else
            {
                return apiResponse;
            }
        }
        #endregion

        #region getAccessToken 
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="complaintNo"></param>
        /// <returns></returns>
        public async Task<ModelMgvcluserResponse> getAccessTokenAPI(ModelMgvcluser modelMgvcluser)
        {
            ModelMgvcluserResponse apiResponse = new ModelMgvcluserResponse();
            var client = new RestClient(MGVCLInfraAccessTokenURL);
            var restRequest = new RestRequest();
            restRequest.Method = Method.POST;
            restRequest.AddHeader("Accept", "application/json");
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddJsonBody(modelMgvcluser);
            var response = await client.ExecuteAsync(restRequest);
            {
                apiResponse = JsonConvert.DeserializeObject<ModelMgvcluserResponse>(response.Content);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return apiResponse;
            }
            else
            {
                return apiResponse;
            }
        }
        #endregion
    }



}
