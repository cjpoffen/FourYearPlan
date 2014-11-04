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
            if (courses == null)
            {
                var query = (from b in db.Course
                             select b);
                courses = query.ToArray();
            }
            return View(courses);
        }

        [HttpPost]
        public ActionResult EditCourse(int value)
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string username, string password)
        {
           
            var exists = (from b in db.Users
                          where username == b.Email
                          select b).Any();
            if(exists){
                //return error
            }
    
            Users u = new Users();
            u.Email = username;
            u.Password = password;
            db.Users.Add(u);
            db.SaveChanges();
            loggedIn = true;
            email = username;

            return Redirect("FourYearPlan");
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
        
    }
}

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