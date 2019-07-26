using System.Linq;
using System;
using Boformer.Redirection;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.Math;

namespace Blooming_Tourism
{
    [TargetType(typeof(TouristAI))]
    public class NewTouristAI : TouristAI
    {
        [RedirectMethod]
        public override void StartTransfer(uint citizenID, ref Citizen data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            // Check and replace the offer if needed
            Randomizer randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            ushort offerBuilding = offer.Building;
            if (Utils.TargetBuildingUndesired_Tourists(offerBuilding))
            {
                // 90% chance not visiting undesired building 
                if (randomizer.Int32(100U) > 9)  // TODO: MAYBE CUSTOMIZE THIS CHANCE
                {
                    TransferManager.TransferOffer newOffer = Utils.FindOffer(material, true);
                    offer = newOffer;
                }

                Debug.Log("Tourist's StartTransfer method called, and replaced with new offer");

            }

            // just copied the entire original code
            if (data.m_flags == Citizen.Flags.None || data.Dead || data.Sick)
                return;
            switch (material)
            {
                case TransferManager.TransferReason.EvacuateA:
                case TransferManager.TransferReason.EvacuateB:
                case TransferManager.TransferReason.EvacuateC:
                case TransferManager.TransferReason.EvacuateD:
                case TransferManager.TransferReason.EvacuateVipA:
                case TransferManager.TransferReason.EvacuateVipB:
                case TransferManager.TransferReason.EvacuateVipC:
                case TransferManager.TransferReason.EvacuateVipD:
                    data.m_flags |= Citizen.Flags.Evacuating;
                    if (this.StartMoving(citizenID, ref data, data.m_visitBuilding, offer))
                    {
                        data.SetVisitplace(citizenID, offer.Building, 0U);
                        break;
                    }
                    data.SetVisitplace(citizenID, offer.Building, 0U);
                    if (data.m_visitBuilding == (ushort)0 || (int)data.m_visitBuilding != (int)offer.Building)
                        break;
                    data.CurrentLocation = Citizen.Location.Visit;
                    break;
                case TransferManager.TransferReason.TouristA:
                case TransferManager.TransferReason.TouristB:
                case TransferManager.TransferReason.TouristC:
                case TransferManager.TransferReason.TouristD:
                label_6:
                    data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                    if (!this.StartMoving(citizenID, ref data, data.m_visitBuilding, offer))
                        break;
                    data.SetVisitplace(citizenID, offer.Building, 0U);
                    break;
                default:
                    switch (material - 51)
                    {
                        case TransferManager.TransferReason.Garbage:
                        case TransferManager.TransferReason.Crime:
                        case TransferManager.TransferReason.Sick:
                        case TransferManager.TransferReason.Dead:
                        case TransferManager.TransferReason.Worker0:
                        case TransferManager.TransferReason.Worker1:
                        case TransferManager.TransferReason.Worker2:
                        case TransferManager.TransferReason.Worker3:
                        case TransferManager.TransferReason.Student1:
                        case TransferManager.TransferReason.Student2:
                            goto label_6;
                        default:
                            switch (material - 30)
                            {
                                case TransferManager.TransferReason.Garbage:
                                case TransferManager.TransferReason.Worker2:
                                    goto label_6;
                                case TransferManager.TransferReason.Crime:
                                    return;
                                case TransferManager.TransferReason.Sick:
                                    return;
                                case TransferManager.TransferReason.Dead:
                                case TransferManager.TransferReason.Worker0:
                                case TransferManager.TransferReason.Worker1:
                                    data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                                    if (!this.StartMoving(citizenID, ref data, data.m_visitBuilding, offer))
                                        return;
                                    data.SetVisitplace(citizenID, (ushort)0, 0U);
                                    return;
                                default:
                                    return;
                            }
                    }
            }
        }

