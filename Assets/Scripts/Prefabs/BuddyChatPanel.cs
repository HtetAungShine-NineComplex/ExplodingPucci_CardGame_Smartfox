using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using Sfs2X.Entities;

/**
 * Script attached to Buddy Chat Panel prefab.
 */
public class BuddyChatPanel : BasePanel
{
    public Text nameLabel;
    public ScrollRect chatScrollView;
    public Text chatTextArea;
    public InputField messageInput;
    public Button sendButton;

    public UnityEvent<string, string> onBuddyMessageSubmit;

    private string buddyName;
    private string buddyDisplayName;
    private string lastSenderName;

    /**
     * On start, hide panel.
     */
    public void Start()
    {
       Hide();
    }

    /**
	 * Initialize the prefab instance.
	 */
    public void Init(Buddy buddy, List<BuddyChatMessage> history)
	{
        buddyName = buddy.Name;

        // Configure chat panel
        SetState(buddy);

        // Clear previous chat messages (panel is reused for different buddies)
        chatTextArea.text = "";

        // Print chat history
        if (history != null)
            foreach (BuddyChatMessage messageObj in history)
                PrintChatMessage(messageObj);

        // Show chat panel
        Show();

        // Scroll view to bottom
        ScrollViewToBottom();

        // Focus on message input
        SetInputFocus();
    }

    /**
	 * Update prefab instance based on the state of the buddy the current user is chatting with.
	 */
    public void SetState(Buddy buddy)
	{
        // Nickname
        string newBuddyDisplayName = (buddy.NickName != null && buddy.NickName != "") ? buddy.NickName : buddy.Name;

        if (newBuddyDisplayName != buddyDisplayName)
		{
            PrintSystemMessage(buddyDisplayName + " is now known as " + newBuddyDisplayName);
            buddyDisplayName = newBuddyDisplayName;
        }

        nameLabel.text = "with\n" + buddyDisplayName;

        // Enable/disable message input
        SetInputInteractable(buddy.IsOnline && !buddy.IsBlocked);
    }

    /**
     * Display a chat message.
     */
    public void PrintChatMessage(BuddyChatMessage messageObj)
	{
        // Print sender name, unless they are the same of the last message
        string senderName = messageObj.sentByMe ? "" : buddyDisplayName;

        if (senderName != lastSenderName)
            chatTextArea.text += "<b>" + (senderName == "" ? "Me" : senderName) + "</b>\n";

        // Print chat message
        chatTextArea.text += messageObj.message + "\n";

        // Save reference to last message sender, to avoid repeating the name for subsequent messages from the same sender
        lastSenderName = senderName;

        // Scroll view to bottom
        ScrollViewToBottom();
    }

    /**
     * Display a system message.
     */
    public void PrintSystemMessage(string message)
    {
        // Print message
        chatTextArea.text += "<color=#ffffff><i>" + message + "</i></color>\n";

        // Scroll view to bottom
        ScrollViewToBottom();
    }

    /**
     * Return the name of the buddy with whom the current user is chatting.
     */
    public string BuddyName
    {
        get => buddyName;
    }

    /**
     * Check if the prefab instance is currenly active.
     */
    public bool IsActive
	{
        get => this.gameObject.activeSelf;
    }

    /**
     * Hide the panel instance.
     */
    override public void Hide()
	{
        base.Hide();

        buddyName = null;
        lastSenderName = null;
    }

    /**
     * On Close button click, hide panel instance.
     */
    public void OnCloseButtonClick()
	{
        Hide();
    }

    /**
     * On chat message input edit end, if Enter key was pressed, send message to buddy.
     */
    public void OnMessageInputEndEdit()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            OnSendMessageButtonClick();
    }

    /**
     * On Send button click, send message to buddy.
     */
    public void OnSendMessageButtonClick()
    {
        if (messageInput.text != "")
        {
            // Dispatch event
            onBuddyMessageSubmit.Invoke(buddyName, messageInput.text);

            // Reset input
            messageInput.text = "";
            SetInputFocus();
        }
    }

    /**
     * Enable/disable message input interaction.
     */
    public void SetInputInteractable(bool interactable)
	{
        messageInput.interactable = sendButton.interactable = interactable;
    }

    /**
     * Set focus on message input.
     */
    private void SetInputFocus()
	{
        messageInput.Select();
        messageInput.ActivateInputField();
    }

    /**
     * Scroll chat to bottom.
     */
    private void ScrollViewToBottom()
	{
        Canvas.ForceUpdateCanvases();
        chatScrollView.verticalNormalizedPosition = 0;
    }
}
