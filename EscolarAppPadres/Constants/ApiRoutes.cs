using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Constants
{
    public static class ApiRoutes
    {
        public const string BaseUrl = "https://apipereyra.clickme.mx/api/Pereyra/";
        public const string BaseUrlPrueba = "http://18.222.34.197:8017/";
        public const string PereyraLuxUrl = "https://lux.org.mx/Jesuitas_webServices/web/app.php";
        public const string PereyraIdcUrl = "https://www.idc.edu.mx/Jesuitas_webServices/web";

        public static class StudentLogin
        {
            public const string LoginStudent = "Login/loginStudent";
            public const string changePassword = "Login/changePassword";
            public const string RefreshToken = "Login/refreshToken";
            public const string Logout = "Login/logout";
            public const string Encrypt = "Login/encryptData";
            public const string Decrypt = "Login/decryptData";
        }

        public static class FatherLogin
        {
            public const string LoginFather = "Login/loginFather";
            public const string changePassword = "Login/changePassword";
            public const string RefreshToken = "Login/refreshToken";
            public const string Logout = "Login/logout";
            public const string Encrypt = "Login/encryptData";
            public const string Decrypt = "Login/decryptData";
        }

        public static class StudentNews
        {
            public const string GetNews = "News/getNews";
            public const string UpdateNews = "News/updateNews";
        }

        public static class StudentCalendar
        {
            public const string GetEventCalendar = "Calendar/getEventCalendar";
            public const string GetEventTypeCalendar = "Calendar/getEventTypeCalendar";
            public const string ApiCalendar = "/api/portalalumno/calendario";
        }

        public static class StudentSubjects
        {
            public const string GetStudentSubjects = "Tutor/GetStudentSubjects/{studentId}";
        }

        public static class StudentAbsences
        {
            public const string GetStudentAbsences = "Tutor/GetStudentAbsences/{studentId}";
        }

        public static class StudentGrades
        {
            public const string GetStudentGrades = "Tutor/GetStudentGrades/{studentId}";
            public const string GetStudentCriteriaGrades = "Tutor/GetStudentCriteriaGrades/{studentId}/{materiaId:int}/{periodoEvaluacionId:int}";
        }

        public static class StudentHomework
        {
            // "Student/GetStudentHomework/{profesorpormateriaplanestudiosid}";
            public const string GetStudentHomework = "Student/GetStudentHomework/{profesorpormateriaplanestudiosid}";
        }

        public static class StudentReports
        {
            public const string GetStudentConductualReports = "Tutor/GetConductualReport/{studentId}";
        }

        public static class Payments
        {
            public const string GetPendingPayments = "Payments/PendingPayments";
            public const string GetStatusCharge = "Payments/GetStatusCharge";
            public const string CreateChargeMovil = "Payments/CreateChargeMovil";
        }
        public static class Services
        {
            public const string GetSchoolDirectory = "Services/GetSchoolDirectory";
        }
    }
}
