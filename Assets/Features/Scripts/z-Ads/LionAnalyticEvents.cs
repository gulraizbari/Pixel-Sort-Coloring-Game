using System;
using System.Collections.Generic;
using LionStudios.Suite.Analytics;
using LionStudios.Suite.Analytics.Events;
using UnityEngine;

namespace Sablo.Analytics
{
    public class LionAnalyticEvents
    {
        private static string _defaultNetworkName = "applovin";
        private static string _defaultAdPlacement = "Gameplay";
        
        #region  P0 Events
        
        public static void OnGameStartEvent()
        {
            LionAnalytics.GameStart();
          //  Debug.LogError("sablo start");
        }

        public static void OnErrorEvent(ErrorEventType eventType, string message, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.ErrorEvent(eventType, message,additionalData);
        }
        
        #endregion

        #region  P1 Events
        
        public static void OnInterstitialAdShow(string placement, int levelNumber, string networkName, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.InterstitialShow(placement, networkName, levelNumber, additionalData);
        }

        public static void OnBoosterUsed(int lvl,string boosterType)
        {
            var additionalData = new Dictionary<string, object>
            {
                { "booster", boosterType },
            };
            LionAnalytics.PowerUpUsed($"{lvl+1}", "", 0, boosterType, additionalData);
            Debug.LogError($"sablo level {lvl+1},{boosterType}");
        }

        public static void OnRewardedVideoAdShow(string placement, int levelNumber, string networkName, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.RewardVideoShow(placement, networkName , levelNumber, additionalData);
        }

        public static void OnRewardVideoReceivedReward(string placement, int levelNumber, string rewardName, string rewardType, int rewardAmount, Dictionary<string, object> additionalData = null)
        {
            var product = new Product();
            var virtualCurrencies = new List<VirtualCurrency>
            {
                new VirtualCurrency(rewardName, rewardType, rewardAmount)
            };
            product.virtualCurrencies = virtualCurrencies;
            var reward = new Reward(product);
            
            LionAnalytics.RewardVideoCollect(placement, reward, levelNumber, additionalData);
        }

        public static void OnLevelComplete(int levelNumber,int attempt,string rewardName, string rewardType, int rewardAmount , Dictionary<string, object> additionalData = null )
        {
            var product = new Product();
            var virtualCurrencies = new List<VirtualCurrency>
            {
                new VirtualCurrency(rewardName, rewardType, rewardAmount)
            };
            product.virtualCurrencies = virtualCurrencies;
            var reward = new Reward(product);
            LionAnalytics.LevelComplete(levelNumber,attempt,null, reward,null,null,"","", additionalData);
           // Debug.LogError($"sablo levelcomplete {levelNumber}, reward {reward} attmept {attempt}"); 
        }
        
        public static void OnLevelFail(int levelNumber,int attempts,string reason, Dictionary<string, object> additionalData = null )
        {
            LionAnalytics.LevelFail(levelNumber,attempts,null,null,null,"","", reason,additionalData);
            //Debug.LogError($"sablo levelfail {levelNumber} levelAtmepts {attempts}"); 
        }
        
        public static void OnLevelStart(int levelNumber, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.LevelStart(levelNumber,null,null,null,null,"","", additionalData);
            //Debug.LogError($"sablo levelstart {levelNumber}"); 
        }

        public static void OnLevelMilestoneComplete(int levelNumber, int playerScore, string levelCollection,Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.LevelStep(levelNumber, playerScore, levelCollection, "","","",null, additionalData);
        }
        
        /*public static void OnMissionComplete(string missionType, string missionName, string missionId, int playerScore, string rewardName, string rewardType, int rewardAmount, bool isTutorial=false, Dictionary<string, object> additionalData = null)
        {
            var product = new Product();
            var virtualCurrencies = new List<VirtualCurrency>
            {
                new VirtualCurrency(rewardName, rewardType, rewardAmount)
            };
            product.virtualCurrencies = virtualCurrencies;
            var reward = new Reward(product);
            
            LionAnalytics.MissionCompleted(isTutorial, missionType, missionName, missionId, playerScore, null, additionalData, reward);
        }*/

        public static void MissionCompleted(bool isTutorial , string missionName , int currentLevel , int subLevelNum , int currentLevelAttempt)
        {
            LionAnalytics.MissionCompleted(isTutorial, missionName, $"{currentLevel}", $"{subLevelNum}", currentLevelAttempt);
            //Debug.LogError($"Sablo Mission Completed : CurrentLevel : {currentLevel} SubLevel {subLevelNum} Current Attempt {currentLevelAttempt}");
        }

        /*
        public static void OnMissionFail(string missionType, string missionName, string missionId, int playerScore, bool isTutorial = false, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.MissionFailed(isTutorial, missionType, missionName, missionId, playerScore, null, additionalData);
        }
        */

        public static void MissionFailed(bool isTutorial , string missionName , int currentLevel , int subLevelNum , int currentLevelAttempt)
        {
            LionAnalytics.MissionFailed(isTutorial, missionName, $"{currentLevel}", $"{subLevelNum}", currentLevelAttempt);
            //Debug.LogError($"Sablo Mission Failed : CurrentLevel : {currentLevel} SubLevel {subLevelNum} Current Attempt {currentLevelAttempt}");
        }

        public static void MissionStarted(bool isTutorial , string missionName , int currentLevel , int subLevelNum , int currentLevelAttempt)
        {
            LionAnalytics.MissionStarted(isTutorial, missionName, $"{currentLevel}", $"{subLevelNum}", currentLevelAttempt);
            //Debug.LogError($"Sablo Mission Started : CurrentLevel : {currentLevel} SubLevel {subLevelNum} Current Attempt {currentLevelAttempt}");
        }
        