        [RedirectMethod]
        public override void SetTarget(ushort instanceID, ref CitizenInstance data, ushort targetIndex, bool targetIsNode)
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint citizen = citizenManager.m_instances.m_buffer[instanceID].m_citizen;  // the citizen, found in m_buffer[]
            ushort sourceBuilding = data.m_sourceBuilding;
            ushort[] ListOfOutsideConnections = Utils.GetOutsideConnections();

            if ( ! targetIsNode)  // skip if it's just a node
            {
                if (ListOfOutsideConnections.Contains(sourceBuilding))  // tourist is from outside connection
                {
                    // replace the targetBuilding if needed, there is no offer here so use a hacky workaround
                    if (Utils.TargetBuildingUndesired_Tourists(targetIndex))
                    {
                        targetIndex = Utils.FindOffer(GetRandomTransferReason(0), true).Building;

                        Debug.Log(string.Format("found and replaced offer with buildingID {0}", targetIndex));

                    }

                    // increase tourism volume here by creating more tourists
                    //after that use the vanilla method _SetSource() and _SetTarget() on those tourists

                    // 2 ways of modifying
                    if (DataStorage.instance.modifierType == TourismIncreaseType.Multiplier)
                    {
                        CreateMoreTourists_ByMultiplier(citizen, sourceBuilding);
                    }
                    //else
                    //{
                    //    // TODO: Do I really need it in here??!  Probably create a Unity object and get this job done
                    //    CreateMoreTourists_ByPopulationSize(citizen, sourceBuilding, targetIndex, targetIsNode);
                    //}
                }
            }

            try
            {
                _SetTarget(instanceID, ref data, targetIndex, targetIsNode);
            }
            catch (Exception e)
            {
                Debug.Log("overriden SetTarget() calling _SetTarget() caused an exception: "+e);
            }
        }


        // Spawn only BIKES!!!
        [RedirectMethod]
        protected override VehicleInfo GetVehicleInfo(ushort instanceID, ref CitizenInstance citizenData, bool forceProbability, out VehicleInfo trailer)
        {
            Randomizer r = new Randomizer(citizenData.m_citizen);
            trailer = (VehicleInfo)null;
            if (citizenData.m_citizen == 0U)
                return (VehicleInfo)null;

            try
            {
                if (!Utils.GetOutsideConnections().Contains(citizenData.m_sourceBuilding))  // tourist is NOT at an outside connection
                {
                    Debug.LogWarning("Spawned only bikes for the toursit");
                    return Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref r, ItemClass.Service.Residential, ItemClass.SubService.ResidentialHigh, ItemClass.Level.Level2);  // bikes
                }
            }
            catch (Exception e)
            {
                Debug.Log("error when trying to spawn only bikes" + e);
            }

