using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace QM.Controllers
{
    public class HomeController : Controller
    {

        List<string> units = new List<string>()
                {
                    "Regular time", "Overtime","Subcontracting","Holding/carrying cost","Lost Sales cost","Increase cost","Decrease cost","Initial Inventory", "Units Last Period"
                };
        List<string> units_short_chase = new List<string>()
                {
                    "demand", "rtc","otc","sc","rtp","otp","sub","short","increase","decrease"
                };
        List<string> units_short_lost = new List<string>()
                {
                    "demand", "rtc","otc","sc","rtp","otp","sub","inventory","shortage"
                };
        int NoPeriod = 4;

        public IActionResult Index()
        {
            HttpContext.Session.SetInt32("NoPeriod", 4);
            TempData["units"] = units;
            return View();
        }
        [HttpPost]
        public IActionResult Index(int NoPeriod)
        {
            HttpContext.Session.SetInt32("NoPeriod", NoPeriod);
            TempData["units"] = units;
            return View();
        }
        

        
        [HttpPost]
        public IActionResult Result(IFormCollection data)
        {
            
            if (data["lost"].ToString() != "")
            {
                SaveData(data);
                CalculateLost();
                Total_Calc(units_short_lost);
                return View("LostSales");
            }
            else if(data["chase"].ToString() != "")
            {
                SaveData(data);
                CalculateChase();
                Total_Calc(units_short_chase);
                return View("Chase");
            }
            return NotFound();
        }

        public void SaveData(IFormCollection data)
        {
            NoPeriod = (int)HttpContext.Session.GetInt32("NoPeriod");

            for (int i = 0; i < NoPeriod; i++)
            {
                TempData["period" + i] = data["period" + i];
                TempData["demand" + i] = data["demand" + i];
                TempData["rtc" + i] = data["rtc" + i];
                TempData["otc" + i] = data["otc" + i];
                TempData["sc" + i] = data["sc" + i];
            }

            for (int i = 0; i < units.Count; i++)
            {
                TempData[units[i]] = data[units[i]];
            }

            TempData["units_short_chase"] = units_short_chase;
            TempData["units_short_lost"] = units_short_lost;

        }

        public void CalculateLost()
        {
            int totaldemand = TotalUnits("demand");
            int initial = int.Parse(TempData["Initial Inventory"].ToString());
            int rtp_calc = (int)Math.Ceiling((totaldemand - initial) / (double)NoPeriod);

            for (int i = 0; i < NoPeriod; i++)
            {
                int demand = int.Parse(TempData["demand" + i].ToString());
                int rtc = int.Parse(TempData["rtc" + i].ToString());

                TempData["rtp" + i] = Math.Min(rtc, rtp_calc);
                int rtp_rest = Math.Abs(rtc - rtp_calc);

                Overtime_Subcontract_Lost(i, rtp_rest);
                
                Inventory_Shortage(i, initial, demand);
                
            }
        }
        public void Overtime_Subcontract_Lost(int i, int rtp_rest)
        {
            int sc = int.Parse(TempData["sc" + i].ToString());
            int otc = int.Parse(TempData["otc" + i].ToString());
            if (int.Parse(TempData["Subcontracting"].ToString()) < int.Parse(TempData["Overtime"].ToString()))
            {
                if (sc >= rtp_rest)
                {
                    TempData["sub" + i] = rtp_rest;
                    TempData["otp" + i] = 0;
                }
                else
                {
                    TempData["sub" + i] = sc;
                    int rtp_restTwo = rtp_rest - sc;
                    if (otc <= rtp_restTwo)
                    {
                        TempData["otp" + i] = otc;
                    }
                    else
                    {
                        TempData["otp" + i] = rtp_restTwo;
                    }

                }
            }
            else
            {
                if (otc >= rtp_rest)
                {
                    TempData["otp" + i] = rtp_rest;
                    TempData["sub" + i] = 0;
                }
                else
                {
                    TempData["otp" + i] = otc;
                    int rtp_restTwo = rtp_rest - otc;
                    if (sc <= rtp_restTwo)
                    {
                        TempData["sub" + i] = sc;
                    }
                    else
                    {
                        TempData["sub" + i] = rtp_restTwo;
                    }

                }

            }
        }
        public void Inventory_Shortage(int i, int initial, int demand )
        {
            int inventory = 0;
            if (i > 0)
            {
                inventory = int.Parse(TempData["inventory" + (i - 1)].ToString()) + int.Parse(TempData["rtp" + i].ToString()) + int.Parse(TempData["otp" + i].ToString()) + int.Parse(TempData["sub" + i].ToString()) - demand;
                if (inventory > 0)
                {
                    TempData["inventory" + i] = inventory;
                    TempData["shortage" + i] = 0;
                }
                else
                {
                    TempData["shortage" + i] = Math.Abs(inventory);
                    TempData["inventory" + i] = 0;
                }

            }
            else
            {
                inventory = initial + int.Parse(TempData["rtp" + i].ToString()) + int.Parse(TempData["otp" + i].ToString()) + int.Parse(TempData["sub" + i].ToString()) - demand;
                if (inventory > 0)
                {
                    TempData["inventory" + i] = inventory;
                    TempData["shortage" + i] = 0;
                }
                else
                {
                    TempData["shortage" + i] = Math.Abs(inventory);
                    TempData["inventory" + i] = 0;
                }
            }
        }

        public void CalculateChase()
        {
            for (int i = 0; i < NoPeriod; i++)
            {
                int initial = int.Parse(TempData["Initial Inventory"].ToString());
                int demand = int.Parse(TempData["demand" + i].ToString());
                int rtc = int.Parse(TempData["rtc" + i].ToString());
                int otc = int.Parse(TempData["otc" + i].ToString());

                TempData["rtp" + i] = Math.Min(demand, rtc);
                int rtp = int.Parse(TempData["rtp" + i].ToString());
                TempData["otp" + i] = Math.Min(demand - rtp, otc);


                Overtime_Subcontract_Chase(i, demand, initial, rtc);
                Increase_Decrease(i);
            }
            

        }

        public void Overtime_Subcontract_Chase(int i, int demand, int initial, int rtc)
        {
            int rtp = int.Parse(TempData["rtp" + i].ToString());
            int sc = int.Parse(TempData["sc" + i].ToString());
            int otc = int.Parse(TempData["otc" + i].ToString());

            int subcontract = demand - (rtc + otc) > 0 ? demand - (rtc + otc) : 0;
            int subcontract_rest = subcontract - sc > 0 ? subcontract - sc : 0;

            int overtime = demand - (rtc + sc) > 0 ? demand - (rtc + sc) : 0;
            int overtime_rest = overtime - otc > 0 ? overtime - otc : 0;

            if (int.Parse(TempData["Subcontracting"].ToString()) < int.Parse(TempData["Overtime"].ToString()))
            {

                TempData["sub" + i] = Math.Min(demand - rtp, sc);


                if (subcontract_rest > 0)
                {
                    TempData["otp" + i] = Math.Min(overtime, otc);
                    TempData["short" + i] = overtime_rest;
                }
                else
                {
                    TempData["otp" + i] = Math.Min(overtime, otc);
                    TempData["short" + i] = 0;
                }

                

            }
            else
            {
                TempData["otp" + i] = Math.Min(demand - rtp, otc);
                

                if (subcontract_rest > 0)
                {
                    TempData["sub" + i] = Math.Min(subcontract, sc);
                    TempData["short" + i] = subcontract_rest;
                }
                else
                {
                    TempData["sub" + i] = Math.Min(subcontract, sc);
                    TempData["short" + i] = 0;
                }


            }

            if(i == 0)
            {
                int otp0 = int.Parse(TempData["otp0"].ToString());
                int sub0 = int.Parse(TempData["sub0"].ToString());
                int short0 = int.Parse(TempData["short0"].ToString());

                if (otp0 > 0)
                {
                    TempData["otp0"] = otp0 - initial;
                }
                else if (sub0 > 0)
                {
                    TempData["sub0"] = sub0 - initial;
                }
                else if (short0 > 0)
                {
                    TempData["short0"] = short0 - initial;
                }
            }

        }

        public void Increase_Decrease(int i)
        {
            if (i > 0)
            {
                int increase = int.Parse(TempData["rtp" + i].ToString()) - int.Parse(TempData["rtp" + (i - 1)].ToString());
                if (increase > 0)
                {
                    TempData["increase" + i] = increase;
                    TempData["decrease" + i] = 0;
                }
                else
                {
                    TempData["decrease" + i] = Math.Abs(increase);
                    TempData["increase" + i] = 0;
                }
            }
            else
            {
                TempData["increase" + i] = 0;
                TempData["decrease" + i] = 0;
            }
        }

        public void Total_Calc(List<string> shorts)
        {
            for (int i = 0; i < shorts.Count; i++)
            {
                TotalUnits(shorts[i]);
            }
        }

        public int TotalUnits(string unit)
        {
            int sum = 0;
            for(int i = 0; i < NoPeriod; i++)
            {
                sum += int.Parse(TempData[unit + i].ToString());
            }
            TempData["Total" + unit] = sum;
            return sum;
        }
    }
}