        /*public static void OnMissionStart(string missionType, string missionName, string missionId, bool isTutorial = false, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.MissionStarted(isTutorial, missionType, missionName, missionId, null, additionalData);
        }*/

        public static void MissionStepCompleted(bool isTutorial , string missionName , int currentLevel , int subLevelNum , int currentLevelAttempt)
        {
            LionAnalytics.MissionStep(isTutorial, missionName, $"{currentLevel}", $"{subLevelNum}", currentLevelAttempt);
            //Debug.LogError($"Sablo Step Completed : CurrentLevel : {currentLevel} SubLevel {subLevelNum} Current Attempt {currentLevelAttempt}");
        }
        
        public static void OnMissionMilestoneComplete(string missionType, string missionName, string missionId, int playerScore, bool isTutorial = false, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.MissionStep(isTutorial, missionType, missionName, missionId, playerScore, null, additionalData);
        }
        

        public static void OnInAppPurchase(int amount,float realAmount, string currencyName, string currencyType, string itemId, string itemType, string placement, string purchaseName = "InAppPurchase", Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.InAppPurchase(amount,currencyName, currencyType, currencyType, realAmount, purchaseName,itemId, null, additionalData, placement, ReceiptStatus.NoValidation);
        }
        
        #endregion

        #region P3 Evetns
        
        public static void OnRewardedAdOptionPresented(string placement)
        {
            LionAnalytics.RewardVideoOpportunity(placement);
        }

        public static void OnShopEntered(string shopName, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.ShopEntered(shopName, null, null, additionalData);
        }
        
        #endregion

        #region  P4 Events
        
        public static void OnInterstitialAdClick(string placement ,int levelNumber, string networkName, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.InterstitialClick(placement, networkName, levelNumber, additionalData);
        }

        public static void OnInterstitialAdEnd(string placement, int levelNumber, string networkName, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.InterstitialEnd(placement, networkName, levelNumber, additionalData);
        }
        
        public static void OnInterstitialAdStart(string placement, int levelNumber, string networkName, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.InterstitialStart(placement, networkName, levelNumber, additionalData);
        }

        public static void OnRewardedAdEnd(string placement, int levelNumber, string networkName, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.RewardVideoEnd(placement, networkName, levelNumber, additionalData);
        }

        public static void OnRewardedAdStart(string placement, int levelNumber, string networkName, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.RewardVideoStart(placement, networkName, levelNumber, additionalData);
        }

        public static void OnRewardedAdClick(string placement, int levelNumber, string networkName, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.RewardVideoClick(placement, networkName, levelNumber, additionalData);
        }

        public static void OnBannerAdRequested(string networkName, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.BannerShowRequested(_defaultAdPlacement, networkName, additionalData);
        }

        public static void OnInterstitialAdLoad(string placement, string networkName, int levelNumber, Dictionary<string, object> additionalData = null)
        {
            LionAnalytics.InterstitialLoad(placement, networkName, levelNumber, additionalData);
        }

        // public static void OnInterstitialAdLoadFail(int levelNumber,  MaxSdkBase.ErrorCode errorType, Dictionary<string, object> additionalData = null)
        // {
        //     LionAnalytics.InterstitialLoadFail(_defaultNetworkName,GetAdErrorType(errorType),levelNumber, additionalData);
        // }
        //
        // public static void OnInterstitialAdShowFail(string placement, string networkName, int levelNumber, MaxSdkBase.ErrorCode errorType, Dictionary<string, object> additionalData = null)
        // {
        //     LionAnalytics.InterstitialShowFail(placement, networkName, levelNumber, GetAdErrorType(errorType), additionalData);
        // }
        //
        // public static void OnRewardedVideoAdLoad( string networkName, int levelNumber, Dictionary<string, object> additionalData = null)
        // {
        //     LionAnalytics.RewardVideoLoad(_defaultAdPlacement, networkName, levelNumber, additionalData);
        // }

        // public static void OnRewardedVideoAdLoadFail(int levelNumber, MaxSdkBase.ErrorCode errorType,Dictionary<string, object> additionalData = null)
        // {
        //     LionAnalytics.RewardVideoLoadFail(_defaultNetworkName, levelNumber, GetAdErrorType(errorType), _defaultAdPlacement, additionalData);
        // }

        // public static void OnRewardedVideoAdShowFail(string placement, string networkName, int levelNumber, MaxSdkBase.ErrorCode errorType, Dictionary<string, object> additionalData = null)
        // {
        //     LionAnalytics.RewardVideoShowFail(placement, networkName, GetAdErrorType(errorType), levelNumber, additionalData);
        // }
        
        #endregion

        // #region Utility Fucnctions
        //
        // private static AdErrorType GetAdErrorType(MaxSdkBase.ErrorCode errorCode) => errorCode switch
        // {
        //      MaxSdkBase.ErrorCode.Unspecified => AdErrorType.Unknown,
        //      MaxSdkBase.ErrorCode.NetworkError => AdErrorType.Offline,
        //      MaxSdkBase.ErrorCode.NoNetwork=> AdErrorType.Offline,
        //      MaxSdkBase.ErrorCode.NoFill => AdErrorType.NoFill,
        //      _ =>  AdErrorType.Unknown
        // };
        //
        // #endregion
        public static void PowerUpUsed( string powerUpName,int lvlNumber )
        {
            LionAnalytics.PowerUpUsed(lvlNumber.ToString(), "", 0, powerUpName, null);//mission attempt can be level attempts
        }
    }
}