            // otherwise use original game code, they are just trying to drive to the city
            Citizen.Wealth wealthLevel = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenData.m_citizen].WealthLevel;
            int num1;
            int num2;
            int num3;
            if (forceProbability || (citizenData.m_flags & CitizenInstance.Flags.BorrowCar) != CitizenInstance.Flags.None)
            {
                num1 = 100;
                num2 = 0;
                num3 = wealthLevel == Citizen.Wealth.Low ? 20 : wealthLevel == Citizen.Wealth.Medium ? 30 : 40;  // camper prob
            }
            else
            {
                num1 = 20;  // car prob
                num2 = 20;  // bike prob
                num3 = 0;
            }
            bool flag1 = r.Int32(100U) < num1;
            bool flag2 = r.Int32(100U) < num2;
            bool flag3 = r.Int32(100U) < num3;
            bool flag4;
            bool flag5;
            if (flag1)
            {
                int electricCarProbability = wealthLevel == Citizen.Wealth.Low ? 10 : wealthLevel == Citizen.Wealth.Medium ? 15 : 20; ;
                flag4 = false;
                flag5 = r.Int32(100U) < electricCarProbability;
            }
            else
            {
                int taxiProbability = 20;
                flag4 = r.Int32(100U) < taxiProbability;
                flag5 = false;
            }
            ItemClass.Service service = ItemClass.Service.Residential;
            ItemClass.SubService subService = !flag5 ? ItemClass.SubService.ResidentialLow : ItemClass.SubService.ResidentialLowEco;
            if (!flag1 && flag4)
            {
                service = ItemClass.Service.PublicTransport;
                subService = ItemClass.SubService.PublicTransportTaxi;
            }
            VehicleInfo randomVehicleInfo1;
            if (flag3)
            {
                Randomizer randomizer = r;
                randomVehicleInfo1 = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref r, service, subService, ItemClass.Level.Level2);
                if (randomVehicleInfo1 == null || randomVehicleInfo1.m_vehicleAI is CarTrailerAI)
                {
                    trailer = randomVehicleInfo1;
                    r = randomizer;
                    randomVehicleInfo1 = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref r, service, subService, ItemClass.Level.Level1);
                }
            }
            else
                randomVehicleInfo1 = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref r, service, subService, ItemClass.Level.Level1);
            VehicleInfo randomVehicleInfo2 = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref r, ItemClass.Service.Residential, ItemClass.SubService.ResidentialHigh, ItemClass.Level.Level2);
            if (flag2 && randomVehicleInfo2 != null)  // Bikes
                return randomVehicleInfo2;
            if ((flag1 || flag4) && randomVehicleInfo1 != null)
                return randomVehicleInfo1;
            return (VehicleInfo)null;
        }

        //private void _SetSource(ushort instanceID, ref CitizenInstance data, ushort sourceBuilding)  // copied from original code
        //{
        //    if ((int)sourceBuilding != (int)data.m_sourceBuilding)
        //    {
        //        if (data.m_sourceBuilding != (ushort)0)
        //            Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].RemoveSourceCitizen(instanceID, ref data);
        //        data.m_sourceBuilding = sourceBuilding;
        //        if (data.m_sourceBuilding != (ushort)0)
        //            Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].AddSourceCitizen(instanceID, ref data);
        //    }
        //    if (sourceBuilding == (ushort)0)
        //        return;
        //    BuildingManager instance = Singleton<BuildingManager>.instance;
        //    BuildingInfo info = instance.m_buildings.m_buffer[(int)sourceBuilding].Info;
        //    data.Unspawn(instanceID);
        //    Randomizer randomizer = new Randomizer((int)instanceID);
        //    Vector3 position;
        //    Vector3 target;
        //    info.m_buildingAI.CalculateSpawnPosition(sourceBuilding, ref instance.m_buildings.m_buffer[(int)sourceBuilding], ref randomizer, this.m_info, out position, out target);
        //    Quaternion quaternion = Quaternion.identity;
        //    Vector3 forward = target - position;
        //    if ((double)forward.sqrMagnitude > 0.00999999977648258)
        //        quaternion = Quaternion.LookRotation(forward);
        //    data.m_frame0.m_velocity = Vector3.zero;
        //    data.m_frame0.m_position = position;
        //    data.m_frame0.m_rotation = quaternion;
        //    data.m_frame1 = data.m_frame0;
        //    data.m_frame2 = data.m_frame0;
        //    data.m_frame3 = data.m_frame0;
        //    data.m_targetPos = new Vector4(target.x, target.y, target.z, 1f);
        //    Color32 eventCitizenColor = Singleton<EventManager>.instance.GetEventCitizenColor(instance.m_buildings.m_buffer[(int)sourceBuilding].m_eventIndex, data.m_citizen);
        //    if (eventCitizenColor.a != byte.MaxValue)
        //        return;
        //    data.m_color = eventCitizenColor;
        //    data.m_flags |= CitizenInstance.Flags.CustomColor;
        //}

        private void _SetTarget(ushort instanceID, ref CitizenInstance data, ushort targetIndex, bool targetIsNode)
        {
            if ((int)targetIndex != (int)data.m_targetBuilding || targetIsNode != ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None))
            {
                if (data.m_targetBuilding != (ushort)0)
                {
                    if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
                    {
                        Singleton<NetManager>.instance.m_nodes.m_buffer[(int)data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
                        ushort num = 0;
                        if (targetIsNode)
                            num = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)data.m_targetBuilding].m_transportLine;
                        if ((data.m_flags & CitizenInstance.Flags.OnTour) != CitizenInstance.Flags.None)
                        {
                            ushort transportLine = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)data.m_targetBuilding].m_transportLine;
                            uint citizen = data.m_citizen;
                            if (transportLine != (ushort)0 && (int)transportLine != (int)num && citizen != 0U)
                            {
                                TransportInfo info = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)transportLine].Info;
                                if (info != null && info.m_vehicleType == VehicleInfo.VehicleType.None)
                                    data.m_flags &= ~CitizenInstance.Flags.OnTour;
                            }
                        }
                        if (!targetIsNode)
                            data.m_flags &= ~CitizenInstance.Flags.TargetIsNode;
                    }
                    else
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
                }
                data.m_targetBuilding = targetIndex;
                if (targetIsNode)
                    data.m_flags |= CitizenInstance.Flags.TargetIsNode;
                if (data.m_targetBuilding != (ushort)0)
                {
                    if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
                        Singleton<NetManager>.instance.m_nodes.m_buffer[(int)data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
                    else
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
                    data.m_targetSeed = (byte)Singleton<SimulationManager>.instance.m_randomizer.Int32(256U);
                }
            }

            if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) == CitizenInstance.Flags.None && IsRoadConnection(targetIndex) || IsRoadConnection(data.m_sourceBuilding))
                data.m_flags |= CitizenInstance.Flags.BorrowCar;
            else
                data.m_flags &= ~CitizenInstance.Flags.BorrowCar;
            if (targetIndex != (ushort)0 && (data.m_flags & (CitizenInstance.Flags.Character | CitizenInstance.Flags.TargetIsNode)) == CitizenInstance.Flags.None)
            {
                Color32 eventCitizenColor = Singleton<EventManager>.instance.GetEventCitizenColor(Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)targetIndex].m_eventIndex, data.m_citizen);
                if (eventCitizenColor.a == byte.MaxValue)
                {
                    data.m_color = eventCitizenColor;
                    data.m_flags |= CitizenInstance.Flags.CustomColor;
                }
            }
            if (this.StartPathFind(instanceID, ref data))
                return;
            data.Unspawn(instanceID);
        }  // copied from original code

        private void CreateMoreTourists_ByMultiplier(uint modelCitizen, ushort sourceBuilding)  // the newly arrived tourist that led to creation of more tourists
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Randomizer randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            if ((citizenManager.m_citizens.m_buffer[modelCitizen].m_flags & Citizen.Flags.DummyTraffic) == Citizen.Flags.None)  // not dummy traffic
            {

                float multiplier = DataStorage.instance.multiplier;
                int i = (int)Math.Truncate(multiplier);
                System.Random random = new System.Random();
                i += (float)random.NextDouble() < multiplier - i ? 1 : 0;
                for (int index = 0; index < i; ++index)
                {
                    TransferManager.TransferReason transferReason = GetRandomTransferReason(0);
                    TransferManager.TransferOffer offer = Utils.FindOffer(transferReason, true);
                    ushort targetBuilding = offer.Building;
                    uint unitID = buildingManager.m_buildings.m_buffer[targetBuilding].GetEmptyCitizenUnit(CitizenUnit.Flags.Visit);
                    int family = Singleton<SimulationManager>.instance.m_randomizer.Int32(256U);  // no need to keep to same values
                    int age = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 240);  // this is exactly the original code
                    if (citizenManager.CreateCitizen(out uint citizen, age, family, ref Singleton<SimulationManager>.instance.m_randomizer))
                    {
                        citizenManager.m_citizens.m_buffer[citizen].m_flags |= Citizen.Flags.Tourist;
                        citizenManager.m_citizens.m_buffer[citizen].m_flags |= Citizen.Flags.MovingIn;
                        Citizen.Wealth wealth = citizenManager.m_citizens.m_buffer[modelCitizen].WealthLevel;
                        citizenManager.m_citizens.m_buffer[citizen].WealthLevel = wealth; // same wealth
                        citizenManager.m_citizens.m_buffer[citizen].SetVisitplace(citizen, (ushort)0, unitID);
                        CitizenInfo citizenInfo = citizenManager.m_citizens.m_buffer[citizen].GetCitizenInfo(citizen);
                        if (citizenInfo != null && citizenManager.CreateCitizenInstance(out ushort citizenInstance, ref Singleton<SimulationManager>.instance.m_randomizer, citizenInfo, citizen))
                        {
                            citizenInfo.m_citizenAI.SetSource(citizenInstance, ref citizenManager.m_instances.m_buffer[(int)citizenInstance], sourceBuilding);
                            _SetTarget(citizenInstance, ref citizenManager.m_instances.m_buffer[(int)citizenInstance], targetBuilding, false);
                            citizenManager.m_citizens.m_buffer[citizen].CurrentLocation = Citizen.Location.Moving;
                            Singleton<StatisticsManager>.instance.Acquire<StatisticArray>(StatisticType.IncomingTourists).Acquire<StatisticInt32>((int)wealth, 3).Add(1);
                        }
                    }
                }
            }
        }

        
        //private void CreateMoreTourists_ByPopulationSize(uint modelCitizen, ushort sourceBuilding, ushort targetBuilding, bool targetIsNode)
        //{
        //    CitizenManager citizenManager = Singleton<CitizenManager>.instance;
        //    BuildingManager buildingManager = Singleton<BuildingManager>.instance;
        //    if ((citizenManager.m_citizens.m_buffer[modelCitizen].m_flags & Citizen.Flags.DummyTraffic) == Citizen.Flags.None)  // not dummy traffic
        //    {
        //        System.Random random = new System.Random();

        //        // Handle the creation of tourists on a per minute basis, try to hit the target amount every minute
        //        float amountOfTouristsThisMinute = DataStorage.instance.amountOfTouristsThisMinute;
        //        int i = (int)Math.Truncate(amountOfTouristsThisMinute);
        //        i += (float)random.NextDouble() < amountOfTouristsThisMinute - i ? 1 : 0;

        //        uint unitID = buildingManager.m_buildings.m_buffer[targetBuilding].GetEmptyCitizenUnit(CitizenUnit.Flags.Visit);

        //        for (int index = 0; index < i; ++index)
        //        {
        //            int family = Singleton<SimulationManager>.instance.m_randomizer.Int32(256U);  // no need to keep to same values
        //            int age = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 240);  // this is exactly the original code
        //            if (citizenManager.CreateCitizen(out uint citizen, age, family, ref Singleton<SimulationManager>.instance.m_randomizer))
        //            {
        //                citizenManager.m_citizens.m_buffer[citizen].m_flags |= Citizen.Flags.Tourist;
        //                citizenManager.m_citizens.m_buffer[citizen].m_flags |= Citizen.Flags.MovingIn;
        //                Citizen.Wealth wealth = citizenManager.m_citizens.m_buffer[modelCitizen].WealthLevel;
        //                citizenManager.m_citizens.m_buffer[citizen].WealthLevel = wealth; // same wealth
        //                citizenManager.m_citizens.m_buffer[citizen].SetVisitplace(citizen, (ushort)0, unitID);
        //                CitizenInfo citizenInfo = citizenManager.m_citizens.m_buffer[citizen].GetCitizenInfo(citizen);
        //                if (citizenInfo != null && citizenManager.CreateCitizenInstance(out ushort citizenInstance, ref Singleton<SimulationManager>.instance.m_randomizer, citizenInfo, citizen))
        //                {
        //                    _SetSource(citizenInstance, ref citizenManager.m_instances.m_buffer[(int)citizenInstance], sourceBuilding);
        //                    _SetTarget(citizenInstance, ref citizenManager.m_instances.m_buffer[(int)citizenInstance], targetBuilding, targetIsNode);
        //                    citizenManager.m_citizens.m_buffer[citizen].CurrentLocation = Citizen.Location.Moving;
        //                    Singleton<StatisticsManager>.instance.Acquire<StatisticArray>(StatisticType.IncomingTourists).Acquire<StatisticInt32>((int)wealth, 3).Add(1);
        //                }
        //            }

        //        }

        //    }
        //}



        //private bool TouristNotDummyTraffic(uint citizen)  // just embed it where needed, kept as a reminder
        //{
        //    CitizenManager citizenManager = Singleton<CitizenManager>.instance;
        //    return (citizenManager.m_citizens.m_buffer[citizen].m_flags & Citizen.Flags.DummyTraffic) == Citizen.Flags.None;
        //}




        //private static bool IsRoadConnection(ushort building)  // copied from original code, nothing special
        //{
        //    if (building != 0)
        //    {
        //        BuildingManager instance = Singleton<BuildingManager>.instance;
        //        if ((instance.m_buildings.m_buffer[building].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None && instance.m_buildings.m_buffer[building].Info.m_class.m_service == ItemClass.Service.Road)
        //            return true;
        //    }
        //    return false;
        //}



        public static TransferManager.TransferReason GetShoppingReason()
        {
            switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(8U))
            {
                case 0:
                    return TransferManager.TransferReason.Shopping;
                case 1:
                    return TransferManager.TransferReason.ShoppingB;
                case 2:
                    return TransferManager.TransferReason.ShoppingC;
                case 3:
                    return TransferManager.TransferReason.ShoppingD;
                case 4:
                    return TransferManager.TransferReason.ShoppingE;
                case 5:
                    return TransferManager.TransferReason.ShoppingF;
                case 6:
                    return TransferManager.TransferReason.ShoppingG;
                case 7:
                    return TransferManager.TransferReason.ShoppingH;
                default:
                    return TransferManager.TransferReason.Shopping;
            }
        }

        public static TransferManager.TransferReason GetEntertainmentReason()
        {
            switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(8U))
            {
                case 0:
                    return TransferManager.TransferReason.Entertainment;
                case 1:
                    return TransferManager.TransferReason.EntertainmentB;
                case 2:
                    return TransferManager.TransferReason.EntertainmentC;
                case 3:
                    return TransferManager.TransferReason.EntertainmentD;
                case 4:
                    return TransferManager.TransferReason.TouristA;
                case 5:
                    return TransferManager.TransferReason.TouristB;
                case 6:
                    return TransferManager.TransferReason.TouristC;
                case 7:
                    return TransferManager.TransferReason.TouristD;
                default:
                    return TransferManager.TransferReason.TouristA;
            }
        }

        public static TransferManager.TransferReason GetRandomTransferReason(int doNothingProbability)
        {
            int type = 0;
            SimulationManager instance = Singleton<SimulationManager>.instance;
            if (instance.m_randomizer.Int32(10000U) < doNothingProbability)
                type = 0;
            int num = 2000;
            int factor = Singleton<BuildingManager>.instance.m_finalMonumentEffect[10].m_factor;
            if (factor != 0)
                num = num * 100 / (100 + factor);
            if (instance.m_randomizer.Int32(10000U) < num)
                type = 1;
            type = instance.m_randomizer.Int32(10000U) < 2500 ? 2 : 3;

            switch (type)
            {
                case 1:  // leaving reason
                    return TransferManager.TransferReason.None;
                case 2:
                    return GetShoppingReason();
                case 3:
                    return GetEntertainmentReason();
                default:
                    return TransferManager.TransferReason.None;
            }
        }

        private static bool IsRoadConnection(ushort building)
        {
            if (building != (ushort)0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                if ((instance.m_buildings.m_buffer[(int)building].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None && instance.m_buildings.m_buffer[(int)building].Info.m_class.m_service == ItemClass.Service.Road)
                    return true;
            }
            return false;
        }

    }
}
