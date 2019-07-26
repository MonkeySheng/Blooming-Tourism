using System;
using ColossalFramework;
using System.Collections.Generic;

namespace Blooming_Tourism
{
    public class DataStorage
    {
        private static DataStorage _instance;
        public static DataStorage instance
        {
            get
            {
                if (DataStorage._instance == null)
                    DataStorage._instance = new DataStorage();
                return DataStorage._instance;
            }
            set { _instance = value; }
        }

        public float multiplier = 1;  // doubles the amount by deafult, 0 means no change to vanilla amount
        public int[] percentageOfPopulation = { 135, 135, 135, 135, 135, 135, 135, 135, 135, 135, 135, 135 };  // per month basis, 12 elements


        Random random = new Random();
        public int amountOfTourists_Month
        {
            get
            {
                return (int) (Utils.GetPopulation() * (instance.percentageOfPopulation[Singleton<SimulationManager>.instance.m_currentGameTime.Month - 1] / 100f) * (1f + (random.Next(-20, 20) / 100f)));  // a bit of fluctuation
            }
        }

        // only used when TourismIncreaseType is population related
        public float AmountOfTourists_Minute
        {
            get
            {
                return instance.amountOfTourists_Month / 43200f;  // DateTime.DaysInMonth(Singleton<SimulationManager>.instance.m_currentGameTime.Year, Singleton<SimulationManager>.instance.m_currentGameTime.Month) might take too long to execute, just use 30days/month: 30 * 1440 = 43200
            }
        }

        // only used when TourismIncreaseType is population related
        public float amountOfTouristsThisMinute  // this amount to create, will account for elapsed minutes and accumulation of the amount to create
        {
            get
            {
                int minutesElapsed = Utils.MinutesElapsed();
                if (minutesElapsed > 60)  // just in case it's been too long
                {
                    minutesElapsed = 60;
                }
                return minutesElapsed * instance.AmountOfTourists_Minute + instance.AmountOfTourists_Minute;
            }
        }


        public TourismIncreaseType modifierType = TourismIncreaseType.PopulationSizeRelated;

        public void SetDeafult()
        {
            DataStorage data = DataStorage.instance;
            data.multiplier = 1;
            data.percentageOfPopulation = new int[] { 135, 135, 135, 135, 135, 135, 135, 135, 135, 135, 135, 135 };
            data.modifierType = TourismIncreaseType.Multiplier;

            data.undesiredServicesList_Tourists = new Dictionary<KeyValuePair<ItemClass.Service, ItemClass.SubService>, bool>
            {
                { new KeyValuePair<ItemClass.Service, ItemClass.SubService>(ItemClass.Service.Beautification, ItemClass.SubService.None), true },
                { new KeyValuePair<ItemClass.Service, ItemClass.SubService>(ItemClass.Service.Education, ItemClass.SubService.None), true },
            };
            data.undesiredServiceList_Residents = new Dictionary<KeyValuePair<ItemClass.Service, ItemClass.SubService>, bool>
            {
                { new KeyValuePair<ItemClass.Service, ItemClass.SubService>(ItemClass.Service.Commercial, ItemClass.SubService.CommercialTourist), true },
            };


        }

        // Modify this dict directly if needed, use it in the OnSettngsUI method
        public Dictionary< KeyValuePair<ItemClass.Service, ItemClass.SubService> ,  bool> undesiredServicesList_Tourists = new Dictionary< KeyValuePair<ItemClass.Service, ItemClass.SubService> ,  bool>
        {
            { new KeyValuePair<ItemClass.Service, ItemClass.SubService>(ItemClass.Service.Beautification, ItemClass.SubService.None), true },  // parks and plazas
            { new KeyValuePair<ItemClass.Service, ItemClass.SubService>(ItemClass.Service.Education, ItemClass.SubService.None), true },  // includes library
            { new KeyValuePair<ItemClass.Service, ItemClass.SubService>(ItemClass.Service.Commercial, ItemClass.SubService.CommercialLow), true },  // low density commercial
        };  // use elements from undesiredServices as KEYS, do this in a for loop perhaps

        public Dictionary<KeyValuePair<ItemClass.Service, ItemClass.SubService>, bool> undesiredServiceList_Residents = new Dictionary<KeyValuePair<ItemClass.Service, ItemClass.SubService>, bool>
        {
            { new KeyValuePair<ItemClass.Service, ItemClass.SubService>(ItemClass.Service.Commercial, ItemClass.SubService.CommercialTourist), true },
        };
    }

    public enum TourismIncreaseType
    {
        Multiplier, PopulationSizeRelated
    }

}
