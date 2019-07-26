using System;
using Boformer.Redirection;
using ColossalFramework;
using ColossalFramework.Math;

namespace Blooming_Tourism
{
    [TargetType(typeof(ResidentAI))]
    class NewResidentAI : ResidentAI
    {
        [RedirectMethod]  // Check and replace the offer if needed, e.g. less likely to visit buildings in tourism specialized districts
        public override void StartTransfer(uint citizenID, ref Citizen data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            // Check and replace the offer if needed
            Randomizer randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            ushort offerBuilding = offer.Building;
            if (Utils.TargetBuildingUndesired_Tourists(offerBuilding))
            {
                // 80% chance not visiting undesired building 
                if (randomizer.Int32(100U) > 19)  // TODO: MAYBE CUSTOMIZE THIS CHANCE
                {
                    TransferManager.TransferOffer newOffer = Utils.FindOffer(reason, false);
                    offer = newOffer;
                }
            }



            {
                if (data.m_flags == Citizen.Flags.None || data.Dead && reason != TransferManager.TransferReason.Dead)
                    return;
                switch (reason)
                {
                    case TransferManager.TransferReason.Single0B:
                    case TransferManager.TransferReason.Single1B:
                    case TransferManager.TransferReason.Single2B:
                    case TransferManager.TransferReason.Single3B:
                    label_34:
                        data.SetHome(citizenID, offer.Building, 0U);
                        if (data.m_homeBuilding != (ushort)0 || data.CurrentLocation == Citizen.Location.Visit && (data.m_flags & Citizen.Flags.Evacuating) != Citizen.Flags.None)
                            break;
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        break;
                    case TransferManager.TransferReason.ShoppingB:
                    case TransferManager.TransferReason.ShoppingC:
                    case TransferManager.TransferReason.ShoppingD:
                    case TransferManager.TransferReason.ShoppingE:
                    case TransferManager.TransferReason.ShoppingF:
                    case TransferManager.TransferReason.ShoppingG:
                    case TransferManager.TransferReason.ShoppingH:
                    label_25:
                        if (data.m_homeBuilding == (ushort)0 || data.Sick)
                            break;
                        data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                        if (!this.StartMoving(citizenID, ref data, (ushort)0, offer))
                            break;
                        data.SetVisitplace(citizenID, offer.Building, 0U);
                        CitizenManager instance1 = Singleton<CitizenManager>.instance;
                        BuildingManager instance2 = Singleton<BuildingManager>.instance;
                        uint containingUnit = data.GetContainingUnit(citizenID, instance2.m_buildings.m_buffer[(int)data.m_homeBuilding].m_citizenUnits, CitizenUnit.Flags.Home);
                        if (containingUnit == 0U)
                            break;
                        instance1.m_units.m_buffer[containingUnit].m_goods += (ushort)100;
                        break;
                    case TransferManager.TransferReason.EntertainmentB:
                    case TransferManager.TransferReason.EntertainmentC:
                    case TransferManager.TransferReason.EntertainmentD:
                    label_30:
                        if (data.m_homeBuilding == (ushort)0 || data.Sick)
                            break;
                        data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                        if (!this.StartMoving(citizenID, ref data, (ushort)0, offer))
                            break;
                        data.SetVisitplace(citizenID, offer.Building, 0U);
                        break;
                    case TransferManager.TransferReason.EvacuateA:
                    case TransferManager.TransferReason.EvacuateB:
                    case TransferManager.TransferReason.EvacuateC:
                    case TransferManager.TransferReason.EvacuateD:
                    case TransferManager.TransferReason.EvacuateVipA:
                    case TransferManager.TransferReason.EvacuateVipB:
                    case TransferManager.TransferReason.EvacuateVipC:
                    case TransferManager.TransferReason.EvacuateVipD:
                        data.m_flags |= Citizen.Flags.Evacuating;
                        if (this.StartMoving(citizenID, ref data, (ushort)0, offer))
                        {
                            data.SetVisitplace(citizenID, offer.Building, 0U);
                            break;
                        }
                        data.SetVisitplace(citizenID, offer.Building, 0U);
                        if (data.m_visitBuilding == (ushort)0 || (int)data.m_visitBuilding != (int)offer.Building)
                            break;
                        data.CurrentLocation = Citizen.Location.Visit;
                        break;
                    default:
                        switch (reason - 2)
                        {
                            case TransferManager.TransferReason.Garbage:
                                if (!data.Sick)
                                    return;
                                data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                                if (!this.StartMoving(citizenID, ref data, (ushort)0, offer))
                                    return;
                                data.SetVisitplace(citizenID, offer.Building, 0U);
                                return;
                            case TransferManager.TransferReason.Crime:
                                if (!data.Dead)
                                    return;
                                data.SetVisitplace(citizenID, offer.Building, 0U);
                                if (data.m_visitBuilding == (ushort)0)
                                    return;
                                data.CurrentLocation = Citizen.Location.Visit;
                                return;
                            case TransferManager.TransferReason.Sick:
                            case TransferManager.TransferReason.Dead:
                            case TransferManager.TransferReason.Worker0:
                            case TransferManager.TransferReason.Worker1:
                                if (data.m_workBuilding != (ushort)0)
                                    return;
                                data.SetWorkplace(citizenID, offer.Building, 0U);
                                return;
                            case TransferManager.TransferReason.Worker2:
                                if (data.m_workBuilding != (ushort)0 || data.EducationLevel != Citizen.Education.Uneducated)
                                    return;
                                data.SetStudentplace(citizenID, offer.Building, 0U);
                                return;
                            case TransferManager.TransferReason.Worker3:
                                if (data.m_workBuilding != (ushort)0 || data.EducationLevel != Citizen.Education.OneSchool)
                                    return;
                                data.SetStudentplace(citizenID, offer.Building, 0U);
                                return;
                            case TransferManager.TransferReason.Student1:
                                if (data.m_workBuilding != (ushort)0 || data.EducationLevel != Citizen.Education.TwoSchools)
                                    return;
                                data.SetStudentplace(citizenID, offer.Building, 0U);
                                return;
                            case TransferManager.TransferReason.Student2:
                                return;
                            case TransferManager.TransferReason.Student3:
                                return;
                            case TransferManager.TransferReason.Fire:
                                return;
                            case TransferManager.TransferReason.Bus:
                                return;
                            case TransferManager.TransferReason.Oil:
                                return;
                            case TransferManager.TransferReason.Ore:
                                return;
                            case TransferManager.TransferReason.Logs:
                                return;
                            case TransferManager.TransferReason.Grain:
                                return;
                            case TransferManager.TransferReason.Goods:
                                return;
                            case TransferManager.TransferReason.PassengerTrain:
                            case TransferManager.TransferReason.Coal:
                            case TransferManager.TransferReason.Family0:
                            case TransferManager.TransferReason.Family1:
                                if (data.m_homeBuilding == (ushort)0 || offer.Building == (ushort)0)
                                    return;
                                uint citizenUnit1 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_homeBuilding].FindCitizenUnit(CitizenUnit.Flags.Home, citizenID);
                                if (citizenUnit1 == 0U)
                                    return;
                                this.MoveFamily(citizenUnit1, ref Singleton<CitizenManager>.instance.m_units.m_buffer[citizenUnit1], offer.Building);
                                return;
                            case TransferManager.TransferReason.Family2:
                            case TransferManager.TransferReason.Family3:
                            case TransferManager.TransferReason.Single0:
                            case TransferManager.TransferReason.Single1:
                                goto label_34;
                            case TransferManager.TransferReason.Single2:
                            case TransferManager.TransferReason.Single3:
                                uint citizen = offer.Citizen;
                                if (citizen == 0U)
                                    return;
                                CitizenManager instance3 = Singleton<CitizenManager>.instance;
                                BuildingManager instance4 = Singleton<BuildingManager>.instance;
                                ushort homeBuilding = instance3.m_citizens.m_buffer[citizen].m_homeBuilding;
                                if (homeBuilding == (ushort)0 || instance3.m_citizens.m_buffer[citizen].Dead)
                                    return;
                                uint citizenUnit2 = instance4.m_buildings.m_buffer[(int)homeBuilding].FindCitizenUnit(CitizenUnit.Flags.Home, citizen);
                                if (citizenUnit2 == 0U)
                                    return;
                                data.SetHome(citizenID, (ushort)0, citizenUnit2);
                                data.m_family = instance3.m_citizens.m_buffer[citizen].m_family;
                                return;
                            case TransferManager.TransferReason.PartnerYoung:
                                goto label_25;
                            case TransferManager.TransferReason.PartnerAdult:
                                return;
                            case TransferManager.TransferReason.Shopping:
                                return;
                            case TransferManager.TransferReason.Petrol:
                                return;
                            case TransferManager.TransferReason.Food:
                                return;
                            case TransferManager.TransferReason.LeaveCity0:
                                return;
                            case TransferManager.TransferReason.LeaveCity1:
                                goto label_30;
                            default:
                                return;
                        }
                }
            }
        }

        // copied from original private function ugh...
        private void MoveFamily(uint homeID, ref CitizenUnit data, ushort targetBuilding)
        {
            BuildingManager instance1 = Singleton<BuildingManager>.instance;
            CitizenManager instance2 = Singleton<CitizenManager>.instance;
            uint unitID = 0;
            if (targetBuilding != (ushort)0)
                unitID = instance1.m_buildings.m_buffer[(int)targetBuilding].GetEmptyCitizenUnit(CitizenUnit.Flags.Home);
            for (int index = 0; index < 5; ++index)
            {
                uint citizen = data.GetCitizen(index);
                if (citizen != 0U && !instance2.m_citizens.m_buffer[citizen].Dead)
                {
                    instance2.m_citizens.m_buffer[citizen].SetHome(citizen, (ushort)0, unitID);
                    if (instance2.m_citizens.m_buffer[citizen].m_homeBuilding == (ushort)0)
                        instance2.ReleaseCitizen(citizen);
                }
            }
        }
    }
}
