using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using Sfs2X.Entities;
using Sfs2X.Entities.Managers;
using Sfs2X.Entities.Variables;

/**
 * Script attached to User Profile Panel prefab.
 */
public class UserProfilePanel : BasePanel
{
	public Text usernameLabel;

	public Toggle onlineToggle;
	public InputField nickInput;
	public InputField yearInput;
	public InputField moodInput;
	public Dropdown stateDropdown;

	public Dropdown expDropdown;
	public RankingInput rankInput;

	public UnityEvent<bool> onOnlineToggleChange;
	public UnityEvent<string, object> onBuddyDetailChange;
	public UnityEvent<string, object> onPlayerDetailChange;

	/**
	 * Show the generic user profile details.
	 */
	public void InitUserProfile(string username)
	{
		// Username
		usernameLabel.text = "<b>Username:</b> " + username;
	}

	/**
	 * Show the profile details related to the buddy list system.
	 */
	public void InitBuddyProfile(IBuddyManager buddyManager)
    {
		// User online/offline state
		onlineToggle.isOn = buddyManager.MyOnlineState;

		// User nickname
		nickInput.text = (buddyManager.MyNickName != null ? buddyManager.MyNickName : "");

		// Available states and current user state
		stateDropdown.AddOptions(buddyManager.BuddyStates);
		stateDropdown.SetValueWithoutNotify(buddyManager.BuddyStates.IndexOf(buddyManager.MyState));

		// Buddy variable: user birth year
		BuddyVariable year = buddyManager.GetMyVariable(LobbySceneController.BUDDYVAR_YEAR);
		yearInput.text = ((year != null && !year.IsNull()) ? year.GetIntValue().ToString() : "");

		// Buddy variable: user mood
		BuddyVariable mood = buddyManager.GetMyVariable(LobbySceneController.BUDDYVAR_MOOD);
		moodInput.text = ((mood != null && !mood.IsNull()) ? mood.GetStringValue() : "");
	}

	/**
	 * Show the player details (used invitations, quick-join, etc).
	 */
	public void InitPlayerProfile(User user)
	{
		// Available experience values
		List<string> expValues = new List<string>();
		foreach (Dropdown.OptionData optionData in expDropdown.options)
			expValues.Add(optionData.text);

		// User variable: experience
		expDropdown.SetValueWithoutNotify(expValues.IndexOf(user.GetVariable(LobbySceneController.USERVAR_EXPERIENCE).GetStringValue()));

		// User variable: ranking
		rankInput.Ranking = user.GetVariable(LobbySceneController.USERVAR_RANKING).GetIntValue();
	}

	/**
	 * Dispatch a custom event to set the user's online/offline state in the buddy list system.
	 */
	public void OnOnlineToggleChange(bool isChecked)
	{
		// Dispatch event
		onOnlineToggleChange.Invoke(isChecked);

		nickInput.interactable = isChecked;
		yearInput.interactable = isChecked;
		moodInput.interactable = isChecked;
		stateDropdown.interactable = isChecked;
	}

	/**
	 * Dispatch a custom event to set the user's nickname in the buddy list system.
	 */
	public void OnNickInputEnd()
	{
		// Dispatch event
		onBuddyDetailChange.Invoke(ReservedBuddyVariables.BV_NICKNAME, nickInput.text);
	}

	/**
	 * Dispatch a custom event to set the user's birth year in the buddy list system.
	 */
	public void OnBirthYearInputEnd()
	{
		// Dispatch event
		if (yearInput.text != "")
			onBuddyDetailChange.Invoke(LobbySceneController.BUDDYVAR_YEAR, Int32.Parse(yearInput.text));
		else
			onBuddyDetailChange.Invoke(LobbySceneController.BUDDYVAR_YEAR, null);
	}

	/**
	 * Dispatch a custom event to set the user's mood in the buddy list system.
	 */
	public void OnMoodInputEnd()
	{
		// Dispatch event
		onBuddyDetailChange.Invoke(LobbySceneController.BUDDYVAR_MOOD, moodInput.text);
	}

	/**
	 * Dispatch a custom event to set the user's state in the buddy list system.
	 */
	public void OnStateDropdownChange()
	{
		// Dispatch event
		onBuddyDetailChange.Invoke(ReservedBuddyVariables.BV_STATE, stateDropdown.options[stateDropdown.value].text);
	}

	/**
	 * Dispatch a custom event to set the user's experience in User Variables.
	 */
	public void OnExperienceDropdownChange()
	{
		// Dispatch event
		onPlayerDetailChange.Invoke(LobbySceneController.USERVAR_EXPERIENCE, expDropdown.options[expDropdown.value].text);
	}

	/**
	 * Dispatch a custom event to set the user's ranking in User Variables.
	 */
	public void OnRankingChange()
	{
		// Dispatch event
		onPlayerDetailChange.Invoke(LobbySceneController.USERVAR_RANKING, rankInput.Ranking);
	}
}
