using System;
using ICities;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace EcoStats
{

    public class EcoStats : IUserMod
    {
        public String Name
        {
            get { return "Eco Stats Mod"; }
        }

        public String Description
        {
            get { return "Economic Statistics Viewer (GDP, GDP per capita, GINI)"; }
        }

    }

    public class LoadingExtension : LoadingExtensionBase
    {

        UIView boxUIView;
        UIButton btnEcoStats;
        static UILabel lblCityStats;

        static bool statsShown = false;

        static Stats stats;

        public override void OnLevelLoaded(LoadMode mode)
        {

            boxUIView = UIView.GetAView();
            btnEcoStats = (UIButton)boxUIView.AddUIComponent(typeof(UIButton));

            UIDragHandle dh = (UIDragHandle)boxUIView.AddUIComponent(typeof(UIDragHandle));

            var uiView = GameObject.FindObjectOfType<UIView>();
            if (uiView == null) return;

            btnEcoStats.width = 125;
            btnEcoStats.height = 30;

            btnEcoStats.normalBgSprite = "ButtonMenu";
            btnEcoStats.hoveredBgSprite = "ButtonMenuHovered";
            btnEcoStats.focusedBgSprite = "ButtonMenuFocused";
            btnEcoStats.pressedBgSprite = "ButtonMenuPressed";

            btnEcoStats.textColor = new Color32(186, 217, 238, 0);
            btnEcoStats.disabledTextColor = new Color32(7, 7, 7, 255);
            btnEcoStats.hoveredTextColor = new Color32(7, 132, 255, 255);
            btnEcoStats.focusedTextColor = new Color32(255, 255, 255, 255);
            btnEcoStats.pressedTextColor = new Color32(30, 30, 44, 255);

            btnEcoStats.transformPosition = new Vector3(1f, 0.95f);

            btnEcoStats.text = "Eco Stats";

            btnEcoStats.eventClick += BtnClickedEcoStats;

            updateStats();

        }

        public class Economy : EconomyExtensionBase
        {
            byte checkerIntervalAmount = 0; //Check every 3 Money Updates
            public override long OnUpdateMoneyAmount(long internalMoneyAmount)
            {
                if(checkerIntervalAmount == 2)
                {
                    //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "Update money amount...");
                    updateStats();
                    checkerIntervalAmount = 0;
                }
                else
                {
                    checkerIntervalAmount++;
                }
                return base.OnUpdateMoneyAmount(internalMoneyAmount);
            }
        }

        private void BtnClickedEcoStats(UIComponent component, UIMouseEventParameter eventParam)
        {

            statsShown = !statsShown;

            if (statsShown)
            {
                lblCityStats = (UILabel)boxUIView.AddUIComponent(typeof(UILabel));

                stats = new Stats();
                //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "Eco Stats: \n" + stats);

                var uiView = GameObject.FindObjectOfType<UIView>();
                if (uiView == null) return;

                lblCityStats.text = stats.ToString();
                lblCityStats.transformPosition = new Vector3(1f, 0.88f);
                lblCityStats.SendToBack();
            }
            else
            {
                UIView.DestroyImmediate(lblCityStats);
            }

        }

        public static void updateStats()
        {
            //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "Will check if should update stats...");
            if (statsShown)
            {
                //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "Update stats");
                try
                {
                    stats = new Stats();
                    lblCityStats.text = stats.ToString();
                } catch (NullReferenceException ex)
                {
                    //Do nothing...
                }
            }
        }

    }

    class ServiceSubServicePair
    {
        public ItemClass.Service service { get; set; }
        public ItemClass.SubService subService { get; set; }

        public ServiceSubServicePair(ItemClass.Service service, ItemClass.SubService subService)
        {
            this.service = service;
            this.subService = subService;
        }


    }

}
