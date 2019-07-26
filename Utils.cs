using System;
using UnityEngine;
using ColossalFramework.Math;
using ColossalFramework;
using System.Collections.Generic;
using System.Reflection;

namespace Blooming_Tourism
{
    public static class Utils
    {
        private static CitizenManager citizenManager = Singleton<CitizenManager>.instance;
        private static BuildingManager buildingManager = Singleton<BuildingManager>.instance;
        private static DistrictManager districtManager = Singleton<DistrictManager>.instance;
        private static TransferManager transferManager = Singleton<TransferManager>.instance;
        private static Randomizer randomizer = Singleton<SimulationManager>.instance.m_randomizer;

        // check if building is in park area or tourism/leisure specialized district
        private static bool BuildingInDesiredDistrict_Tourists(ushort buildingID)
        {
            Vector3 buildingPos = buildingManager.m_buildings.m_buffer[buildingID].m_position;
            // Check for park area
            DistrictPark parkArea = districtManager.m_parks.m_buffer[districtManager.GetPark(buildingPos)];
            if (parkArea.IsPark)
                return true;
            // not a park but a district
            District district = districtManager.m_districts.m_buffer[districtManager.GetDistrict(buildingPos)];
            if (district.IsPolicySet(DistrictPolicies.Policies.Tourist) || district.IsPolicySet(DistrictPolicies.Policies.Leisure))
                return true;
            return false;
        }

        // Undesired buildings for tourists, UNLESS the building is in a park or a district with tourism specialization
        public static bool TargetBuildingUndesired_Tourists(ushort targetIndex)
        {
            BuildingInfo buildingInfo = buildingManager.m_buildings.m_buffer[targetIndex].Info;
            ItemClass.Service service = buildingInfo.GetService();
            ItemClass.SubService subService = buildingInfo.GetSubService();
            if (DataStorage.instance.undesiredServicesList_Tourists.TryGetValue(new KeyValuePair<ItemClass.Service, ItemClass.SubService>(service, subService), out bool toggled) && toggled)  // now, check if they are in a park or tourism specialized district
            {
                if (BuildingInDesiredDistrict_Tourists(targetIndex))
                    return false;
                return true;
            }
            else  // combo not registered or toggled off, defaults to desired, returns false
            {
                return false;
            }
        }

        // Undesired buildings for residents, e.g. tourism specialized commercial etc.
        public static bool TargetBuildingUndesired_Residents(ushort targetIndex)
        {
            BuildingInfo buildingInfo = buildingManager.m_buildings.m_buffer[targetIndex].Info;
            ItemClass.Service service = buildingInfo.GetService();
            ItemClass.SubService subService = buildingInfo.GetSubService();
            if (DataStorage.instance.undesiredServiceList_Residents.TryGetValue(new KeyValuePair<ItemClass.Service, ItemClass.SubService>(service, subService), out bool toggled) && toggled)
            {
                return true;
            }
            else { return false; }
        }


        // Modify the offer amount etc., just like how they are done in TransferManager.MatchOffers(), trust that the call site will use what is returned
        // WILL NOT return an UNDESIRED offer
        public static TransferManager.TransferOffer FindOffer(TransferManager.TransferReason reason, bool IsForTourists)
        {

            TransferManager.TransferOffer[] outgoingOffers = (TransferManager.TransferOffer[])typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(transferManager);
            ushort[] outgoingCount = (ushort[])typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(transferManager);

            Debug.Log(string.Format("outgoing offers aquired, outgoingCount is {0}", outgoingCount));

            TransferManager.TransferOffer offer = new TransferManager.TransferOffer();
            int offerIndex_ = -1; int offerPos_ = -1; int offerCategory_ = -1;
            for (int counter = 0; counter < 20; ++counter)  // try randomly fetch an offer 20 times before giving up
            {
                int priority = randomizer.Int32(8);
                int offerCategory = (int)reason * 8 + priority; offerCategory_ = offerCategory;
                int offerPos = randomizer.Int32(outgoingCount[offerCategory]); offerPos_ = offerPos;
                int offerIndex = offerCategory * 256 + offerPos; offerIndex_ = offerIndex;
                offer = outgoingOffers[offerIndex];
                if (offer.Building == 0)  // just in case, 0 is the default value
                {
                    --counter;
                    continue;
                }

                switch (IsForTourists)
                {
                    case true:
                        if (TargetBuildingUndesired_Tourists(offer.Building))
                        {
                            continue;
                        }
                        goto properOfferFound;

                    case false:
                        if (TargetBuildingUndesired_Residents(offer.Building))
                        {
                            continue;
                        }
                        goto properOfferFound;
                }
            properOfferFound:
                // there you go the desired offer, also assumes amount is 1 for 1 tourist, which should always be the case
                {
                    TransferManager.TransferOffer _offer = offer;
                    _offer.Amount -= 1;
                    if (_offer.Amount <= 0)
                    {
                        outgoingOffers[offerIndex] = outgoingOffers[offerCategory * 256 + outgoingCount[offerCategory] - 1];  // move the last in array here
                        outgoingCount[offerCategory] -= 1;
                        typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(transferManager, outgoingOffers);
                        typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(transferManager, outgoingCount);
                    }
                    else
                    {
                        outgoingOffers[offerIndex] = _offer;
                        typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(transferManager, outgoingOffers);
                    }

                    return offer;
                }
            }
            TransferManager.TransferOffer __offer = offer;
            __offer.Amount -= 1;
            if (__offer.Amount <= 0)
            {
                outgoingOffers[offerIndex_] = outgoingOffers[offerCategory_ * 256 + outgoingCount[offerCategory_] - 1];  // move the last in array here
                outgoingCount[offerCategory_] -= 1;
                typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(transferManager, outgoingOffers);
                typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(transferManager, outgoingCount);
            }
            else
            {
                outgoingOffers[offerIndex_] = __offer;
                typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(transferManager, outgoingOffers);
            }
            return offer;  // failed to find the proper offer after 20 times, return the last found offer regardless
        }



