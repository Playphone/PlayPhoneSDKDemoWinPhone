//
//  MNWSCurrUserSubscriptionStatus.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

namespace PlayPhone.MultiNet.Core.WS.Data
 {
  public class MNWSCurrUserSubscriptionStatus : MNWSGenericItem
   {
    public bool? GetHasSubscription ()
     {
      return GetBooleanValue("has_subscription");
     }

    public string GetOffersAvailable ()
     {
      return GetValueByName("offers_available");
     }

    public bool? GetIsSubscriptionAvailable ()
     {
      return GetBooleanValue("is_subscription_available");
     }
   }
 }
