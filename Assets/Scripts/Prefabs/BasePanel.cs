using UnityEngine;

/**
 * Parent class of the script attached to the Panel objects.
 */
public class BasePanel : MonoBehaviour
{
	/**
	 * Show panel instance.
	 */
	public virtual void Show()
	{
		this.gameObject.SetActive(true);
	}

	/**
	 * Hide panel instance.
	 */
	public virtual void Hide()
	{
		this.gameObject.SetActive(false);
	}

	/**
	 * Return panel's active state.
	 */
	public virtual bool IsVisible
	{
		get => this.gameObject.activeSelf;
	}
}
