using CallCenterCoreAPI.Database.Repository;
using CallCenterCoreAPI.Models;
using CallCenterCoreAPI.Models.QueryModel;
using CallCenterCoreAPI.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
namespace CallCenterCoreAPI.Controllers
{
   
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
      
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        IConfiguration _configuration;


        public LoginController(ILogger<LoginController> logger, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("DoLogin")]
        public IActionResult DoLogin(UserRequestQueryModel modelUser)
        {
            ILogger<LoginRepository> modelLogger = _loggerFactory.CreateLogger<LoginRepository>();
            LoginRepository modelLoginRepository = new LoginRepository(modelLogger);
            UserViewModel userViewModels = new UserViewModel();
           
            userViewModels = modelLoginRepository.ValidateUser(modelUser);
            if (!string.IsNullOrEmpty(userViewModels.USER_NAME))
            {
                double expiryMins= string.IsNullOrEmpty(_configuration["Jwt:TokenValidityInMinutes"]) ? 5 : Convert.ToDouble(_configuration["Jwt:TokenValidityInMinutes"]);
                var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("ID", userViewModels.ID.ToString()),
                        new Claim("USER_NAME", userViewModels.USER_NAME),
                        new Claim("NAME", userViewModels.NAME),
                    };


                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,expires: DateTime.UtcNow.AddMinutes(expiryMins),signingCredentials: signIn);

