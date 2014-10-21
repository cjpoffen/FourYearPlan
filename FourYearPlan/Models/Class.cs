using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FourYearPlan.Models
{
    public class Class
    {
        private String name;
        private char semester;
        private Class[] prereqs;
        private int numOfPrereqs;
        private int n = 0; // tracks number of preqs entered so far;
        private bool completed = false; // false this course hasnt been taken, true it has
        private bool classInprogress = false;
        private int semesterTaken;
        private int earliestSemester;

        public Class(String Name)
        {
            name = Name;
        }

        public Class(String Name, char Semester, int NumOfPrereqs)
        {
            name = Name;
            semester = Semester;
            prereqs = new Class[NumOfPrereqs];
            numOfPrereqs = NumOfPrereqs;
        }

        public void addPrerequisite(Class c)
        {

            prereqs[n] = c;
            n++;
        }

        public String getName()
        {
            return name;
        }

        public int getSemesterTaken()
        {
            return semesterTaken;
        }

        public Class setSemester(int s)
        {
            semesterTaken = s;
            return this;
        }

        public bool canTake(int semester)
        {
            foreach (Class course in prereqs)
            {
                if (course.getSemesterTaken() > semester)
                {
                    return false;
                }
            }
            return true;
        }

        public void setEarliestSemester(int x)
        {
            earliestSemester = x;
        }

    }
}