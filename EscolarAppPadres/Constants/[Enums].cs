using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Constants
{

    public enum AttendanceStatusEnum : short { Default = 0, Valid = 1, Canceled = 2, Justified = 3 }
    public enum AttendanceTypeEnum : short { Undefined = 0, Attendance = 1, Late = 2, Absence = 3 }
    public enum AppInstituteEnum : short { Undefined = 0, Lux = 1, Idec = 2 }
    public enum AppSectionEnum : short
    {
        Unknown = 0,
        StudentsAcademy = 3,
        StudentsAcademySubjects = 4,
        StudentsAcademyAttendance = 5,
        StudentsAcademyGrades = 6,
        StudentsEvents = 7,
        StudentsNews = 8,
        ParentsServices = 10,
        ParentsServicesPayments = 11,
        ParentsServicesDirectory = 12,
        ParentsServicesWorkshops = 13,
        ParentsAcademy = 15,
        ParentsAcademySubjects = 16,
        ParentsAcademyAttendance = 17,
        ParentsAcademyReports = 18,
        ParentsEvents = 19,
        ParentsNews = 20,
        PreKDailyReport = 22,
        PreKStock = 23,
        PreKDailyMenu = 24,
        PreKNews = 25,
        ParentsAcademyGrades = 26,
    }
    public enum AppTypeEnum : short { Unknown = 0, Students = 1, Parents = 2, PreK = 3, Transport = 4 }
    public enum BillStatusEnum : short { Undefined = 0, Pending = 1, Sent = 2, Canceled = 3, CanceledByError = 4, Resent = 5 }
    public enum EatTypeEnum : short { Undefined = 0, All = 1, Partial = 2, None = 3 }
    public enum EventTypeEnum : int { Undefined = 0, Event = 1, Holiday = 2, Test = 3, Homework = 4, Scholarship = 5, Admission = 6, Schedule = 7 }
    public enum HomeworkDeliverTypeEnum : short { Undefined = 0, Digital = 1, Physical = 2 }
    public enum MoodTypeEnum : short { Undefined = 0, Happy = 1, Sad = 2, Angry = 3 }
    public enum NewsTypeEnum : short { Undefined = 0, News = 1, DailyReport = 2, Stock = 3, DailyMenu = 4, Event = 5, Academy = 6 }
    public enum PaymentStatusEnum : byte { Undefined = 0, Pending = 1, Paid = 2, Canceled = 3, Approved = 4 }
    public enum PaymentTypeEnum : byte { Undefined = 0, Scholarship = 1, Special = 2, Others = 3 }
    public enum PooTypeEnum : short { Undefined = 0, Normal = 1, Diarrhea = 2, Constipated = 3 }
    public enum RegistryStatusEnum : byte { Undefined = 0, Preregistered = 1, Pending = 2, Registered = 3 }
    public enum RequestStatusEnum : byte { Success = 0, Warning = 1, Error = 2 }
    public enum SchoolLevelEnum : short
    {
        Undefined = 0,
        Preschool = 1,
        Elementary = 2,
        Middle = 3,
        High = 4
    }
    public enum SchoolGradeEnum : short
    {
        Undefined = 0,
        Preschool1 = 1, Preschool2 = 2, Preschool3 = 3,
        Elementary1 = 4, Elementary2 = 5, Elementary3 = 6,
        Elementary4 = 7, Elementary5 = 8, Elementary6 = 9,
        Middle1 = 10, Middle2 = 11, Middle3 = 12,
        High1 = 13, High2 = 14, High3 = 15, High4 = 16, High5 = 17, High6 = 18,
        PreK = 19
    }
    public enum StockTypeEnum : short { Undefined = 0, Stock = 1, Hygiene = 2 }
    public enum UserTypeEnum : short { Undefined = 0, Admin = 1, Teacher = 2, Student = 3, Parent = 4 }
}