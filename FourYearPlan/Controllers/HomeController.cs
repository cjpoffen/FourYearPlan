using FourYearPlan.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FourYearPlan.Controllers
{
    public class HomeController : Controller
    {
        private static DatabaseEntities db = new DatabaseEntities();
        private static Course[] courses = null;
        private static int numOfReq;
        private static string[] breakDown;
        private static string email; //username
        private static bool loggedIn = false;
        private static int cancel = 0;
        private static string canceledClass = "";
        private static bool admin = false;

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            canceledClass = "";
            var query = (from b in db.Users
                         where username == b.Email
                         select b).FirstOrDefault();
            if (query != null && query.Password.Equals(password))
            {
                loggedIn = true;
                email = username;
                if(query.Administrator == 1){
                    admin = true;
                    return Redirect("Admin");
                }
                if(query.Plan != null){
                    breakDown = query.Plan.Split(new char[] { '\n' });
                    checkCanceled();
                    return Redirect("Result");
                }
                return Redirect("FourYearPlan");
            }
            return View();
        }

        [HttpGet]
        public ActionResult Admin()
        {   
            var query = (from b in db.Course
                         select b);
            courses = query.ToArray();

            return View(courses);
        }

        [HttpPost]
        public ActionResult Admin(int value)
        {
            return View("EditCourse", courses[value]);
        }

        [HttpGet]
        public ActionResult EditCourse()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EditCourse(Course c)
        {
            updateCourse(c);
            return Redirect("Admin");
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string username, string password, string admin)
        {
           
            var exists = (from b in db.Users
                          where username.Trim() == b.Email
                          select b).Any();
            if(exists){
                //return error
            }
            else
            {
                Users u = new Users();
                u.Email = username;
                u.Password = password;
                if(admin == "admin"){
                    u.Administrator = 1;
                }
                else
                {
                    u.Administrator = 0;
                }
                db.Users.Add(u);
                db.SaveChanges();
                loggedIn = true;
                email = username;
                if(admin != null){
                    return Redirect("Admin");
                }
                return Redirect("FourYearPlan");
            }

            return View();
            
        }

        [HttpGet]
        public ActionResult FourYearPlan()
        {
            if(!loggedIn){
                return Redirect("Login");
            }
            var query = (from b in db.Course
                         select b);
            courses = query.ToArray();

            List<SelectListItem> req = new List<SelectListItem>();
            List<SelectListItem> tech = new List<SelectListItem>();
            List<SelectListItem> math = new List<SelectListItem>();
            List<SelectListItem> suppl = new List<SelectListItem>();
            List<SelectListItem> econ = new List<SelectListItem>();
            List<SelectListItem> se = new List<SelectListItem>();
            List<SelectListItem> gen = new List<SelectListItem>();

            foreach (Course c in courses)
            {
                if (c.Cancelled == 0)
                {
                    if (c.Type == "REQ")
                    {
                        req.Add(new SelectListItem { Text = c.name, Value = c.Id.ToString() });
                    }
                    else if (c.Type == "SE ELECT")
                    {
                        se.Add(new SelectListItem { Text = c.name, Value = c.Id.ToString() });
                    }
                    else if (c.Type == "TECH ELECT")
                    {
                        tech.Add(new SelectListItem { Text = c.name, Value = c.Id.ToString() });
                    }
                    else if (c.Type == "MATH ELECT")
                    {
                        math.Add(new SelectListItem { Text = c.name, Value = c.Id.ToString() });
                    }
                    else if (c.Type == "SUPPL ELECT")
                    {
                        suppl.Add(new SelectListItem { Text = c.name, Value = c.Id.ToString() });
                    }
                    else if (c.Type == "ECON")
                    {
                        econ.Add(new SelectListItem { Text = c.name, Value = c.Id.ToString() });
                    }
                    else if (c.Type == "GEN ED")
                    {
                        gen.Add(new SelectListItem { Text = c.name, Value = c.Id.ToString() });
                    }
                }
            }
            numOfReq = req.ToArray().Length;
            ViewBag.req = req;
            ViewBag.se = se;
            ViewBag.tech = tech;
            ViewBag.math = math;
            ViewBag.suppl = suppl;
            ViewBag.econ = econ;
            ViewBag.gen = gen;
            
            return View();
        }

        [HttpPost]
        public ActionResult FourYearPlan(string[] se, string[] tech, string[] math, string[] suppl, string[] econ, string[] gen)
        {
            Class[] masterList = new Class[courses.Length];
            int numOfClasses = numOfReq;
            numOfClasses += se != null ? se.Length : 0;
            numOfClasses += tech != null ? tech.Length : 0;
            numOfClasses += math != null ? math.Length : 0;
            numOfClasses += suppl != null ? suppl.Length : 0;
            numOfClasses += econ != null ? econ.Length : 0;
            numOfClasses += gen != null ? gen.Length : 0;

            
            Class[] list = new Class[numOfClasses];
            Course c;
            int x;
            for(int i = 0; i < courses.Length; i++){
                c = courses[i];
                masterList[i] = new Class(c.name, c.semester.Trim()[0], c.numOfPrerequisites);
            }
            for(int i = 0; i < courses.Length; i++)
            {
                c = courses[i];
                if(c.numOfPrerequisites != 0){
                    string[] indexs = c.prereqIndex.Split(new char[] { ',', ';' });// will create array where last index is ""
                    for (int j = 0; j < indexs.Length - 1; j++ )
                    {
                        x = Convert.ToInt32(indexs[j]);
                        masterList[i].addPrerequisite(masterList[x]);
                    }
                }
                
            }

            SoftwareEngineering softe = new SoftwareEngineering();
            softe.init(masterList);
            softe.setEcon(getClasses(masterList, econ)[0]);
            softe.setMath(getClasses(masterList, math)[0]);
            softe.setSe(getClasses(masterList, se));
            softe.setSuppl(getClasses(masterList, suppl));
            softe.setTech(getClasses(masterList, tech)[0]);
            softe.setGen(getClasses(masterList, gen));
            var plan = softe.getPlan();
            var classPerSemester = softe.getClassPerSemester();

            string ret = "";

            for (int i = 0; i < plan.Length; i++)
            {
                for (int j = 0; j < classPerSemester[i]; j++)
                {
                    ret += (plan[i][j].getName()) + " | ";
                }
                ret = ret.Substring(0, ret.Length - 3);
                ret += "\n";
            }
            var user = (from b in db.Users
                       where b.Email == email
                       select b).FirstOrDefault();
            user.Plan = ret;
            db.SaveChanges();
            breakDown = ret.Split(new char[] { '\n' });

            return Redirect("Result");
        }

        [HttpGet]
        public ActionResult Result()
        {
            if(!loggedIn){
                return Redirect("Login");
            }
            var query = (from b in db.Users
                         where b.Email == email
                         select b.Plan).FirstOrDefault();

            breakDown = query.Split(new char[] { '\n' });
            checkCanceled();

            ViewBag.first = breakDown[0];
            ViewBag.second = breakDown[1];
            ViewBag.third = breakDown[2];
            ViewBag.forth = breakDown[3];
            ViewBag.five = breakDown[4];
            ViewBag.six = breakDown[5];
            ViewBag.seven = breakDown[6];
            ViewBag.eight = breakDown[7];
            if(cancel == 1){
                ViewBag.cancel = "1";
                ViewBag.cl = canceledClass;
            }
            else
            {
                ViewBag.cancel = "0";
                ViewBag.cl = canceledClass;
            }
            
            return View();
        }

        [HttpGet]
        public ActionResult SharedPlan()
        {

            ViewBag.username = getUsernames();
            return View();
        }

        [HttpPost]
        public ActionResult Plan(string username)
        {
            var query = (from b in db.Users
                         where b.Email == username
                         select b.Plan).FirstOrDefault();

            var plan = query.Split(new char[] { '\n' });
            ViewBag.first = plan[0];
            ViewBag.second = plan[1];
            ViewBag.third = plan[2];
            ViewBag.forth = plan[3];
            ViewBag.five = plan[4];
            ViewBag.six = plan[5];
            ViewBag.seven = plan[6];
            ViewBag.eight = plan[7];
            return View();
        }

        [HttpGet]
        public ActionResult sharePlan()
        {
            return View();
        }

        [HttpPost]
        public ActionResult sharePlan(string username)
        {
            var user = (from b in db.Users
                        where b.Email == username
                        select b).FirstOrDefault();
            var id = (from b in db.Users
                        where b.Email == email
                        select b.Id).FirstOrDefault();

            if(user.SharedPlan == null || user.SharedPlan == ""){
                user.SharedPlan = id + ";";
            }
            else
            {
                user.SharedPlan = user.SharedPlan.Substring(0, user.SharedPlan.Length - 1) + id + ";";
            }
            db.SaveChanges();
            return Redirect("Result");
        }

        [HttpGet]
        public ActionResult EditPlan()
        {
            var plan = (from b in db.Users
                            where b.Email == email
                            select b.Plan).FirstOrDefault();
            string[] semester = plan.Split(new char[] { '\n' });
            ViewBag.FistSemester = semester[0].Split(new char[] { '|' });
            ViewBag.SecondSemester = semester[1].Split(new char[] { '|' });
            ViewBag.ThirdSemester = semester[2].Split(new char[] { '|' });
            ViewBag.ForthSemster = semester[3].Split(new char[] { '|' });
            ViewBag.FifthSemester = semester[4].Split(new char[] { '|' });
            ViewBag.SixthSemester = semester[5].Split(new char[] { '|' });
            ViewBag.SeventhSemester = semester[6].Split(new char[] { '|' });
            ViewBag.EighthSemester = semester[7].Split(new char[] { '|' });


            return View();
        }

        [HttpPost]
        public ActionResult EditPlan(string[] FistSemester, string[] SecondSemester, string[] ThirdSemester,
                                        string[] ForthSemster, string[] FifthSemester, string[] SixthSemester, 
                                            string[] SeventhSemester, string[] EighthSemester)
        {
            string ret = "";
            for (int i = 0; i < FistSemester.Length; i++)
            {
                ret += FistSemester[i] + " | ";
            }
            ret = ret.Substring(0, ret.Length - 3) + "\n";

            for (int i = 0; i < SecondSemester.Length; i++)
            {
                ret += SecondSemester[i] + " | ";
            }
            ret = ret.Substring(0, ret.Length - 3) + "\n";

            for (int i = 0; i < ThirdSemester.Length; i++)
            {
                ret += ThirdSemester[i] + " | ";
            }
            ret = ret.Substring(0, ret.Length - 3) + "\n";

            for (int i = 0; i < ForthSemster.Length; i++)
            {
                ret += ForthSemster[i] + " | ";
            }
            ret = ret.Substring(0, ret.Length - 3) + "\n";

            for (int i = 0; i < FifthSemester.Length; i++)
            {
                ret += FifthSemester[i] + " | ";
            }
            ret = ret.Substring(0, ret.Length - 3) + "\n";

            for (int i = 0; i < SixthSemester.Length; i++)
            {
                ret += SixthSemester[i] + " | ";
            }
            ret = ret.Substring(0, ret.Length - 3) + "\n";

            for (int i = 0; i < SeventhSemester.Length; i++)
            {
                ret += SeventhSemester[i] + " | ";
            }
            ret = ret.Substring(0, ret.Length - 3) + "\n";

            for (int i = 0; i < EighthSemester.Length; i++)
            {
                ret += EighthSemester[i] + " | ";
            }
            ret = ret.Substring(0, ret.Length - 3) + "\n";

            var query = (from b in db.Users
                         where b.Email == email
                         select b).FirstOrDefault();

            query.Plan = ret;
            db.SaveChanges();

            return Redirect("Result");
        }

        private Class[] getClasses(Class[] masterList, string[] index)
        {
            int x;
            Class[] classes = new Class[index.Length];
            for (int i = 0; i < index.Length; i++)
            {
                x = Convert.ToInt32(index[i]);
                classes[i] = masterList[x];
            }
            return classes;
        }

        private void checkCanceled()
        {
            var query = from b in db.Course
                        where b.Cancelled == 1
                        select b;
            if(query != null){
                cancel = 1;
                foreach(Course c in query)
                {
                    if(canceledClass == "")
                        canceledClass += c.name;
                    else
                        canceledClass += ", " + c.name;
                }
            }
        }

        private void updateCourse(Course updated)
        {
            var query = (from b in db.Course
                         where updated.Id == b.Id
                         select b).FirstOrDefault();

            query.name = updated.name;
            query.semester = updated.semester;
            query.Type = updated.Type;
            
            if(query.numOfPrerequisites != updated.numOfPrerequisites && updated.numOfPrerequisites != 0){
                query.numOfPrerequisites = updated.numOfPrerequisites;
                string indexs = "";
                string[] classes = updated.prerequisites.Split(new char[] { ',', ';' });// will create array where last index is ""
                for (int i = 0; i < classes.Length-1; i++)
                {
                    for (int j = 0; j < courses.Length; j++)
                    {
                        if (courses[j].name == classes[i])
                        {
                            indexs += courses[j].Id + ",";
                            break;
                        }
                    }
                }
                indexs = indexs.Substring(0, indexs.Length - 1) + ";";
                query.prerequisites = updated.prerequisites;
                query.prereqIndex = indexs;
            }
            if (updated.numOfPrerequisites == 0)
            {
                query.prerequisites = "";
                query.prereqIndex = ""; ;
            }

            query.numOfPrerequisites = updated.numOfPrerequisites;
            db.SaveChanges();
            
        }

        private string[] getUsernames()
        {
            var query = (from b in db.Users
                        where b.Email == email
                         select b).FirstOrDefault();

            if(query.SharedPlan != null && query.SharedPlan != ""){
                var index = query.SharedPlan.Split(new char[] { ',', ';' });
                      
                string[] us = new string[index.Length - 1];

                for (int i = 0; i < index.Length - 1; i++)
                {
                    int id = Int32.Parse(index[i]);
                    us[i] = (from b in db.Users
                             where b.Id == id
                             select b.Email).FirstOrDefault();
                }
                return us;
            }
            return null;


        }



    }
}
//rearrange http://www.blazonry.com/javascript/selmenu.php
//int count = 0;
//for(int i = 0; i < courses.Length; i++)
//{
//    if(courses[i].Type == "REQ"){
//        list[count] = masterList[i];
//        count++;
//    }
//}
//Class temp = list[10];
//list[10] = list[29];
//list[29] = temp;
//string[] ind = {""};
//if (se != null) ind = ind.Concat(se).ToArray();
//if (tech != null) ind = ind.Concat(tech).ToArray();
//if (math != null) ind = ind.Concat(math).ToArray();
//if (suppl != null) ind = ind.Concat(suppl).ToArray();
//if (econ != null) ind = ind.Concat(econ).ToArray();

//for (int i = 1; i < ind.Length; i++)
//{
//    x = Convert.ToInt32(ind[i]);
//    list[count] = masterList[x];
//    count++;
//}

//PlanAlgorithm pa = new PlanAlgorithm(list.Length, 6);
//pa.backtrack(new Class[list.Length + 1], 0, list, 1, 0, 'F');
//var result = pa.getMinSemester();