        public static ushort[] GetOutsideConnections()  // Just a wrapper
        {
            return buildingManager.GetOutsideConnections().ToArray();
        }

        public static bool OutsideConnectionIsConnected(ushort buildingID)  // somehow this m_incomingProblemTimer will be 0 for not connected ones
        {
            return buildingManager.m_buildings.m_buffer[buildingID].m_incomingProblemTimer != 0;
        }

        public static ushort[] GetConnectedOutsideConnections()
        {
            List<ushort> connectedList = new List<ushort>();
            foreach (ushort connection in GetOutsideConnections())
            {
                if (OutsideConnectionIsConnected(connection))
                {
                    connectedList.Add(connection);
                }
            }
            return connectedList.ToArray();
        }

        public static ushort[] GetConnectedOutsideConnections(ItemClass.SubService typeOfTransport)
        {
            List<ushort> chosenConnections = new List<ushort>();
            ushort[] connections = GetConnectedOutsideConnections();
            foreach (var connection in connections)
            {
                ItemClass.SubService subService = buildingManager.m_buildings.m_buffer[connection].Info.m_class.m_subService;
                // as long as it is an outgoing connection aka providing stuff into the city
                if (subService == typeOfTransport && (buildingManager.m_buildings.m_buffer[connection].m_flags & Building.Flags.Outgoing) != 0)
                    chosenConnections.Add(connection);
            }
            return chosenConnections.ToArray();
        }


        public static int GetTouristVisits()
        {
            StatisticsManager sm = Singleton<StatisticsManager>.instance;
            StatisticBase statsTourists = sm.Get(StatisticType.TouristVisits);
            if (statsTourists != null)
            {
                return statsTourists.GetLatestInt32();
            }
            else { return -1; }  // maybe not useful at all
        }  // could well do without this it seems

        public static int GetIncomingTourists()
        {
            StatisticsManager sm = Singleton<StatisticsManager>.instance;
            StatisticBase statsIncomingTourists = sm.Get(StatisticType.IncomingTourists);
            if (statsIncomingTourists != null)
            {
                return statsIncomingTourists.GetLatestInt32();
            }
            else { return -1; }  // maybe not useful at all
        }  // seems not useful, idk

        public static int GetPopulation()
        {
            if (Singleton<DistrictManager>.exists)  // just in case, idk
            {
                return (int)Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_residentialData.m_finalHomeOrWorkCount;  // whole populatioin includes tourists
            }
            else { return -1; }
        }

        public static DateTime _lastCheckedTime;  // initialised OnLevelLoaded
        public static DateTime lastCheckedTime
        {
            get
            {
                DateTime __lastCheckedTime = _lastCheckedTime;
                _lastCheckedTime = Singleton<SimulationManager>.instance.m_currentGameTime;  // update to currentGameTime whenever accessed, but return the old one 
                return __lastCheckedTime;
            }
        }
        public static int MinutesElapsed()  // returns the whole number of minutes (truncated, not rounded)
        {
            int minElapsed = (int)Singleton<SimulationManager>.instance.m_currentGameTime.Subtract(lastCheckedTime).TotalMinutes;
            Debug.Log("MinutesElapsed() called, returned this many minutes: " + minElapsed);
            return minElapsed;
        }

        private static DateTime lastActionTime;
        public static bool ComponentActionRequired  // the component takes action every minute at least
        {
            get
            {
                if (Singleton<SimulationManager>.instance.m_currentGameTime.Subtract(lastActionTime).TotalMinutes == 0)
                    return false;
                lastActionTime = Singleton<SimulationManager>.instance.m_currentGameTime;
                return true;
            }
        }


        // TODO: Implement method to display warning for constant failure of creation of tourists, perhaps change uiTextField's text
        public static void DisplayTooManyTouristsWarning()
        {

        }
    }
}