                userViewModels.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation("Login success");
                return Ok(userViewModels);
            }
            else
            {
                _logger.LogInformation("Invalid credentials");
                
                return NotFound(-1);
            }
        }

        [HttpPost]
        [Route("GetToken")]
        public IActionResult GetToken(UserRequestQueryModel modelUser)
        {
            ILogger<LoginRepository> modelLogger = _loggerFactory.CreateLogger<LoginRepository>();
            LoginRepository modelLoginRepository = new LoginRepository(modelLogger);
            UserViewAPIModel userViewModels = new UserViewAPIModel();

            userViewModels = modelLoginRepository.ValidateUserAPI(modelUser);
            if (!string.IsNullOrEmpty(userViewModels.ID.ToString()) && userViewModels.ID!=0)
            {
                double expiryMins = string.IsNullOrEmpty(_configuration["Jwt:TokenValidityInMinutes"]) ? 5 : Convert.ToDouble(_configuration["Jwt:TokenValidityInMinutes"]);
                var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("ID", userViewModels.ID.ToString()),
                    };


                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims, expires: DateTime.UtcNow.AddMinutes(expiryMins), signingCredentials: signIn);

                userViewModels.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation("Login success");
                return Ok(userViewModels);
            }
            else
            {
                _logger.LogInformation("Invalid credentials");

                return NotFound(-1);
            }
        }

        [HttpPost]
        [Route("SearchComplaintListByMobileNo")]
        public IActionResult SearchComplaintListByMobileNo(ComplaintSearchQueryModelMobileNo complaintSearchQueryModel)
        {
            ILogger<LoginRepository> modelLogger = _loggerFactory.CreateLogger<LoginRepository>();
            LoginRepository modelloginRepository = new LoginRepository(modelLogger);
            List<COMPLAINT_SEARCH> lstComplaints = modelloginRepository.GetPendingComplaintNoByMobileNo(complaintSearchQueryModel.MobileNo);
            return Ok(lstComplaints);
        }

        [HttpPost]
        [Route("SendComplaintRegisterToCMS")]
        public async Task<IActionResult> SendComplaintRegisterToCMS(ModelComplaintSendToCMS modelComplaintSendTocms)
        {
            ILogger<LoginRepository> modelLogger = _loggerFactory.CreateLogger<LoginRepository>();
            LoginRepository modelloginRepository = new LoginRepository(modelLogger);
            String lstComplaints = await modelloginRepository.SendComplaintRegisterToCMS(modelComplaintSendTocms);
            return Ok(lstComplaints);
        }

        [HttpPost]
        [Route("SendComplaintRegisterNonConsumerToCMS")]
        public async Task<IActionResult> SendComplaintRegisterNonConsumerToCMS(ModelComplaintSendNonConsumerToCMS modelComplaintSendTocms)
        {
            ILogger<LoginRepository> modelLogger = _loggerFactory.CreateLogger<LoginRepository>();
            LoginRepository modelloginRepository = new LoginRepository(modelLogger);
            String lstComplaints = await modelloginRepository.SendComplaintRegisterNonConsumerToCMS(modelComplaintSendTocms);
            return Ok(lstComplaints);
        }

        [HttpPost]
        [Route("GetPaymentBillInfoFromCMS")]
        public async Task<IActionResult> GetPaymentBillInfoFromCMS(ModelCnsumerNo cons_no)
        {
            ILogger<LoginRepository> modelLogger = _loggerFactory.CreateLogger<LoginRepository>();
            LoginRepository modelloginRepository = new LoginRepository(modelLogger);
            String PaymentBillInfo = await modelloginRepository.GetPaymentBillInfoFromCMS(cons_no);
            return Ok(PaymentBillInfo);
        }

        [HttpPost]
        [Route("SendComplaintStatusToCMS")]
        public async Task<IActionResult> SendComplaintStatusToCMS(ModelComplaintSendStatusToCMS modelComplaintSendTocms)
        {
            ILogger<LoginRepository> modelLogger = _loggerFactory.CreateLogger<LoginRepository>();
            LoginRepository modelloginRepository = new LoginRepository(modelLogger);
            String lstComplaints = await modelloginRepository.SendComplaintStatusToCMS(modelComplaintSendTocms);
            return Ok(lstComplaints);
        }
        [HttpPost]
        [Route("SendComplaintTagChangeToCMS")]
        public async Task<IActionResult> SendComplaintTagChangeToCMS(ModelComplaintTagChangeToCMS modelComplaintSendTocms)
        {
            ILogger<LoginRepository> modelLogger = _loggerFactory.CreateLogger<LoginRepository>();
            LoginRepository modelloginRepository = new LoginRepository(modelLogger);
            String lstComplaints = await modelloginRepository.SendComplaintTagChangeToCMS(modelComplaintSendTocms);
            return Ok(lstComplaints);
        }

        [HttpPost]
        [Route("RegisterUser")]
        public async Task<IActionResult> RegisterUser(UserRegisterModel modelUser)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<LoginRepository> modelLogger = _loggerFactory.CreateLogger<LoginRepository>();
            LoginRepository modelComplaintRepository = new LoginRepository(modelLogger);
            Int64 retStatus = 0;
            string msg = string.Empty;
            
            retStatus = await modelComplaintRepository.SaveRegisterUser(modelUser);
            if (retStatus == 1)
            {
                returnStatus.response = 1;
                returnStatus.status = "User Successfully Registered";
                return Ok(returnStatus);
            }
            else if (retStatus == 0)
            {
                returnStatus.response = 1;
                returnStatus.status = "User Name Already Exist";
                return Ok(returnStatus);
            }

            else
            {
                returnStatus.response = 0;
                returnStatus.status = "Error in Saving User";
                return BadRequest(returnStatus);
            }


        }
        [HttpPost]
        [Route("SendSmsWeb")]
        public async Task<IActionResult> SendSmsWeb(SMSModel smsmodel, String Type)
        {
            ReturnStatusModel returnStatus = new ReturnStatusModel();
            ILogger<ComplaintRepository> modelLogger = _loggerFactory.CreateLogger<ComplaintRepository>();
            ComplaintRepository modelComplaintRepository = new ComplaintRepository(modelLogger);
            string retStatus = "0";
            string msg = string.Empty;
            if (Type == "E")
            {
                retStatus = await modelComplaintRepository.SendSmsRepEng(smsmodel);
            }
            else
            {
                retStatus = await modelComplaintRepository.SendSmsRep(smsmodel);
            }
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
