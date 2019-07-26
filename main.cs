using Boformer.Redirection;
using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Blooming_Tourism
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private readonly Dictionary<MethodInfo, Redirector> redirectsOnCreated = new Dictionary<MethodInfo, Redirector>();
        public static volatile bool isModEnabled = false;
        public static volatile bool isLevelLoaded = false;

        public static string currentFileLocation = DataLocation.localApplicationData + Path.DirectorySeparatorChar.ToString() + "Blooming_Tourism.xml";
        public static TourismControlComponent component;

        private void Redirect()
        // Goes to RedirectionUtil which goes to Redirector which finally goes to the actual detouring in RedirectionHelper, I guess...

        {

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())

            {

                try

                {

                    var r = RedirectionUtil.RedirectType(type);

                    if (r != null)

                    {

                        foreach (var pair in r)

                        {

                            redirectsOnCreated.Add(pair.Key, pair.Value);

                        }

                    }

                }

                catch (Exception e)

                {

                    UnityEngine.Debug.Log($"An error occured while applying {type.Name} redirects!");

                    UnityEngine.Debug.Log(e.StackTrace);

                }

            }

        }

        private void RevertRedirect()

        {

            foreach (var kvp in redirectsOnCreated)

            {

                try

                {

                    kvp.Value.Revert();

                }

                catch (Exception e)

                {

                    UnityEngine.Debug.Log($"An error occured while reverting {kvp.Key.Name} redirect!");

                    UnityEngine.Debug.Log(e.StackTrace);

                }

            }

            redirectsOnCreated.Clear();

        }

        public override void OnCreated(ILoading loading)

        {
            if (!isModEnabled)

            {
                isModEnabled = true;
                Redirect();
            }
            try
            {
                XML.ReadIntoDataStorage();
            }
            catch { }
        }

        public override void OnReleased()

        {

            if (isModEnabled)

            {
                isModEnabled = false;
                RevertRedirect();
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame)
            {
                // init the game time to be used
                Utils._lastCheckedTime = Singleton<SimulationManager>.instance.m_currentGameTime;

                GameObject gameObject = new GameObject("BloomingTourism");
                component = gameObject.AddComponent<TourismControlComponent>();

                Debug.LogWarning(string.Format("population size is {0}, amount of tourists to create this month is {1}", Utils.GetPopulation(), DataStorage.instance.amountOfTourists_Month));

            }
        }

        public override void OnLevelUnloading()
        {
            if (component != null)
            {
                GameObject.Destroy(component.gameObject);
                component = null;
            }
        }  // Destroys the game object

    }
}
