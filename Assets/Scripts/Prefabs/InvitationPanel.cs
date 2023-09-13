using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using Sfs2X.Entities.Invitation;

/**
 * Script attached to Invitation Panel prefab.
 */
public class InvitationPanel : BasePanel
{
	public Text invMessageText;
	public Text expirationText;

	public UnityEvent<Invitation, bool> onInvitationReplyClick;

	private InvitationWrapper invitationWrapper;
	private float timer;
	private bool timerEnabled;

	/**
	 * Update invitation expiration timer.
	 */
	protected virtual void Update()
	{
		if (timerEnabled)
		{
			timer -= Time.deltaTime;
			ShowCountdown();

			if (Math.Floor(timer) <= 0)
				TriggerInvitationReplyEvent(false);
		}
	}

	/**
	 * Show panel instance with the invitation details.
	 */
	public void Show(InvitationWrapper iw)
	{
		this.invitationWrapper = iw;

		// Display invitation message
		string message = "";

		if (iw.invitation.Params.GetUtfString("message") != "")
			message += "<i>" + iw.invitation.Params.GetUtfString("message") + "</i>\n";

		message += "You have been invited by <b>" + iw.invitation.Inviter.Name + "</b> to play <b>" + iw.invitation.Params.GetUtfString("room") + "</b>";

		invMessageText.text = message;

		// Set expiration time
		timer = iw.expiresInSeconds;
		timerEnabled = true;

		// Display remaining time for replying
		ShowCountdown();

		// Show panel
		this.gameObject.SetActive(true);
	}

	/**
	 * Refuse invitation.
	 */
	override public void Hide()
	{
		// Refuse invitation
		if (invitationWrapper != null)
			TriggerInvitationReplyEvent(false);
	}

	/**
	 * On Accept button click, send invitation reply.
	 */
	public void OnInvitationAcceptClick()
	{
		// Accept invitation
		TriggerInvitationReplyEvent(true);
	}

	/**
	 * On Refuse button click, send invitation reply.
	 */
	public void OnInvitationRefuseClick()
	{
		// Refuse invitation
		TriggerInvitationReplyEvent(false);
	}

	/**
	 * Dispatch a custom event to reply to the invitation.
	 */
	private void TriggerInvitationReplyEvent(bool accept)
	{
		// Stop timer
		timerEnabled = false;

		Invitation invitation = invitationWrapper.invitation;

		// Delete reference to invitation wrapper
		this.invitationWrapper = null;

		// Hide panel
		this.gameObject.SetActive(false);

		// Trigger event
		onInvitationReplyClick.Invoke(invitation, accept);
	}

	/**
	 * Display countdown to expiration.
	 */
	private void ShowCountdown()
	{
		int secs = (int)Math.Floor(timer);
		expirationText.text = "This invitation will expire in " + secs + " seconds";
	}
}
