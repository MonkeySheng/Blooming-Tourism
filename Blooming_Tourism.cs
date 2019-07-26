using System;
using ColossalFramework;
using ColossalFramework.Math;
using ICities;
using UnityEngine;

namespace Blooming_Tourism
{
    public class Blooming_Tourism:IUserMod
    {
        public string Name
        {
            get { return "Blooming Tourism"; }
        }

        public string Description
        {
            get { return "More control on the tourism aspect of the vanilla game"; }
        }

        /*
         * enable toggles for various undesired buildings for both tourists and residents
         * percentage of population on which to base the amount of tourists to create
         */
        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelper uiHelper = helper as UIHelper;
            UIHelper save = (UIHelper)uiHelper.AddGroup("Save to XML file");
            save.AddButton("Save", new OnButtonClicked(this.Save));
        }

        public void Save()
        {
            XML.WriteToDataStorage();
        }
    }


    // This is only for population related tourism control, will be added as a Unity component OnLevelLoaded
    public class TourismControlComponent : MonoBehaviour
    {
        System.Random random = new System.Random();

        CitizenManager citizenManager = Singleton<CitizenManager>.instance;
        BuildingManager buildingManager = Singleton<BuildingManager>.instance;
        Randomizer randomizer = Singleton<SimulationManager>.instance.m_randomizer;

        // TODO: maybe display a message in options panel, like "creating too many tourists, thus hitting the instance limit"
        int createTouristFailed;  // use this to detect if creating tourists is constantly hitting the max instance amount, try to create more next time
        int failedTimes;  // how many times creation of tourists has failed


        public void Update()  // called every frame by UnityEngine
        {
            // create tourists every minute, perhaps choose at which outside connections to create them
            // PROBABLY NOT spawn vehicles at outside connections more frequently, with reference to in game time?
            // TODO: Space Elevator NOT CONSIDERED!!!!!
            Debug.Log("Update() called by unity engine");

            if (DataStorage.instance.modifierType == TourismIncreaseType.PopulationSizeRelated)
            {
                if (Utils.ComponentActionRequired)
                {
                    ushort[] carConnections = Utils.GetConnectedOutsideConnections(ItemClass.SubService.None);  // ItemClass.Service.Road
                    ushort[] trainConnections = Utils.GetConnectedOutsideConnections(ItemClass.SubService.PublicTransportTrain);
                    ushort[] planeConnections = Utils.GetConnectedOutsideConnections(ItemClass.SubService.PublicTransportPlane);
                    ushort[] shipConnections = Utils.GetConnectedOutsideConnections(ItemClass.SubService.PublicTransportShip);

                    // use avg land value to calculate the probability (percentage) of different modes of transport
                    int avgLandValue = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].GetLandValue();

                    int planeProb = planeConnections.Length == 0 ? 0 : 100 * avgLandValue / (avgLandValue + 40);
                    int trainProb = trainConnections.Length == 0 ? 0 : planeProb == 0 ? ((100 - planeProb) * avgLandValue / (avgLandValue + 10)) / 2 : ((100 - planeProb) * avgLandValue / (avgLandValue + 15)) / 2;  // ship and train prob, maybe just divide by 2 for each of these
                    int shipProb = 0;

                    if (trainProb == 0)
                    {
                        if (!(shipConnections.Length == 0))  // no train connections, do have ship connections
                            shipProb = planeProb == 0 ? ((100 - planeProb) * avgLandValue / (avgLandValue + 10)) : ((100 - planeProb) * avgLandValue / (avgLandValue + 15));
                    }
                    else
                    {
                        if (!(shipConnections.Length == 0))
                            shipProb = trainProb;
                        else  // have train connections but no ship connections
                            trainProb *= 2;
                    }

                    int carProb = 100 - (planeProb + trainProb + shipProb);  // would be pretty low even when land value is quite low


                    //int trainProb = trainConnections.Length == 0 ? 0 : 100 - 100 * (avgLandValue / (avgLandValue + 25));  // TODO: maybe customize this
                    //int shipProb = shipConnections.Length == 0 ? 0 : (100 - trainProb) - (100 - trainProb) * (avgLandValue / (avgLandValue + 30));
                    //int planeProb = 100 - (trainProb + shipProb);


                    float amountOfTouristsThisMinute = DataStorage.instance.amountOfTouristsThisMinute;
                    int amount = (int)Math.Truncate(amountOfTouristsThisMinute);
                    amount += (float)random.NextDouble() < amountOfTouristsThisMinute - amount ? 1 : 0;
                    amount += createTouristFailed;

                    Debug.Log(string.Format("amount of tourists to create decided: {0}", amount));

                    int i;  // for debug purpose
                    for (i = 0; i < amount; ++i)
                    {
                        // decide which type of outside connection they are going to spawn at, hence decide the wealth level accordingly
                        ItemClass.SubService typeOfTransport = ItemClass.SubService.PublicTransportPlane;
                        int prob = randomizer.Int32(1, 100);
                        if (prob < carProb)
                        {
                            typeOfTransport = ItemClass.SubService.None;
                        }
                        else if (prob < trainProb + shipProb)
                        {
                            typeOfTransport = randomizer.Int32(2) < 1 ? ItemClass.SubService.PublicTransportTrain : ItemClass.SubService.PublicTransportShip;
                        }
                        // else it's plane


                        // decide the wealth of the tourist based on which type of outside connection it will come from
                        Citizen.Wealth wealth = Citizen.Wealth.Low;
                        if (typeOfTransport == ItemClass.SubService.PublicTransportPlane)
                        {
                            int rand = randomizer.Int32(100);
                            wealth = rand > 18 ? Citizen.Wealth.High : Citizen.Wealth.Medium;
                        }
                        else if (typeOfTransport == ItemClass.SubService.PublicTransportTrain)
                        {
                            int rand = randomizer.Int32(100);
                            wealth = rand > 80 ? Citizen.Wealth.High : rand > 40 ? Citizen.Wealth.Medium : Citizen.Wealth.Low;
                        }
                        else if (typeOfTransport == ItemClass.SubService.PublicTransportShip)
                        {
                            int rand = randomizer.Int32(100);
                            wealth = rand > 70 ? Citizen.Wealth.High : rand > 25 ? Citizen.Wealth.Medium : Citizen.Wealth.Low;
                        }
                        else  // roads, ItemClass.SubService.None
                        {
                            int rand = randomizer.Int32(100);
                            wealth = rand > 70 ? Citizen.Wealth.High : rand > 30 ? Citizen.Wealth.Medium : Citizen.Wealth.Low;
                        }


                        // randomly choose which sourceBuilding aka the outside connection building the tourist is coming from
                        ushort sourceBuilding = 0;

                        try
                        {
                            switch (typeOfTransport)
                            {
                                case ItemClass.SubService.PublicTransportPlane:
                                    sourceBuilding = planeConnections[randomizer.Int32(Convert.ToUInt32(planeConnections.Length))];
                                    break;
                                case ItemClass.SubService.PublicTransportTrain:
                                    sourceBuilding = trainConnections[randomizer.Int32(Convert.ToUInt32(trainConnections.Length))];
                                    break;
                                case ItemClass.SubService.PublicTransportShip:
                                    sourceBuilding = shipConnections[randomizer.Int32(Convert.ToUInt32(shipConnections.Length))];
                                    break;
                                default:  // road connections
                                    sourceBuilding = carConnections[randomizer.Int32(Convert.ToUInt32(carConnections.Length))];
                                    break;
                            }
                            Debug.Log("will be spawned at this outside connection: " + sourceBuilding);
                        }
                        catch
                        {
                            Debug.Log("choosing connection building has thrown an exception");
                        }


                        TransferManager.TransferReason transferReason = NewTouristAI.GetRandomTransferReason(0);
                        TransferManager.TransferOffer offer = Utils.FindOffer(transferReason, true);
                        ushort targetBuilding = offer.Building;
                        Debug.Log(string.Format("component found building {0} for new tourist", targetBuilding));
                        uint unitID = buildingManager.m_buildings.m_buffer[targetBuilding].GetEmptyCitizenUnit(CitizenUnit.Flags.Visit);
                        int family = Singleton<SimulationManager>.instance.m_randomizer.Int32(256U);
                        int age = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 240);

                        if (citizenManager.CreateCitizen(out uint citizen, age, family, ref Singleton<SimulationManager>.instance.m_randomizer))
                        {
                            citizenManager.m_citizens.m_buffer[citizen].m_flags |= Citizen.Flags.Tourist;
                            citizenManager.m_citizens.m_buffer[citizen].m_flags |= Citizen.Flags.MovingIn;
                            citizenManager.m_citizens.m_buffer[citizen].WealthLevel = wealth;
                            citizenManager.m_citizens.m_buffer[citizen].SetVisitplace(citizen, (ushort)0, unitID);
                            CitizenInfo citizenInfo = citizenManager.m_citizens.m_buffer[citizen].GetCitizenInfo(citizen);
                            if (citizenInfo != null && citizenManager.CreateCitizenInstance(out ushort citizenInstance, ref Singleton<SimulationManager>.instance.m_randomizer, citizenInfo, citizen))
                            {
                                citizenInfo.m_citizenAI.SetSource(citizenInstance, ref citizenManager.m_instances.m_buffer[(int)citizenInstance], sourceBuilding);
                                citizenInfo.m_citizenAI.SetTarget(citizenInstance, ref citizenManager.m_instances.m_buffer[(int)citizenInstance], targetBuilding, false);
                                citizenManager.m_citizens.m_buffer[citizen].CurrentLocation = Citizen.Location.Moving;
                                Singleton<StatisticsManager>.instance.Acquire<StatisticArray>(StatisticType.IncomingTourists).Acquire<StatisticInt32>((int)wealth, 3).Add(1);
                            }
                            else
                            {
                                failedTimes += 1;
                                createTouristFailed += amount - i;
                                citizenManager.ReleaseCitizen(citizen);
                                break;
                            }
                        }
                        else
                        {
                            failedTimes += 1;
                            createTouristFailed += amount - i;
                            break;
                        }
                    }
                    Debug.Log(i + " amount of tourists created in this loop");
                }
            }
        }
    }
}
