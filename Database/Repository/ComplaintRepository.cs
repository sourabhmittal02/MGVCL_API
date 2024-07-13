using Azure;
using CallCenterCoreAPI.ExternalAPI.TextSmsAPI;
using CallCenterCoreAPI.Models;
using CallCenterCoreAPI.Models.QueryModel;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace CallCenterCoreAPI.Database.Repository
{
    public class ComplaintRepository
    {
        private readonly ILogger<ComplaintRepository> _logger;
        private string conn = AppSettingsHelper.Setting(Key: "ConnectionStrings:DevConn");

        private string MGVCLApiURL = AppSettingsHelper.Setting(Key: "MGVCL_API:ApiURL");
        private string MGVCLsecret_key = AppSettingsHelper.Setting(Key: "MGVCL_API:secret_key");
        private string MGVCLtoken = AppSettingsHelper.Setting(Key: "MGVCL_API:token");
        public ComplaintRepository(ILogger<ComplaintRepository> logger)
        {
            _logger = logger;
        }

        #region SaveComplaint
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="modelComplaint"></param>
        /// <returns></returns>
        public async Task<Int64> SaveComplaint(COMPLAINT modelComplaint)
        {
            Int64 retStatus = 0;
            string retMsg = String.Empty; ;
            COMPLAINT obj = new COMPLAINT();
            obj = modelComplaint;

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


            SqlParameter parmretComplaint_no = new SqlParameter();
            parmretComplaint_no.ParameterName = "@retComplaint_no";
            parmretComplaint_no.DbType = DbType.Int64;
            parmretComplaint_no.Size = 8;
            parmretComplaint_no.Direction = ParameterDirection.Output;
            SqlParameter[] param ={
                    new SqlParameter("@OFFICE_CODE",modelComplaint.OFFICE_CODE),
                    new SqlParameter("@COMPLAINT_TYPE",modelComplaint.ComplaintTypeId),
                    new SqlParameter("@COMPLAINT_SOURCE_ID",modelComplaint.sourceId),//modelComplaint.com),
                    new SqlParameter("@NAME",modelComplaint.NAME),
                    new SqlParameter("@FATHER_NAME",modelComplaint.FATHER_NAME),
                    new SqlParameter("@KNO",modelComplaint.KNO),
                    new SqlParameter("@LANDLINE_NO",modelComplaint.LANDLINE_NO),
                    new SqlParameter("@MOBILE_NO",modelComplaint.MOBILE_NO),
                    new SqlParameter("@ALTERNATE_MOBILE_NO",modelComplaint.ALTERNATE_MOBILE_NO),
                    new SqlParameter("@EMAIL",modelComplaint.EMAIL),
                    new SqlParameter("@ACCOUNT_NO",modelComplaint.ACCOUNT_NO),
                    new SqlParameter("@ADDRESS1",modelComplaint.ADDRESS1),
                    new SqlParameter("@ADDRESS2",modelComplaint.ADDRESS2),
                    new SqlParameter("@ADDRESS3",modelComplaint.ADDRESS3),

                    new SqlParameter("@LANDMARK",modelComplaint.LANDMARK),
                    new SqlParameter("@CONSUMER_STATUS",modelComplaint.CONSUMER_STATUS),
                    new SqlParameter("@FEEDER_NAME",modelComplaint.FEEDER_NAME),
                    new SqlParameter("@AREA_CODE",modelComplaint.AREA_CODE),
                    new SqlParameter("@REMARKS",modelComplaint.REMARKS),
                     new SqlParameter("@USER_ID",modelComplaint.UserId),
                    parmretStatus,parmretMsg,parmretComplaint_no};
            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "COMPLAINTS_REGISTER_API", param);

                if (param[20].Value != DBNull.Value)// status
                    retStatus = Convert.ToInt64(param[22].Value);
                if (retStatus > 0 && modelComplaint.MOBILE_NO.Length == 10)
                {
                    SqlParameter[] param1 ={
                    new SqlParameter("@COMPLAINT_TYPE",modelComplaint.ComplaintTypeId),
                    new SqlParameter("@SUB_COMPLAINT_TYPE",1),
                    new SqlParameter("@SourceID",modelComplaint.sourceId)};
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
                    try
                    {
                        string MGVCLCMSComplaintURL = MGVCLApiURL;
                        var client = new HttpClient();
                        var request = new HttpRequestMessage(HttpMethod.Post, MGVCLCMSComplaintURL);
                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(MGVCLsecret_key), "secret_key");
                        content.Add(new StringContent(MGVCLtoken), "token");
                        content.Add(new StringContent("LaunchComplaint"), "tag");
                        content.Add(new StringContent(retStatus.ToString()), "p_compl_number");
                        content.Add(new StringContent(modelComplaint.KNO), "cons_no");
                        content.Add(new StringContent(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), "reg_date");
                        content.Add(new StringContent(complaintType), "compl_category");
                        content.Add(new StringContent(subcomplainttype), "compl_subcategory");
                        content.Add(new StringContent(ComplaintSource), "complaint_source");
                        content.Add(new StringContent("Complaint Register"), "compl_Details");
                        content.Add(new StringContent(modelComplaint.MOBILE_NO), "consumer_mobile");
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        String s = await response.Content.ReadAsStringAsync();
                        SaveCMSResponse(retStatus.ToString(), s, "Complaint Register");
                    }
                    catch (Exception EX) { SaveCMSResponse(retStatus.ToString(), EX.ToString(), "Complaint Register"); }
                    _logger.LogInformation(modelComplaint.MOBILE_NO.ToString());
                    //ModelSmsAPI modelSmsAPI = new ModelSmsAPI();
                    //modelSmsAPI.To = "91" + modelComplaint.MOBILE_NO.ToString();
                    //modelSmsAPI.Smstext = "Dear Consumer,Your Complaint has been registered with complaint No. " + retStatus + " on Date: " + DateTime.Now.ToString("dd-MMM-yyyy") + " AVVNL";

                    //TextSmsAPI textSmsAPI = new TextSmsAPI();
                    //string response = await textSmsAPI.RegisterComplaintSMS(modelSmsAPI);
                    ////modelComplaint.SMS = modelSmsAPI.Smstext;
                    //_logger.LogInformation(response.ToString());

                    //PUSH_SMS_DETAIL_Consumer(modelComplaint, response, modelSmsAPI.Smstext);

                }
                else
                    retStatus = 0;
            }
                catch (Exception ex)
            {
                retStatus = -1;
            }
            return retStatus;

        }
        #endregion

        #region SaveComplaintAPI
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="modelComplaint"></param>
        /// <returns></returns>
        public async Task<Int64> SaveComplaintAPI(COMPLAINT_API modelComplaint)
        {
            Int64 retStatus = 0;
            string retMsg = String.Empty; ;
            COMPLAINT_API obj = new COMPLAINT_API();
            obj = modelComplaint;

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


            SqlParameter parmretComplaint_no = new SqlParameter();
            parmretComplaint_no.ParameterName = "@retComplaint_no";
            parmretComplaint_no.DbType = DbType.Int64;
            parmretComplaint_no.Size = 8;
            parmretComplaint_no.Direction = ParameterDirection.Output;
            SqlParameter[] param ={
                    new SqlParameter("@COMPLAINT_TYPE",modelComplaint.ComplaintTypeId),
                    new SqlParameter("@SUB_COMPLAINT_TYPE",modelComplaint.SubComplaintTypeId),
                    new SqlParameter("@COMPLAINT_SOURCE_ID",modelComplaint.sourceId),//modelComplaint.com),
                    new SqlParameter("@NAME",modelComplaint.NAME),
                    new SqlParameter("@KNO",modelComplaint.KNO),
                    new SqlParameter("@MOBILE_NO",modelComplaint.MOBILE_NO),
                    new SqlParameter("@ALTERNATE_MOBILE_NO",modelComplaint.ALTERNATE_MOBILE_NO),
                    new SqlParameter("@EMAIL",modelComplaint.EMAIL),
                    new SqlParameter("@ADDRESS1",modelComplaint.ADDRESS1),
                    new SqlParameter("@ADDRESS2",modelComplaint.ADDRESS2),
                    new SqlParameter("@ADDRESS3",modelComplaint.ADDRESS3),

                    new SqlParameter("@LANDMARK",modelComplaint.LANDMARK),
                     new SqlParameter("@USER_ID",modelComplaint.UserId),
                    parmretStatus,parmretMsg,parmretComplaint_no};
            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "COMPLAINTS_REGISTER_API1", param);

                if (param[13].Value != DBNull.Value)// status
                    retStatus = Convert.ToInt64(param[13].Value);
                if (retStatus > 0 && modelComplaint.MOBILE_NO.Length == 10)
                {

                    SqlParameter[] param1 ={
                    new SqlParameter("@COMPLAINT_TYPE",modelComplaint.ComplaintTypeId),
                    new SqlParameter("@SUB_COMPLAINT_TYPE",modelComplaint.SubComplaintTypeId),
                    new SqlParameter("@SourceID",modelComplaint.sourceId)};
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
                    try
                    {
                        string MGVCLCMSComplaintURL = MGVCLApiURL;
                        var client = new HttpClient();
                        var request = new HttpRequestMessage(HttpMethod.Post, MGVCLCMSComplaintURL);
                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(MGVCLsecret_key), "secret_key");
                        content.Add(new StringContent(MGVCLtoken), "token");
                        content.Add(new StringContent("LaunchComplaint"), "tag");
                        content.Add(new StringContent(retStatus.ToString()), "p_compl_number");
                        content.Add(new StringContent(modelComplaint.KNO), "cons_no");
                        content.Add(new StringContent(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), "reg_date");
                        content.Add(new StringContent(complaintType), "compl_category");
                        content.Add(new StringContent(subcomplainttype), "compl_subcategory");
                        content.Add(new StringContent(ComplaintSource), "complaint_source");
                        content.Add(new StringContent("Complaint Register"), "compl_Details");
                        content.Add(new StringContent(modelComplaint.MOBILE_NO), "consumer_mobile");
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        String s = await response.Content.ReadAsStringAsync();
                        SaveCMSResponse(retStatus.ToString(), s, "Complaint Register");
                    }
                    catch (Exception EX) { SaveCMSResponse(retStatus.ToString(), EX.ToString(), "Complaint Register"); }

                    _logger.LogInformation(modelComplaint.MOBILE_NO.ToString());
                    //ModelSmsAPI modelSmsAPI = new ModelSmsAPI();
                    //modelSmsAPI.To = modelComplaint.MOBILE_NO.ToString();
                    //modelSmsAPI.Smstext = "प्रिय उपभोक्ता, आपका शिकायत क्रमांक " + retStatus + " दिनांक " + DateTime.Now.ToString("dd-MMM-yyyy") + " है। विद्युत सम्बन्धित शिकायत एवं अन्य सुविधाओं के लिए https://bit.ly/JDVVNLCCC का प्रयोग करें। जोधपुर डिस्कॉम।";
                    //modelSmsAPI.Smstemplete = "1307171445679499387";
                    //TextSmsAPI textSmsAPI = new TextSmsAPI();
                    //string response = await textSmsAPI.RegisterComplaintSMS(modelSmsAPI);
                    ////modelComplaint.SMS = modelSmsAPI.Smstext;
                    //_logger.LogInformation(response.ToString());

                    //PUSH_SMS_DETAIL_ConsumerAPI(modelComplaint.MOBILE_NO, response, modelSmsAPI.Smstext);

                }
                else
                    retStatus = 0;
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }
            return retStatus;

        }
        #endregion
        #region SaveComplaintIntegrationAPI
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="modelComplaint"></param>
        /// <returns></returns>
        public async Task<Int64> SaveComplaintIntegrationAPI(COMPLAINT_Integration modelComplaint)
        {
            _logger.LogInformation("API Call");
            Int64 retStatus = 0;
            string retMsg = String.Empty; ;
            COMPLAINT_Integration obj = new COMPLAINT_Integration();
            obj = modelComplaint;

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


            SqlParameter parmretComplaint_no = new SqlParameter();
            parmretComplaint_no.ParameterName = "@retComplaint_no";
            parmretComplaint_no.DbType = DbType.Int64;
            parmretComplaint_no.Size = 8;
            parmretComplaint_no.Direction = ParameterDirection.Output;
            SqlParameter[] param ={
                    new SqlParameter("@OFFICECODE",modelComplaint.OfficeCode),
                    new SqlParameter("@COMPLAINT_TYPE",modelComplaint.ComplaintType),
                    new SqlParameter("@COMPLAINT_SOURCE",modelComplaint.sourceType),
                    new SqlParameter("@SUB_COMPLAINT_TYPE",modelComplaint.SubComplaintType),//modelComplaint.com),
                    new SqlParameter("@NAME",modelComplaint.NAME),
                    new SqlParameter("@KNO",modelComplaint.KNO),
                    new SqlParameter("@MOBILE_NO",modelComplaint.MOBILE_NO),
                    new SqlParameter("@EMAIL",modelComplaint.EMAIL),
                    new SqlParameter("@ADDRESS1",modelComplaint.ADDRESS1),
                    new SqlParameter("@ADDRESS2",modelComplaint.ADDRESS2),
                    new SqlParameter("@ADDRESS3",modelComplaint.ADDRESS3),

                    new SqlParameter("@LANDMARK",modelComplaint.LANDMARK),
                     new SqlParameter("@USER_ID",modelComplaint.UserId),
                     new SqlParameter("@REMARK",modelComplaint.Remark),
                    parmretStatus,parmretMsg,parmretComplaint_no};
            try
            {
                _logger.LogInformation("Before Call");
                _logger.LogInformation(conn);
                _logger.LogInformation(param.ToString());
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "COMPLAINTS_REGISTER_Integration", param);
                _logger.LogInformation("Agter Call");
                _logger.LogInformation(param[16].Value.ToString());
                if (param[16].Value != DBNull.Value)// status
                    retStatus = Convert.ToInt64(param[16].Value);
                if (retStatus > 0 && modelComplaint.MOBILE_NO.Length == 10)
                {
                    try
                    {
                        string MGVCLCMSComplaintURL = MGVCLApiURL;
                        var client = new HttpClient();
                        var request = new HttpRequestMessage(HttpMethod.Post, MGVCLCMSComplaintURL);
                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(MGVCLsecret_key), "secret_key");
                        content.Add(new StringContent(MGVCLtoken), "token");
                        content.Add(new StringContent("LaunchComplaint"), "tag");
                        content.Add(new StringContent(retStatus.ToString()), "p_compl_number");
                        content.Add(new StringContent(modelComplaint.KNO), "cons_no");
                        content.Add(new StringContent(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), "reg_date");
                        content.Add(new StringContent(modelComplaint.ComplaintType), "compl_category");
                        content.Add(new StringContent(modelComplaint.SubComplaintType), "compl_subcategory");
                        content.Add(new StringContent(modelComplaint.sourceType), "complaint_source");
                        content.Add(new StringContent(modelComplaint.Remark), "compl_Details");
                        content.Add(new StringContent(modelComplaint.MOBILE_NO), "consumer_mobile");
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        String s = await response.Content.ReadAsStringAsync();
                        SaveCMSResponse(retStatus.ToString(), s, "Complaint Register");
                    }
                    catch(Exception EX) { SaveCMSResponse(retStatus.ToString(), EX.ToString(), "Complaint Register"); } 
                    //ModelSmsAPI modelSmsAPI = new ModelSmsAPI();
                    //modelSmsAPI.To = "91" + modelComplaint.MOBILE_NO.ToString();
                    //modelSmsAPI.Smstext = "Dear Consumer,Your Complaint has been registered with complaint No. " + retStatus + " on Date: " + DateTime.Now.ToString("dd-MMM-yyyy") + " AVVNL";

                    //TextSmsAPI textSmsAPI = new TextSmsAPI();
                    //string response = await textSmsAPI.RegisterComplaintSMS(modelSmsAPI);
                    ////modelComplaint.SMS = modelSmsAPI.Smstext;
                    //_logger.LogInformation(response.ToString());

                    //PUSH_SMS_DETAIL_ConsumerAPI(modelComplaint.MOBILE_NO, response, modelSmsAPI.Smstext);

                }
                else
                    retStatus = 0;
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }
            return retStatus;

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
        public int PUSH_SMS_DETAIL_ConsumerAPI(string mobile_no, string response, string SMS)
        {
            int retStatus = 0;
            string retMsg = String.Empty; ;
            SqlParameter[] param =
                {
                new SqlParameter("@PHONE_NO",mobile_no),
                new SqlParameter("@TEXT_MEESAGE",SMS),
                new SqlParameter("@DELIVERY_RESPONSE",response),
                new SqlParameter("@REMARK","SMS SENT")};
            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "PUSH_SMS_DETAIL", param);
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }

            return retStatus;

        }

        #region SaveComplaintDetailIVR
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="modelComplaint"></param>
        /// <returns></returns>
        public async Task<Int64> SaveComplaintDetailIVR(COMPLAINTIVR modelComplaint)
        {
            Int64 retStatus = 0;
            string retMsg = String.Empty; ;
            COMPLAINTIVR obj = new COMPLAINTIVR();
            obj = modelComplaint;

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


            SqlParameter parmretComplaint_no = new SqlParameter();
            parmretComplaint_no.ParameterName = "@retComplaint_no";
            parmretComplaint_no.DbType = DbType.Int64;
            parmretComplaint_no.Size = 8;
            parmretComplaint_no.Direction = ParameterDirection.Output;
            SqlParameter[] param ={
                    new SqlParameter("@KNO",modelComplaint.KNO),
                    new SqlParameter("@MOBILE_NO",modelComplaint.MobileNo),
                    new SqlParameter("@COMPLAINT_TYPE",modelComplaint.Complaint_type),
                    parmretStatus,parmretMsg,parmretComplaint_no};
            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "COMPLAINTS_REGISTER_IVR", param);

                if (param[5].Value != DBNull.Value)// status
                    retStatus = Convert.ToInt64(param[5].Value);
                if (retStatus > 0 && modelComplaint.MobileNo.Length == 10)
                {
                    SqlParameter[] param1 ={
                    new SqlParameter("@COMPLAINT_TYPE",modelComplaint.Complaint_type),
                    new SqlParameter("@SUB_COMPLAINT_TYPE",1),
                    new SqlParameter("@SourceID",11)};
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
                    try
                    {
                        string MGVCLCMSComplaintURL = MGVCLApiURL;
                        var client = new HttpClient();
                        var request = new HttpRequestMessage(HttpMethod.Post, MGVCLCMSComplaintURL);
                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(MGVCLsecret_key), "secret_key");
                        content.Add(new StringContent(MGVCLtoken), "token");
                        content.Add(new StringContent("LaunchComplaint"), "tag");
                        content.Add(new StringContent(retStatus.ToString()), "p_compl_number");
                        content.Add(new StringContent(modelComplaint.KNO), "cons_no");
                        content.Add(new StringContent(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), "reg_date");
                        content.Add(new StringContent(complaintType), "compl_category");
                        content.Add(new StringContent(subcomplainttype), "compl_subcategory");
                        content.Add(new StringContent(ComplaintSource), "complaint_source");
                        content.Add(new StringContent("Complaint Register Through IVR"), "compl_Details");
                        content.Add(new StringContent(modelComplaint.MobileNo), "consumer_mobile");
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        String s = await response.Content.ReadAsStringAsync();
                        SaveCMSResponse(retStatus.ToString(), s, "Complaint Register");
                    }
                    catch (Exception EX) { SaveCMSResponse(retStatus.ToString(), EX.ToString(), "Complaint Register"); }
                    _logger.LogInformation(modelComplaint.MobileNo.ToString());
                    //ModelSmsAPI modelSmsAPI = new ModelSmsAPI();
                    //modelSmsAPI.To =  modelComplaint.MobileNo.ToString();
                    //modelSmsAPI.Smstext = "प्रिय उपभोक्ता, आपका शिकायत क्रमांक " + retStatus + " दिनांक " + DateTime.Now.ToString("dd-MMM-yyyy") + " है। विद्युत सम्बन्धित शिकायत एवं अन्य सुविधाओं के लिए \"\"VIDYUT SAATHI\"\" ऐप का प्रयोग करें। जोधपुर डिस्कॉम।";
                    //modelSmsAPI.Smstemplete = "1307160688860548923";
                    //TextSmsAPI textSmsAPI = new TextSmsAPI();
                    //string response = await textSmsAPI.RegisterComplaintSMS(modelSmsAPI);
                    ////modelComplaint.SMS = modelSmsAPI.Smstext;
                    //_logger.LogInformation(response.ToString());

                    //PUSH_SMS_DETAIL_ConsumerIVR(modelComplaint, response, modelSmsAPI.Smstext);

                }
                else
                    retStatus = 0;
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }
            return retStatus;

        }
        #endregion




        #region CheckUser
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="modelComplaint"></param>
        /// <returns></returns>
        public async Task<Int64> CheckUser(CheckUserAvailableModel modelUser)
        {
            Int64 retStatus = 0;
            string retMsg = String.Empty; ;
            CheckUserAvailableModel obj = new CheckUserAvailableModel();
            obj = modelUser;

            SqlParameter parmretStatus = new SqlParameter();
            parmretStatus.ParameterName = "@retStatus";
            parmretStatus.DbType = DbType.Int32;
            parmretStatus.Size = 8;
            parmretStatus.Direction = ParameterDirection.Output;

            SqlParameter[] param ={
                new SqlParameter("@USER_NAME",modelUser.UserName),
                    parmretStatus};


            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "CheckUserAvailability", param);

                if (param[1].Value != DBNull.Value)// status
                    retStatus = Convert.ToInt32(param[1].Value);
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
        #region SendSmsRep
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="modelComplaint"></param>
        /// <returns></returns>
        public async Task<string> SendSmsRep(SMSModel smsmodel)
        {
            string retStatus = "0";
            _logger.LogInformation(smsmodel.to.ToString());
            ModelSmsAPI modelSmsAPI = new ModelSmsAPI();
            modelSmsAPI.To = smsmodel.to.ToString();
            modelSmsAPI.Smstext = smsmodel.smsText;
            modelSmsAPI.Smstemplete = smsmodel.templateid;
            try
            {
                TextSmsAPI textSmsAPI = new TextSmsAPI();
                string response = await textSmsAPI.RegisterComplaintSMS(modelSmsAPI);
                //modelComplaint.SMS = modelSmsAPI.Smstext;
                _logger.LogInformation(response.ToString());
                PUSH_SMS_DETAIL_ConsumerAPI(modelSmsAPI.To, response, modelSmsAPI.Smstext);
                //UPDATE_SMS_DETAIL_Consumer(response, smsmodel.id);
                retStatus = response;
            }
            catch
            {
                retStatus = "0";
            }

            return retStatus;

        }

        public async Task<string> SendSmsRepEng(SMSModel smsmodel)
        {
            string retStatus = "0";
            _logger.LogInformation(smsmodel.to.ToString());
            ModelSmsAPI modelSmsAPI = new ModelSmsAPI();
            modelSmsAPI.To = smsmodel.to.ToString();
            modelSmsAPI.Smstext = smsmodel.smsText;
            modelSmsAPI.Smstemplete = smsmodel.templateid;
            try
            {
                TextSmsAPI textSmsAPI = new TextSmsAPI();
                string response = await textSmsAPI.RegisterComplaintSMSEng(modelSmsAPI);
                //modelComplaint.SMS = modelSmsAPI.Smstext;
                _logger.LogInformation(response.ToString());
                PUSH_SMS_DETAIL_ConsumerAPI(modelSmsAPI.To, response, modelSmsAPI.Smstext);
                //UPDATE_SMS_DETAIL_Consumer(response, smsmodel.id);
                retStatus = response;
            }
            catch
            {
                retStatus = "0";
            }

            return retStatus;

        }
        #endregion

        #region SaveRemark
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="modelComplaint"></param>
        /// <returns></returns>
        public async Task<Int64> SaveRemark(RemarkModel modelRemark, string imgName)
        {
            Int64 retStatus = 0;
            string retMsg = String.Empty; ;
            RemarkModel obj = new RemarkModel();
            obj = modelRemark;

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
        new SqlParameter("@COMPLAINT_NO",modelRemark.ComplaintNo),
            new SqlParameter("@REMARK",modelRemark.Remark),
            new SqlParameter("@USER_ID",modelRemark.UserID),
            new SqlParameter("@status",modelRemark.status),
            new SqlParameter("@sign_name",imgName),
            parmretStatus,parmretMsg};


            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "SAVE_REMARK_API", param);

                if (param[5].Value != DBNull.Value)// status
                    retStatus = Convert.ToInt32(param[5].Value);
                else
                    retStatus = 0;
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }



            return retStatus;

        }
        #endregion

        #region CloseComplaint
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="modelComplaint"></param>
        /// <returns></returns>
        public async Task<Int64> CloseComplaint(CloseComplaintModel modelcloseComplaint)
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
        new SqlParameter("@complaint_no",modelcloseComplaint.ComplaintNo),
            new SqlParameter("@REMARK",modelcloseComplaint.Remark),
            new SqlParameter("@USER_ID",modelcloseComplaint.UserID),
            new SqlParameter("@OUTAGE_TYPE",modelcloseComplaint.ClosingAction),
            parmretStatus,parmretMsg};


            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "Save_Complaint_Close_Api", param);

                if (param[4].Value != DBNull.Value)// status
                    retStatus = Convert.ToInt32(param[4].Value);
                else
                    retStatus = 0;
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }
            if (retStatus > 0)
            {
                try
                {
                    string MGVCLCMSComplaintURL = MGVCLApiURL;
                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, MGVCLCMSComplaintURL);
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(MGVCLsecret_key), "secret_key");
                    content.Add(new StringContent(MGVCLtoken), "token");
                    content.Add(new StringContent("update_ComplaintStatus"), "tag");
                    content.Add(new StringContent(modelcloseComplaint.ComplaintNo), "p_compl_number");
                    content.Add(new StringContent("Closed"), "compl_status");
                    content.Add(new StringContent(modelcloseComplaint.ClosingAction), "compl_action_reason");
                    content.Add(new StringContent(modelcloseComplaint.Remark), "compl_action_description");
                    content.Add(new StringContent(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), "compl_action_datetime");
                    request.Content = content;
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    String s = await response.Content.ReadAsStringAsync();
                    SaveCMSResponse(retStatus.ToString(), s, "Close Complaint");
                }
                catch (Exception EX) { SaveCMSResponse(retStatus.ToString(), EX.ToString(), "Close Complaint"); }
            }
        
            return retStatus;

        }
        #endregion

        #region SaveCallDetail
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="modelComplaint"></param>
        /// <returns></returns>
        public async Task<Int64> SaveCallDetail(CallDetailModel modelRemark)
        {
            Int64 retStatus = 0;
            string retMsg = String.Empty; ;
            CallDetailModel obj = new CallDetailModel();
            obj = modelRemark;

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
                new SqlParameter("@Date1",modelRemark.date),
                    new SqlParameter("@Total_Calls_Offered",modelRemark.Total_Calls_Offered),
                    new SqlParameter("@Total_Calls_Answered",modelRemark.Total_Calls_Answered),
                    new SqlParameter("@Short_Calls",modelRemark.Short_Calls),
                    new SqlParameter("@Ans_LessThan_5_Secs",modelRemark.Ans_LessThan_5_Secs),
                    new SqlParameter("@Ans_6to10_Secs",modelRemark.Ans_6to10_Secs),
                    new SqlParameter("@Ans_11to20_Secs",modelRemark.Ans_11to20_Secs),
                    new SqlParameter("@Ans_21to30_Secs",modelRemark.Ans_21to30_Secs),
                    new SqlParameter("@Ans_31to40_Secs",modelRemark.Ans_31to40_Secs),
                    new SqlParameter("@Ans_41to50_Secs",modelRemark.Ans_41to50_Secs),
                    new SqlParameter("@Ans_51to60_Secs",modelRemark.Ans_51to60_Secs),
                    new SqlParameter("@Ans_GreaterThan_60_Secs",modelRemark.Ans_GreaterThan_60_Secs),
                    new SqlParameter("@Total_Calls_Abandon",modelRemark.Total_Calls_Abandon),
                    new SqlParameter("@Aban_LessThan5_Sec",modelRemark.Aban_LessThan5_Sec),
                    new SqlParameter("@Aban_6to10_Secs",modelRemark.Aban_6to10_Secs),
                    new SqlParameter("@Aban_11to20_Secs",modelRemark.Aban_11to20_Secs),
                    new SqlParameter("@Aban_21to30_Secs",modelRemark.Aban_21to30_Secs),
                    new SqlParameter("@Aban_31to40_Secs",modelRemark.Aban_31to40_Secs),
                    new SqlParameter("@Aban_41to50_Secs",modelRemark.Aban_41to50_Secs),
                    new SqlParameter("@Aban_51to60_Secs",modelRemark.Aban_51to60_Secs),
                    new SqlParameter("@Aban_GreaterThan_60_Secs",modelRemark.Aban_GreaterThan_60_Secs),
                    new SqlParameter("@Failed",modelRemark.Failed),
                    new SqlParameter("@Total_Call_Handling_Time",modelRemark.Total_Call_Handling_Time),
                    new SqlParameter("@Total_Talk_Time",modelRemark.Total_Talk_Time),
                    new SqlParameter("@Average_Talk_Time",modelRemark.Average_Talk_Time),
                    new SqlParameter("@Average_Call_Handling_Time",modelRemark.Average_Call_Handling_Time),
                    new SqlParameter("@Calls_Held",modelRemark.Calls_Held),
                    new SqlParameter("@Calls_Queued",modelRemark.Calls_Queued),
                    new SqlParameter("@Calls_Queue_Time",modelRemark.Calls_Queue_Time),
                    new SqlParameter("@Avg_Queue_Time",modelRemark.Avg_Queue_Time),
                    new SqlParameter("@Service_per",modelRemark.Service_per),
                    new SqlParameter("@Aban_Per",modelRemark.Aban_Per),
                    new SqlParameter("@SLA_Per",modelRemark.SLA_Per),
                    new SqlParameter("@ABN_10_Sec",modelRemark.ABN_10_Sec),
                    new SqlParameter("@ABN_10_Sec_Per",modelRemark.ABN_10_Sec_Per),
                    new SqlParameter("@ABN_20_Sec",modelRemark.ABN_20_Sec),
                    new SqlParameter("@ABN_20_Sec_Per",modelRemark.ABN_20_Sec_Per),
                    new SqlParameter("@ABN_30_Sec",modelRemark.ABN_30_Sec),
                    new SqlParameter("@ABN_30_Sec_Per",modelRemark.ABN_30_Sec_Per),
                    new SqlParameter("@ABN_60_Sec",modelRemark.ABN_60_Sec),
                    new SqlParameter("@ABN_60_Sec_Per",modelRemark.ABN_60_Sec_Per),
                    new SqlParameter("@ABN_90_Sec",modelRemark.ABN_90_Sec),
                    new SqlParameter("@ABN_90_Sec_Per",modelRemark.ABN_90_Sec_Per),
                    new SqlParameter("@SLA_10_Sec",modelRemark.SLA_10_Sec),
                    new SqlParameter("@SLA_10_Sec_Per",modelRemark.SLA_10_Sec_Per),
                    new SqlParameter("@SLA_20_Sec",modelRemark.SLA_20_Sec),
                    new SqlParameter("@SLA_20_Sec_Per",modelRemark.SLA_20_Sec_Per),
                    new SqlParameter("@SLA_30_Sec",modelRemark.SLA_30_Sec),
                    new SqlParameter("@SLA_30_Sec_Per",modelRemark.SLA_30_Sec_Per),
                    new SqlParameter("@SLA_60_Sec",modelRemark.SLA_60_Sec),
                    new SqlParameter("@SLA_60_Sec_Per",modelRemark.SLA_60_Sec_Per),
                    new SqlParameter("@SLA_90_Sec",modelRemark.SLA_90_Sec),
                    new SqlParameter("@SLA_90_Sec_Per",modelRemark.SLA_90_Sec_Per),
                    new SqlParameter("@Total_Hold_Time",modelRemark.Total_Hold_Time),
                    new SqlParameter("@Avg_Hold_Time_of_Ans",modelRemark.Avg_Hold_Time_of_Ans),
                    new SqlParameter("@Total_Wrapup_Time",modelRemark.Total_Wrapup_Time),
                    new SqlParameter("@Avg_Wrapup_Time",modelRemark.Avg_Wrapup_Time),
                    //new SqlParameter("@Call_Type",modelRemark.Call_Type),
                    parmretStatus,parmretMsg};


            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "CALL_Queue_Abandon_Insert", param);

                if (param[57].Value != DBNull.Value)// status
                    retStatus = Convert.ToInt32(param[57].Value);
                else
                    retStatus = 0;
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }
            return retStatus;
        }
        #endregion
        public async Task<int> AddUser(SignUPModel UserDetail)
        {
            int retStatus = 0;
            SqlParameter parmretStatus = new SqlParameter();
            parmretStatus.ParameterName = "@retStatus";
            parmretStatus.DbType = DbType.Int32;
            parmretStatus.Size = 8;
            parmretStatus.Direction = ParameterDirection.Output;
            SqlParameter[] param ={
                    new SqlParameter("@USER_NAME",UserDetail.USER_NAME),
                    new SqlParameter("@PASSWORD",Utility.EncryptText(UserDetail.PASSWORD.Trim())),
                    new SqlParameter("@NAME",UserDetail.NAME),
                    new SqlParameter("@ADDRESS",UserDetail.ADDRESS),
                    new SqlParameter("@MOBILE_NO",UserDetail.MOBILE_NO),
                    new SqlParameter("@EMAIL_ID",UserDetail.EMAIL_ID),
                    parmretStatus
                    };
            SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "SignUpConsumer", param);
            if (param[6].Value != DBNull.Value)// status
                retStatus = Convert.ToInt32(param[6].Value);
            else
                retStatus = 2;
            return retStatus;
        }

        #region SearchComplaint
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="kno"></param>
        /// <returns></returns>
        public DataSet SearchComplaint(string kno,int complaintType)
        {
            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] param = { new SqlParameter("@kno", kno),
                    new SqlParameter("@ComplaintType",complaintType) };
                ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "Search_Complaint", param);
            }
            catch (Exception ex)
            {

            }
            return ds;
        }
        #endregion

        #region SearchComplaintInti
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="kno"></param>
        /// <returns></returns>
        public DataSet SearchComplaintInti(string kno, string complaintType)
        {
            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] param = { new SqlParameter("@kno", kno),
                    new SqlParameter("@ComplaintType",complaintType) };
                ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "Search_Complaint_INTIGRATION", param);
            }
            catch (Exception ex)
            {

            }
            return ds;
        }
        #endregion

        #region SearchComplaintIVR
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="kno"></param>
        /// <returns></returns>
        public DataSet SearchComplaintIVR(COMPLAINTIVR modelComplaint)
        {
            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] param = { new SqlParameter("@kno", modelComplaint.KNO),
                new SqlParameter("@Complaint_Type", modelComplaint.Complaint_type)};
                ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "Search_ComplaintIVR", param);
            }
            catch (Exception ex)
            {

            }
            return ds;
        }
        #endregion

        #region GetPreviousComplaintByKno
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="Kno"></param>
        /// <returns></returns>
        public List<COMPLAINT_SEARCH_API> GetPreviousComplaintByKno(string Kno)
        {
            List<COMPLAINT_SEARCH_API> obj = new List<COMPLAINT_SEARCH_API>();
            SqlParameter[] param ={
                    new SqlParameter("@KNO",Kno) };

            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SearchComplaintByKNo", param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new COMPLAINT_SEARCH_API
                    {
                        //Consumer Info
                        //SDO_CODE = Convert.ToString(dr["SDO_CODE"]),

                        OFFICE_CODE = Convert.ToInt64(dr["OFFICE_CODE"]),
                        ComplaintCategory = Convert.ToString(dr["COMPLAINT_TYPE"]),
                        ComplaintSubCategory = Convert.ToString(dr["SUB_COMPLAINT_TYPE"]),
                        ComplaintRegDate = Convert.ToString(dr["TIME_STAMP"]),
                        ComplaintDetails = Convert.ToString(dr["REMARKS"]),
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
                        ComplaintActionDescription = Convert.ToString(dr["ComplaintActionDescription"]),
                        ComplaintActionReason = Convert.ToString(dr["ComplaintActionReason"]),
                        ComplaintLastUpdateDateTime = Convert.ToString(dr["ComplaintLastUpdateDateTime"]),

                    }
                    );
            }
            return (obj);
        }
        #endregion

        #region GetPendingComplaintFRTWise
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="Kno"></param>
        /// <returns></returns>
        public List<COMPLAINT_SEARCH> GetPendingComplaintFRTWise(string offcieID)
        {
            List<COMPLAINT_SEARCH> obj = new List<COMPLAINT_SEARCH>();
            SqlParameter[] param ={
                    new SqlParameter("@offcieID",offcieID) };

            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SearchComplaintByFTR", param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new COMPLAINT_SEARCH
                    {
                        //Consumer Info
                        //SDO_CODE = Convert.ToString(dr["SDO_CODE"]),

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
                        Complaint_Date = Convert.ToString(dr["Complaint_date"]),

                    }
                    );
            }
            return (obj);
        }

        public List<ComplaintCurrentStatusList> GetComplaintCurrentStatus_List()
        {
            List<ComplaintCurrentStatusList> obj = new List<ComplaintCurrentStatusList>();

            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GET_COMPLAINT_CURRENT_STATUS_LIST");
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new ComplaintCurrentStatusList
                    {
                        //Consumer Info
                        //SDO_CODE = Convert.ToString(dr["SDO_CODE"]),

                        ID = Convert.ToInt32(dr["ID"]),
                        CURRENT_STATUS = Convert.ToString(dr["CURRENT_STATUS"]),

                    }
                    );
            }
            return (obj);
        }
        #endregion

        #region GetPreviousComplaintNo
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="complaintNo"></param>
        /// <returns></returns>
        public List<COMPLAINT_SEARCH_API> GetPreviousComplaintNo(string complaintNo)
        {
            List<COMPLAINT_SEARCH_API> obj = new List<COMPLAINT_SEARCH_API>();
            SqlParameter[] param ={
                    new SqlParameter("@Complaint_NO",complaintNo) };

            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "SearchComplaintByComplaintNo", param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new COMPLAINT_SEARCH_API
                    {
                        //Consumer Info
                        //SDO_CODE = Convert.ToString(dr["SDO_CODE"]),

                        OFFICE_CODE = Convert.ToInt64(dr["OFFICE_CODE"]),
                        ComplaintCategory = Convert.ToString(dr["COMPLAINT_TYPE"]),
                        ComplaintSubCategory = Convert.ToString(dr["SUB_COMPLAINT_TYPE"]),
                        ComplaintRegDate = Convert.ToString(dr["TIME_STAMP"]),
                        ComplaintDetails = Convert.ToString(dr["REMARKS"]),
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
                        ComplaintActionDescription = Convert.ToString(dr["ComplaintActionDescription"]),
                        ComplaintActionReason = Convert.ToString(dr["ComplaintActionReason"]),
                        ComplaintLastUpdateDateTime = Convert.ToString(dr["ComplaintLastUpdateDateTime"]),
                    }
                    );
            }
            return (obj);
        }
        #endregion

        #region GetPendingComplaintNo
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="complaintNo"></param>
        /// <returns></returns>
        public List<COMPLAINT_STATUS> GetPendingComplaintNo(string complaintNo)
        {
            List<COMPLAINT_STATUS> obj = new List<COMPLAINT_STATUS>();
            SqlParameter[] param ={
                    new SqlParameter("@Complaint_NO",complaintNo) };

            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetComplaintStatusIVR", param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new COMPLAINT_STATUS
                    {
                        //Consumer Info
                        //SDO_CODE = Convert.ToString(dr["SDO_CODE"]),

                        COMPLAINT_NO = Convert.ToString(dr["COMPLAINT_NO"]),
                        Complaint_Status = Convert.ToString(dr["Complaint_Status"]),
                    }
                    );
            }
            return (obj);
        }
        #endregion

        #region GetPendingComplaintNoByKNO
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="complaintNo"></param>
        /// <returns></returns>
        public List<COMPLAINT_STATUS> GetPendingComplaintNoByKNO(string KNO)
        {
            List<COMPLAINT_STATUS> obj = new List<COMPLAINT_STATUS>();
            SqlParameter[] param ={
                    new SqlParameter("@Kno",KNO) };

            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetComplaintStatusByKNOIVR", param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new COMPLAINT_STATUS
                    {
                        //Consumer Info
                        //SDO_CODE = Convert.ToString(dr["SDO_CODE"]),

                        COMPLAINT_NO = Convert.ToString(dr["COMPLAINT_NO"]),
                        Complaint_Status = Convert.ToString(dr["Complaint_Status"]),
                    }
                    );
            }
            return (obj);
        }
        #endregion

        #region GetOfficeList
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public List<ModelOfficeCode> GetOfficeList()
        {
            List<ModelOfficeCode> lstOfficeCode = new List<ModelOfficeCode>();
            ModelOfficeCode objBlank = new ModelOfficeCode();
            objBlank.OfficeId = "0";
            objBlank.OfficeCode = "Select Office Code";
            lstOfficeCode.Insert(0, objBlank);

            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetOfficeCode");

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                objBlank = new ModelOfficeCode();
                objBlank.OfficeCode = dr.ItemArray[0].ToString();
                objBlank.OfficeId = dr.ItemArray[1].ToString();
                lstOfficeCode.Add(objBlank);
            }
            return lstOfficeCode;
        }
        #endregion

        #region CheckPowerOutageList
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public List<ModelPowerOutage> CheckPowerOutageList(ModelKNO Kno)
        {
            List<ModelPowerOutage> PowerOutage = new List<ModelPowerOutage>();
            ModelPowerOutage objBlank = new ModelPowerOutage();
            SqlParameter[] param = { new SqlParameter("@KNO", Kno.KNO)};
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "CHECK_POWER_OUTAGE_IVR", param);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                objBlank = new ModelPowerOutage();
                objBlank.OFFICE_CODE = Convert.ToInt64(dr.ItemArray[0].ToString());
                objBlank.START_TIME = dr.ItemArray[1].ToString();

                objBlank.END_TIME = dr.ItemArray[2].ToString();
                objBlank.COLONIES = dr.ItemArray[3].ToString();
                objBlank.SHUT_DOWN_INFORMATION = dr.ItemArray[4].ToString();
                objBlank.INFORMATION_SOURCE = dr.ItemArray[5].ToString();
                PowerOutage.Add(objBlank);
            }
            return PowerOutage;
        }
        #endregion

        #region GetComplaintTypeList
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="OFFICE_ID"></param>
        /// <returns></returns>
        public List<ModelComplaintType> GetComplaintTypeList(string OFFICE_ID)
        {
            List<ModelComplaintType> obj = new List<ModelComplaintType>();
            SqlParameter[] param ={
                    new SqlParameter("@OFFICE_ID",OFFICE_ID)};
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetComplaintTypeAPI", param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new ModelComplaintType
                    {
                        ComplaintTypeId = Convert.ToInt32(dr["Id"]),
                        ComplaintType = Convert.ToString(dr["Complaint_Type"]),
                        ComplaintTileColor = Convert.ToString(dr["TileColor"]),
                        Status = Convert.ToBoolean(dr["IS_ACTIVE"]),
                        COMPLAINT_COUNT = Convert.ToString(dr["COMPLAINT_COUNT"]),
                    }
                    );
            }
            return (obj);
        }
        #endregion

        #region GetComplaintTypeDetail
        public List<ModelComplaintTypeList> GetComplaintTypeDetail()
        {
            List<ModelComplaintTypeList> obj = new List<ModelComplaintTypeList>();
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetComplaintTypeList");
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new ModelComplaintTypeList
                    {
                        ComplaintTypeId = Convert.ToInt32(dr["ID"]),
                        ComplaintType = Convert.ToString(dr["COMPLAINT_TYPE"]),
                    }
                    );
            }
            return (obj);
        }
        #endregion

        #region GetComplaintSourceDetail
        public List<ModelComplaintSourceList> GetComplaintSourceDetail()
        {
            List<ModelComplaintSourceList> obj = new List<ModelComplaintSourceList>();
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetComplaintSource_register");
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new ModelComplaintSourceList
                    {
                        ComplaintSourceId = Convert.ToInt32(dr["ID"]),
                        ComplaintSource = Convert.ToString(dr["SOURCE_NAME"]),
                    }
                    );
            }
            return (obj);
        }
        #endregion

        #region CheckMobileAvailableDetail
        public List<KnoList> CheckMobileAvailableDetail(ModelMobile mobileno)
        {
            List<KnoList> obj = new List<KnoList>();
            SqlParameter[] param =
                {
                new SqlParameter("@mobile_no",mobileno.MobileNo)};
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "CheckMobileNoRegister",param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new KnoList
                    {
                        Kno = Convert.ToString(dr["KNO"]),
                    }
                    );
            }
            return (obj);
        }
        #endregion


        #region GetSubComplaintTypeList
        /// <summary>
        /// Save Complaint
        /// </summary>
        /// <param name="ComplaintTypeId"></param>
        /// <returns></returns>
        public List<ModelComplaintTypeAPI> GetSubComplaintTypeList(int ComplaintTypeId)
        {
            List<ModelComplaintTypeAPI> obj = new List<ModelComplaintTypeAPI>();
            SqlParameter[] param ={
                    new SqlParameter("@ComplaintTypeId",ComplaintTypeId)};
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetSubComplaintByComplaintTypeAPI", param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new ModelComplaintTypeAPI
                    {
                        SubComplaintTypeId = Convert.ToInt32(dr["Id"]),
                        SubComplaintType = Convert.ToString(dr["SUB_COMPLAINT_TYPE"]),
                        ComplaintTypeId = Convert.ToInt32(dr["COMPLAINT_TYPE_ID"]),
                    }
                    );
            }
            return (obj);
        }
        #endregion


        public int PUSH_SMS_DETAIL_Consumer(COMPLAINT modelRemark, string response,string SMS)
        {
            int retStatus = 0;
            string retMsg = String.Empty; ;
            COMPLAINT obj = new COMPLAINT();
            obj = modelRemark;
            SqlParameter[] param =
                {
                new SqlParameter("@PHONE_NO",modelRemark.MOBILE_NO),
                new SqlParameter("@TEXT_MEESAGE",SMS),
                new SqlParameter("@DELIVERY_RESPONSE",response),
                new SqlParameter("@REMARK","SMS SENT")};
            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "PUSH_SMS_DETAIL", param);
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }

            return retStatus;

        }

        public int PUSH_SMS_DETAIL_ConsumerIVR(COMPLAINTIVR modelRemark, string response, string SMS)
        {
            int retStatus = 0;
            string retMsg = String.Empty; ;
            COMPLAINTIVR obj = new COMPLAINTIVR();
            obj = modelRemark;
            SqlParameter[] param =
                {
                new SqlParameter("@PHONE_NO",modelRemark.MobileNo),
                new SqlParameter("@TEXT_MEESAGE",SMS),
                new SqlParameter("@DELIVERY_RESPONSE",response),
                new SqlParameter("@REMARK","SMS SENT")};
            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "PUSH_SMS_DETAIL", param);
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }

            return retStatus;

        }

        public int UPDATE_SMS_DETAIL_Consumer(string response, string id)
        {
            int retStatus = 0;
            string retMsg = String.Empty; ;
            SqlParameter[] param =
                {
                new SqlParameter("@id",id),
                new SqlParameter("@DELIVERY_RESPONSE",response)};
            try
            {
                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "UPDATE_SMS_DETAIL", param);
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }

            return retStatus;

        }

        public async Task<int> AddKNO(KNOMODEL KnoDetail)
        {
            int retStatus = 0;
            SqlParameter parmretStatus = new SqlParameter();
            parmretStatus.ParameterName = "@retStatus";
            parmretStatus.DbType = DbType.Int32;
            parmretStatus.Size = 8;
            parmretStatus.Direction = ParameterDirection.Output;
            SqlParameter[] param ={
                    new SqlParameter("@USER_ID",KnoDetail.userid),
                    new SqlParameter("@KNO",KnoDetail.kno),
                    parmretStatus
                    };
            SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "Add_KNO", param);
            if (param[2].Value != DBNull.Value)// status
                retStatus = Convert.ToInt32(param[2].Value);
            else
                retStatus = 0;
            return retStatus;
        }

        public async Task<int> ConsumerStatusCheck(ModelKNO KnoDetail)
        {
            int retStatus = 0;
            SqlParameter parmretStatus = new SqlParameter();
            parmretStatus.ParameterName = "@Ret_Status";
            parmretStatus.DbType = DbType.Int32;
            parmretStatus.Size = 8;
            parmretStatus.Direction = ParameterDirection.Output;
            SqlParameter[] param ={
                    new SqlParameter("@KNO",KnoDetail.KNO),
                    parmretStatus
                    };
            SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "CheckConsumerStatus", param);
            if (param[1].Value != DBNull.Value)// status
                retStatus = Convert.ToInt32(param[1].Value);
            else
                retStatus = 2;
            return retStatus;
        }

        public List<KNOMODEL> ListKNO(long userid)
        {
            List<KNOMODEL> obj = new List<KNOMODEL>();
            SqlParameter[] param ={
                    new SqlParameter("@USER_ID",userid)};
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "ListKNO", param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new KNOMODEL
                    {
                        userid = userid,
                        kno = Convert.ToInt64(dr["KNO"]),
                    }
                    );
            }
            return (obj);
        }

        public async Task<int> UpdateDetail(UserDetailModel UserDetail)
        {
            int retStatus = 0;
            try
            {
                SqlParameter parmretStatus = new SqlParameter();
                parmretStatus.ParameterName = "@retStatus";
                parmretStatus.DbType = DbType.Int32;
                parmretStatus.Size = 8;
                parmretStatus.Direction = ParameterDirection.Output;
                long uid = Convert.ToInt64(UserDetail.User_Id);
                SqlParameter[] param ={
                    new SqlParameter("@User_ID",uid),
                    new SqlParameter("@Name",UserDetail.Name),
                    new SqlParameter("@Address",UserDetail.Address),
                    new SqlParameter("@Email",UserDetail.Email),
                    new SqlParameter("@Phone",Convert.ToInt64(UserDetail.Phone)),
                    parmretStatus
                    };

                SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "UpdateUsers", param);
                if (param[5].Value != DBNull.Value)// status
                    retStatus = Convert.ToInt32(param[5].Value);
                else
                    retStatus = 0;
            }
            catch (Exception ex)
            {
                retStatus = -1;
            }
            return retStatus;
        }

        public List<ModelUser> GetDetail(long userid)
        {
            List<ModelUser> obj = new List<ModelUser>();
            SqlParameter[] param ={
                    new SqlParameter("@USER_ID",userid)};
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetUsers", param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new ModelUser
                    {
                        User_id = Convert.ToInt32(dr["USER_ID"]),
                        User_Name = Convert.ToString(dr["USER_NAME"]),
                        Name = Convert.ToString(dr["NAME"]),
                        Role = Convert.ToString(dr["ROLE_NAME"]),
                        Mobile_NO = Convert.ToInt64(dr["MOBILE_NO"]),
                        Email = Convert.ToString(dr["EMAIL_ID"]),
                        Address = Convert.ToString(dr["ADDRESS"]),
                    }
                    );
            }
            return (obj);
        }

        public List<COMPLAINT> GetKNODetailS(long KNO)
        {
            List<COMPLAINT> obj = new List<COMPLAINT>();
            SqlParameter[] param ={
                    new SqlParameter("@KNO",KNO)};
            DataSet ds = SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetKnoDetails", param);
            //Bind Complaint generic list using dataRow     
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                obj.Add(

                    new COMPLAINT
                    {
                        OFFICE_CODE = Convert.ToInt32(dr["OFFICE_CODE"]),
                        NAME = Convert.ToString(dr["Name"]),
                        FATHER_NAME = Convert.ToString(dr["FatherName"]),
                        KNO = Convert.ToString(dr["KNO"]),
                        MOBILE_NO = Convert.ToString(dr["MOBILE_NO"]),
                        LANDLINE_NO = Convert.ToString(dr["LANDLINENO"]),
                        EMAIL = Convert.ToString(dr["EMAIL_ADDRESS"]),
                        ACCOUNT_NO = Convert.ToString(dr["ACCOUNT_NO"]),
                        ADDRESS1 = Convert.ToString(dr["ADDRESS1"]),
                        ADDRESS2 = Convert.ToString(dr["ADDRESS2"]),
                        ADDRESS3 = Convert.ToString(dr["ADDRESS3"]),
                        LANDMARK = Convert.ToString(dr["LANDMARK"]),
                        CONSUMER_STATUS = Convert.ToString(dr["SERVICE_STATUS"]),
                        FEEDER_NAME = Convert.ToString(dr["FEEDER_NAME"]),
                        AREA_CODE = Convert.ToString(dr["AREA_CODE"]),
                    }
                    );
            }
            return (obj);
        }

    }
}
