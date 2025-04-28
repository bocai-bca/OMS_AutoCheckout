using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace OMS_AutoCheckout;

[BepInPlugin("oms_autocheckout", "AutoCheckout", "0.0.0")]
public class AutoCheckout : BaseUnityPlugin
{
	internal static new ManualLogSource Logger;

	private void Awake()
	{
		Logger = base.Logger;
		Logger.LogInfo($"Plugin oms_autocheckout is loaded!");
		Harmony harmony = new("oms_autocheckout");
		harmony.PatchAll();
	}
	[HarmonyPatch(typeof(UIManager), "OpenJournal")]
	public static class Injector
	{
		public static void Postfix()
		{
			if (IsInjected)
			{
				return;
			}
			GameObject gameObject = new();
			gameObject.AddComponent<Loop>();
			Object.DontDestroyOnLoad(gameObject);
			IsInjected = true;
		}
	}
	static bool IsInjected = false;
	static Checkout[] CheckoutList = [];
	static int SearchTimer = 120;
	public class Loop : MonoBehaviour
	{
		private void Update()
		{
			SearchTimer--;
			if (SearchTimer <= 0)
			{
				CheckoutList = Object.FindObjectsByType<Checkout>(FindObjectsSortMode.None);
				SearchTimer = 120;
			}
			foreach (Checkout checkout in CheckoutList)
			{
				if (!checkout.IsHost)
				{
					return;
				}
				List<CheckoutItem> checkoutItems = checkout.GetCheckoutItems();
				if (checkoutItems.Count > 0)
				{
					checkoutItems[0].EmployeeInteract();
					return;
				}
				CoinPouch coinPouch = checkout.GetCoinPouch();
				coinPouch?.EmployeeInteract();
			}
		}
	}
}