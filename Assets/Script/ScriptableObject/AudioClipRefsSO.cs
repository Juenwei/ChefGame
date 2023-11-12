using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AudioClipRefsSO : ScriptableObject
{
	public AudioClip[] chopClips;
	public AudioClip[] deliveryFailClips;
	public AudioClip[] deliverySuccessClips;
	public AudioClip[] footsetpClips;
	public AudioClip[] objectDropClips;
	public AudioClip[] objectPickupClips;
	public AudioClip[] trashClips;
	public AudioClip[] warningClips;
	public AudioClip sizzleClips;
}
