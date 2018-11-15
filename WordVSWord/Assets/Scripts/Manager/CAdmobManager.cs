using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSingleton;
using GoogleMobileAds.Api;

public class CAdmobManager {

    #region Fields
	public static string appId = "ca-app-pub-9073576735501135~6997756728";
	public static string adUnitIdBanner = "ca-app-pub-9073576735501135/8134744429";
	public static string adUnitIDRewardedVideo = "ca-app-pub-9073576735501135/1916820431";

	protected static BannerView bannerView;
    protected static bool bannerShowing = false;
    protected static RewardBasedVideoAd rewardBasedVideo;
    public static Action OnVideoAdsReward;
    public static Action OnVideoAdsClose;

    #endregion

    #region Constructor

	public static void Init()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);
    }

    #endregion

    #region Banner

	public static void InitBanner()
    {
        // Create a 320x50 banner at the bottom of the screen.
        bannerView = new BannerView(adUnitIdBanner, AdSize.Banner, AdPosition.Bottom);
		// Called when an ad request has successfully loaded.
        bannerView.OnAdLoaded -= HandleOnAdBannerLoaded;
        bannerView.OnAdLoaded += HandleOnAdBannerLoaded;
        // Called when an ad request failed to load.
        bannerView.OnAdFailedToLoad -= HandleOnAdBannerFailedToLoad;
        bannerView.OnAdFailedToLoad += HandleOnAdBannerFailedToLoad;
        // Called when an ad is clicked.
        bannerView.OnAdOpening -= HandleOnAdBannerOpening;
        bannerView.OnAdOpening += HandleOnAdBannerOpening;
        // Called when the user returned from the app after an ad click.
        bannerView.OnAdClosed -= HandleOnAdBannerClosed;
        bannerView.OnAdClosed += HandleOnAdBannerClosed;
        // Called when the ad click caused the user to leave the application.
        bannerView.OnAdLeavingApplication -= HandleOnAdBannerLeavingApplication;
        bannerView.OnAdLeavingApplication += HandleOnAdBannerLeavingApplication;
    }

    public static void LoadBanner()
    {
        if (bannerView != null)
        {
            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();
            // Load the banner with the request.
            bannerView.LoadAd(request);
            bannerShowing = false;
        }
    }

    public static void ShowHideBanner(bool value)
    {
        if (bannerView != null)
        {
            if (value)
            {
                bannerView.Show();
                bannerShowing = true;
            }
            else
            {
                bannerView.Hide();
                bannerShowing = false;
            }
        }
    }

    public static void DestroyBanner()
    {
        if (bannerView != null)
        {
            bannerView.OnAdLoaded -= HandleOnAdBannerLoaded;
            bannerView.OnAdFailedToLoad -= HandleOnAdBannerFailedToLoad;
            bannerView.OnAdOpening -= HandleOnAdBannerOpening;
            bannerView.OnAdClosed -= HandleOnAdBannerClosed;
            bannerView.OnAdLeavingApplication -= HandleOnAdBannerLeavingApplication;
            bannerView.Destroy();
        }
    }

	protected static void HandleOnAdBannerLoaded(object sender, EventArgs args)
    {
        Debug.LogWarning("HandleAdLoaded event received");
        ShowHideBanner(true);
    }

    protected static void HandleOnAdBannerFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.LogWarning("HandleFailedToReceiveAd event received with message: " + args.Message);
        LoadBanner();
    }

    protected static void HandleOnAdBannerOpening(object sender, EventArgs args)
    {
        Debug.LogWarning("HandleAdOpened event received");
    }

    protected static void HandleOnAdBannerClosed(object sender, EventArgs args)
    {
        Debug.LogWarning("HandleAdClosed event received");
    }

    protected static void HandleOnAdBannerLeavingApplication(object sender, EventArgs args)
    {
         Debug.LogWarning("HandleAdLeavingApplication event received");
    }

    #endregion

    #region Reward Video

    public static void InitRewardedVideo()
    {
        rewardBasedVideo = RewardBasedVideoAd.Instance;

        // Called when an ad request has successfully loaded.
        rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
        // Called when an ad request failed to load.
        rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        // Called when an ad is shown.
        rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
        // Called when the ad starts to play.
        rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
        // Called when the user should be rewarded for watching a video.
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        // Called when the ad is closed.
        rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        // Called when the ad click caused the user to leave the application.
        rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;
    }

    public static void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {

    }

    public static void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        LoadRewardedVideo();
    }

    public static void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {

    }

    public static void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {

    }

    public static void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        if (OnVideoAdsClose != null)
        {
            OnVideoAdsClose();
        }
        LoadRewardedVideo();
    }

    public static void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        if (OnVideoAdsReward != null)
        {
            OnVideoAdsReward();
        }
    }

    public static void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {

    }

    public static void LoadRewardedVideo()
    {
        if (rewardBasedVideo != null)
        {
            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();
       
            // Load the rewarded video ad with the request.
            rewardBasedVideo.LoadAd(request, adUnitIDRewardedVideo);
        }
    }

    public static void ShowRewardedVideo()
    {
        if (rewardBasedVideo.IsLoaded())
        {
            rewardBasedVideo.Show();
        }
        else
        {
            LoadRewardedVideo();
        }
    }

    public static void DestroyRewardedVideo()
    {
        if (rewardBasedVideo != null)
        {
            rewardBasedVideo.OnAdLoaded -= HandleRewardBasedVideoLoaded;
            rewardBasedVideo.OnAdFailedToLoad -= HandleRewardBasedVideoFailedToLoad;
            rewardBasedVideo.OnAdOpening -= HandleRewardBasedVideoOpened;
            rewardBasedVideo.OnAdStarted -= HandleRewardBasedVideoStarted;
            rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
            rewardBasedVideo.OnAdClosed -= HandleRewardBasedVideoClosed;
            rewardBasedVideo.OnAdLeavingApplication -= HandleRewardBasedVideoLeftApplication;
        }
    }

    #endregion

}
