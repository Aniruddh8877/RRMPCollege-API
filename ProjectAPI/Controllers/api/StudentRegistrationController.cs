using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/StudentRegistration")]
    public class StudentRegistrationController : ApiController
    {
        [HttpPost]
        [Route("DashboardCount")]
        public ExpandoObject DashboardCount(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                RRMParaMedicalCollegeEntities dbContext = new RRMParaMedicalCollegeEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

                var today = DateTime.Today;
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                StudentRegistration model = JsonConvert.DeserializeObject<StudentRegistration>(decryptData);
                int StaffRollId = dbContext.StaffLoginRoles.First(x => x.StaffLoginId == model.CreatedBy).RoleId;

                    
                if (StaffRollId == 0)
                {
                    response.Message = "Invalid CreatedBy / StaffLoginId";
                    return response;
                }

                if (StaffRollId == 5) // Admin
                {
                    response.TotalRegistrations = dbContext.StudentRegistrations.Count();
                    response.TodayRegistrations = dbContext.StudentRegistrations.Count(x => x.RegsitrationDate >= today);
                }
                else // Institute login
                {
                    response.TotalRegistrations = dbContext.StudentRegistrations
                        .Count(x => x.CreatedBy == model.CreatedBy);

                    response.TodayRegistrations = dbContext.StudentRegistrations
                        .Count(x => x.CreatedBy == model.CreatedBy && x.RegsitrationDate >= today);
                }

                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }



        [HttpPost]
        [Route("StudentList")]
        public ExpandoObject StudentList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                RRMParaMedicalCollegeEntities dbContext = new RRMParaMedicalCollegeEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                StudentRegistration model = JsonConvert.DeserializeObject<StudentRegistration>(decryptData);


                var StaffRollId = dbContext.StaffLoginRoles.First(x => x.StaffLoginId == model.CreatedBy).RoleId;

                var AdminList = (from s in dbContext.StudentRegistrations
                                 join staff in dbContext.StaffLogins on s.StaffId equals staff.StaffId
                                 where (model.StaffId == 0||model.StaffId == s.StaffId)
                                 orderby s.StudentName
                                 select new
                                 {
                                     s.StudentId,
                                     s.StudentName,
                                     s.StaffId,
                                     s.Staff.StaffName,
                                     s.FatherName,
                                     s.MobileNo,
                                     s.AadharNo,
                                     s.WhatsAppNo,
                                     s.Education,
                                     s.StudentCode,
                                     s.RegsitrationDate,
                                     s.Status,
                                     s.CreatedBy,
                                     s.CreatedOn,
                                     s.UpdatedBy,
                                     s.UpdatedOn
                                 }).ToList();

                var InstituteList = (from s in dbContext.StudentRegistrations
                                     join staff in dbContext.StaffLogins on s.StaffId equals staff.StaffId
                                     where s.CreatedBy == model.CreatedBy
                                     orderby s.StudentName
                                     select new
                                     {
                                         s.StudentId,
                                         s.StudentName,
                                         s.StaffId,
                                         s.Staff.StaffName,
                                         s.FatherName,
                                         s.MobileNo,
                                         s.AadharNo,
                                         s.WhatsAppNo,
                                         s.Education,
                                         s.StudentCode,
                                         s.RegsitrationDate,
                                         s.Status,
                                         s.CreatedBy,
                                         s.CreatedOn,
                                         s.UpdatedBy,
                                         s.UpdatedOn
                                     }).ToList();

                if (StaffRollId == 5) // Admin
                {
                    response.StudentList = AdminList;
                    response.Count = AdminList.Count;
                }
                else // Institute
                {
                    response.StudentList = InstituteList;
                }

                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }


        [HttpPost]
        [Route("saveStudent")]
        public ExpandoObject saveStudent(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                RRMParaMedicalCollegeEntities dbContext = new RRMParaMedicalCollegeEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                StudentRegistration model = JsonConvert.DeserializeObject<StudentRegistration>(decryptData);

                StudentRegistration studentRegistration = null;
                if (model.StudentId > 0)
                {
                    studentRegistration = dbContext.StudentRegistrations.Where(x => x.StudentId == model.StudentId).First();
                    if (studentRegistration == null)
                    {
                        response.Message = "Party Details not found";
                        return response;
                    }
                    studentRegistration.StudentName = model.StudentName;
                    studentRegistration.StudentCode = model.StudentCode;
                    studentRegistration.StaffId = model.StaffId;
                    studentRegistration.MobileNo = model.MobileNo;
                    studentRegistration.WhatsAppNo = model.WhatsAppNo;
                    studentRegistration.AadharNo = model.AadharNo;
                    studentRegistration.FatherName = model.FatherName;
                    studentRegistration.Education = model.Education;
                    studentRegistration.RegsitrationDate = model.RegsitrationDate;
                    //studentRegistration.GSTNo = model.GSTNo;
                    //studentRegistration.PartyStatus = model.PartyStatus;
                    studentRegistration.UpdatedBy = model.UpdatedBy;
                    studentRegistration.UpdatedOn = DateTime.Now;
                }
                else
                {
                    studentRegistration = new StudentRegistration();

                    studentRegistration.StudentName = model.StudentName;
                    studentRegistration.StudentCode = AppData.GeneratePartyCode(dbContext);
                    studentRegistration.StaffId = model.StaffId;
                    studentRegistration.MobileNo = model.MobileNo;
                    studentRegistration.WhatsAppNo = model.WhatsAppNo;
                    studentRegistration.AadharNo = model.AadharNo;
                    studentRegistration.FatherName = model.FatherName;
                    studentRegistration.Education = model.Education;
                    studentRegistration.RegsitrationDate = DateTime.Now;
                    studentRegistration.Status = model.Status;
                    studentRegistration.CreatedBy = model.CreatedBy;
                    studentRegistration.CreatedOn = DateTime.Now;
                    dbContext.StudentRegistrations.Add(studentRegistration);
                }


                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }


        [HttpPost]
        [Route("deleteStudent")]
        public ExpandoObject deleteStudent(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                RRMParaMedicalCollegeEntities dbContext = new RRMParaMedicalCollegeEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                StudentRegistration model = JsonConvert.DeserializeObject<StudentRegistration>(decryptData);
                var student = dbContext.StudentRegistrations.Where(x => x.StudentId == model.StudentId).First();
                dbContext.StudentRegistrations.Remove(student);
                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {

                response.Message = ex.Message;
            }
            return response;
        }

    }
}
