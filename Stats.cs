using System;
using System.Collections.Generic;
using System.Text;

using ColossalFramework;

namespace EcoStats
{
    class Stats
    {

        public float gdp { get; set; }
        public float gdpPerCapita { get; set; }
        public float gini { get; set; }
        public float cityValuePC { get; set; }
        public float cityValuePCNoMoney { get; set; }
        public int population { get; set; }
        public float govtSpending { get; set; }
        public float privateSpending { get; set; }
        public float giniBuildings { get; set; }

        public Stats()
        {
            //GDP
            ServiceSubServicePair[] servAndSubServ = new ServiceSubServicePair[] {
                new ServiceSubServicePair(ItemClass.Service.Residential, ItemClass.SubService.ResidentialHigh),
                new ServiceSubServicePair(ItemClass.Service.Residential, ItemClass.SubService.ResidentialHighEco),
                new ServiceSubServicePair(ItemClass.Service.Residential, ItemClass.SubService.ResidentialLow),
                new ServiceSubServicePair(ItemClass.Service.Residential, ItemClass.SubService.ResidentialLowEco),
                new ServiceSubServicePair(ItemClass.Service.Commercial, ItemClass.SubService.CommercialEco),
                new ServiceSubServicePair(ItemClass.Service.Commercial, ItemClass.SubService.CommercialHigh),
                new ServiceSubServicePair(ItemClass.Service.Commercial, ItemClass.SubService.CommercialLeisure),
                new ServiceSubServicePair(ItemClass.Service.Commercial, ItemClass.SubService.CommercialLow),
                new ServiceSubServicePair(ItemClass.Service.Commercial, ItemClass.SubService.CommercialTourist),
                new ServiceSubServicePair(ItemClass.Service.Industrial, ItemClass.SubService.IndustrialFarming),
                new ServiceSubServicePair(ItemClass.Service.Industrial, ItemClass.SubService.IndustrialForestry),
                new ServiceSubServicePair(ItemClass.Service.Industrial, ItemClass.SubService.IndustrialGeneric),
                new ServiceSubServicePair(ItemClass.Service.Industrial, ItemClass.SubService.IndustrialOil),
                new ServiceSubServicePair(ItemClass.Service.Industrial, ItemClass.SubService.IndustrialOre),
                new ServiceSubServicePair(ItemClass.Service.Office, ItemClass.SubService.OfficeGeneric),
                new ServiceSubServicePair(ItemClass.Service.Office, ItemClass.SubService.OfficeHightech)
            };

            long outable;

            int[] taxRates = new int[servAndSubServ.Length];
            long[] incomes = new long[servAndSubServ.Length];
            float expendableIncome = 0;  //Non taxed income
            for (int i = 0; i < servAndSubServ.Length; i++)
            {
                taxRates[i] = Singleton<EconomyManager>.instance.GetTaxRate(servAndSubServ[i].service, servAndSubServ[i].subService, ItemClass.Level.None);
                Singleton<EconomyManager>.instance.GetIncomeAndExpenses(servAndSubServ[i].service, servAndSubServ[i].subService, ItemClass.Level.None, out incomes[i], out outable);
                expendableIncome += incomes[i] / taxRates[i] * (100 - taxRates[i]);
            }

            long govtExpLong;
            float governmentExpenses;
            long income;    //Al pedo...
            Singleton<EconomyManager>.instance.GetIncomeAndExpenses(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Level.None, out income, out govtExpLong);

            expendableIncome /= 100;
            governmentExpenses = govtExpLong / 100;
            float loanExpenses = Singleton<EconomyManager>.instance.GetLoanExpenses() / 100;

            this.gdp = expendableIncome + governmentExpenses - loanExpenses;
            this.privateSpending = expendableIncome;
            this.govtSpending = governmentExpenses;

            this.population = Singleton<CitizenManager>.instance.m_citizenCount;

            //GDP PER CAPITA
            if (this.population == 0)
            {
                this.gdpPerCapita = -1;
            }
            else
            {
                this.gdpPerCapita = (gdp / this.population);
            }

            //GINI
            BuildingManager buildingManager = BuildingManager.instance;

            List<byte> coefficientsGiniRes = new List<byte>();
            List<byte> coefficientsGiniBuildings = new List<byte>();

            for (int i = 0; i < buildingManager.m_buildings.m_size; i++)
            {
                BuildingInfo info = buildingManager.m_buildings.m_buffer[i].Info;
                byte level = buildingManager.m_buildings.m_buffer[i].m_level;
                if (info.GetService() == ItemClass.Service.Residential)
                {
                    byte citizenCount = buildingManager.m_buildings.m_buffer[i].m_citizenCount;
                    for (byte j = 0; j < citizenCount; j++)
                    {
                        coefficientsGiniRes.Add(level);
                    }
                }
                coefficientsGiniBuildings.Add(level);
            }

            this.gini = calculateGINI(coefficientsGiniRes);
            this.giniBuildings = calculateGINI(coefficientsGiniBuildings);

            //CITY VALUE PER CAPITA
            float cityValue = Singleton<StatisticsManager>.instance.Get(StatisticType.CityValue).GetLatestFloat();
            if (this.population == 0)
            {
                this.cityValuePC = -1;
            }
            else
            {
                this.cityValuePC = cityValue / this.population;
            }

            //CITY VALUE - CURRENT MONEY AMOUNT PER CAPITA
            if (this.population == 0)
            {
                this.cityValuePCNoMoney = -1;
            }
            else
            {
                long lastCashAmount = Singleton<EconomyManager>.instance.LastCashAmount / 100;
                this.cityValuePCNoMoney = (cityValue - lastCashAmount) / this.population;
            }
        }

        public override string ToString()
        {
            string data = "";
            if (this.population != 0)
            {
                data += "GDP: ₡" + String.Format("{0:n}", this.gdp) + "\n";
                data += "GDP Per Capita: ₡" + String.Format("{0:n}", this.gdpPerCapita) + "\n";
                data += "Residential GINI: " + this.gini + "\n";
                data += "City Value per Capita: ₡" + String.Format("{0:n}", this.cityValuePC) + "\n";
                data += "City Value per Capita (W/O Savings): ₡" + String.Format("{0:n}", this.cityValuePCNoMoney) + "\n";
                data += "Total Government Spending: ₡" + String.Format("{0:n}", this.govtSpending) + "\n";
                data += "Total Private Spending: ₡" + String.Format("{0:n}", this.privateSpending) + "\n";
                data += "Government Spending to GDP: " + (this.govtSpending * 100 / this.gdp) + "%\n";
                data += "Private Spending to GDP: " + (this.privateSpending * 100 / this.gdp) + "%";
            }
            else
            {
                data += "I need population!";
            }
            return data;
        }

        private float calculateGINI(List<byte> coefficients)
        {
            float gini = -1;
            if (coefficients.Count != 0)
            {
                coefficients.Sort();    //Sort amounts in ascending order
                double upper = 0;
                double downer = 0;
                for (int i = 0; i < coefficients.Count; i++)
                {
                    upper += ((i + 1) * coefficients[i]);
                    downer += (coefficients[i]);
                }
                upper = 2 * upper;
                downer = coefficients.Count * downer;
                float rester = (float)(coefficients.Count + 1) / coefficients.Count;
                if (downer == 0)
                {
                    gini = 0;   //It means all icomes are the same (perfect equality)
                }
                else
                {
                    gini = (float)(upper / downer) - rester;
                }
            }
            return gini;
        }

    }
}
