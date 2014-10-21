using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FourYearPlan.Models
{
    public class SoftwareEngineering
    {
        Class[][] plan = new Class[8][];
        int[] semester = new int[8];

        public SoftwareEngineering()
        {
            for (int i = 0; i < plan.Length; i++)
            {
                plan[i] = new Class[6];
            }
        }
        
        //done??
        public void init(Class[] list)
        {
            //semester one
            plan[0][0] = list[0].setSemester(0);
            plan[0][1] = list[1].setSemester(0);
            plan[0][2] = list[2].setSemester(0);
            plan[0][3] = list[3].setSemester(0);
            plan[0][4] = list[4].setSemester(0);
            plan[0][5] = list[5].setSemester(0);
            semester[0] = 6;


            //semester two
            plan[1][0] = list[6].setSemester(1);
            plan[1][1] = list[7].setSemester(1);
            plan[1][2] = list[8].setSemester(1);
            plan[1][3] = list[44].setSemester(1);
            //plan[1][4] = list[4]; // econ
            semester[1] = 5;


            //semester three
            plan[2][0] = list[9].setSemester(2);
            //plan[2][1] = list[1]; suppl elective
            plan[2][2] = list[10].setSemester(2);
            plan[2][3] = list[11].setSemester(2);
            plan[2][4] = list[12].setSemester(2);
            semester[2] = 5;

            //semester four
            //plan[3][0] = list[0]; //Gen Ed
            plan[3][1] = list[13].setSemester(3);
            plan[3][2] = list[14].setSemester(3);
            //plan[3][3] = list[3]; //math 
            plan[3][4] = list[15].setSemester(3);
            semester[3] = 5;

            //semester five
            //plan[4][0] = list[0]; //Gen Ed
            plan[4][1] = list[16].setSemester(4);
            plan[4][2] = list[17].setSemester(4);
            plan[4][3] = list[18].setSemester(4);
            plan[4][4] = list[19].setSemester(4);
            semester[4] = 5;

            //semester six
            plan[5][0] = list[20].setSemester(5);
            plan[5][1] = list[21].setSemester(5);
            plan[5][2] = list[22].setSemester(5);
            plan[5][3] = list[23].setSemester(5);
            plan[5][4] = list[24].setSemester(5);
            semester[5] = 5;

            //semester seven
            //plan[6][0] = list[20]; //gen ed
            //plan[6][1] = list[21]; // tech 
            //plan[6][2] = list[22]; // se elect
            plan[6][3] = list[25].setSemester(6);
            plan[6][4] = list[26].setSemester(6);
            plan[6][5] = list[27].setSemester(6);
            semester[6] = 6;

            //semester eight
            //plan[7][0] = list[20]; // gen ed
            //plan[7][1] = list[21]; // se elect
            //plan[7][2] = list[22]; // suppl elect
            //plan[7][3] = list[25]; // suppl elect
            plan[7][4] = list[28].setSemester(7); 
            plan[7][5] = new Class("Open Elective"); //open elective
            semester[7] = 6;

        }

        //done
        public void setEcon(Class c){
            plan[1][4] = c;
        }

        //done
        public void setTech(Class c)
        {

            plan[6][1] = c;

        }

        //done hopefully
        public void setSuppl(Class[] c)
        {
            bool set = false;
            bool first = false;
            bool second = false;
            bool third = false;


            foreach (Class cl in c)
            {
                set = false;
                if (cl.canTake(2) && !first)
                {

                    plan[2][1] = cl;
                    set = true;
                    first = true;

                }
                if (cl.canTake(7) && !second && !set)
                {

                    plan[7][2] = cl;
                    set = true;
                    second = true;
                }
                if (cl.canTake(7) && !third && !set)
                {
                    plan[7][3] = cl;
                    third = true;

                }
            }

            //plan[2][1]
            //plan[7][2]
            //plan[7][3]
        }

        //done
        public void setMath(Class c)
        {
            plan[3][3] = c;

        }

        //done hopefully
        public void setSe(Class[] c)
        {
            bool first = false;
            bool second = false;
            bool set;

            foreach (Class cl in c)
            {
                set = false;

                if (cl.canTake(6) && !first)
                {
                    plan[6][2] = cl;
                    first = set = true;
                    
                }
                
                if (!set && cl.canTake(7) && !second)
                {
                    plan[7][1] = cl;
                    second = true;
                }
            }
            //plan[6][2]
            //plan[7][1]
        }
        
        //done
        public void setGen(Class[] c)
        {
            bool first = false;
            bool second = false;
            bool third = false;
            bool forth = false;
            bool set;

            foreach (Class cl in c)
            {
                set = false;

                if (cl.canTake(3) && !first)
                {
                    plan[3][0] = cl;
                    first = set = true;

                }

                if (!set && cl.canTake(4) && !second)
                {
                    plan[4][0] = cl;
                    second = true;
                }

                if (!set && cl.canTake(7) && !third)
                {
                    plan[6][0] = cl;
                    third = set = true;
                }

                if (!set && cl.canTake(7) && !forth)
                {
                    plan[7][0] = cl;
                    forth = true;
                }
            }
        }

        public Class[][] getPlan()
        {
            return plan;
        }

        public int[] getClassPerSemester()
        {
            return semester;
        }

        private int setEarliest(Class c)
        {
            for (int i = 0; i < 9; i++)
            {
                if(c.canTake(i)){
                    c.setEarliestSemester(i);
                    return 1;
                }
            }
            c.setEarliestSemester(-1);
            return -1;
        }

    }
}