using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepManager : MonoBehaviour
{

    public PlayerMovementTest controller;
    public AudioSource audioSource;
    public List<Footstep> footsteps = new List<Footstep>();

    private int currentIndex = 0;
    private bool audioQueued = false;

    [SerializeField] private bool active = false;

    public PhysicMaterial GetMaterial()
    {
        if (controller.StoodOn != null)
        {
            if (controller.GetComponent<Collider>())
            {
                return controller.StoodOn.GetComponent<Collider>().material;
            }
        }

        return null;
    }

    private void Update()
    {
        if (!active)
            return;

        if (controller.isGrounded)
        {
            PhysicMaterial mat = GetMaterial();
            if (mat != null)
            {
                if (!audioQueued)
                {
                    if (mat.name.Length <= 0)
                        return;

                    var name = mat.name.Substring(0, mat.name.Length - 11); // Trip off " (Instance)"
                    StartCoroutine(PlayFootstepFor(string.IsNullOrEmpty(name) ? "Concrete" : name));
                }
            }
            else
            {
                if (audioSource.isPlaying)
                    audioSource.Stop();
                audioQueued = false;
                currentIndex = 0;
            }
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            audioQueued = false;
            currentIndex = 0;
        }
    }

    IEnumerator PlayFootstepFor(string footstepId)
    {
        if (string.IsNullOrEmpty(footstepId) || controller.movingDirection == Vector3.zero)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            yield break;
        }
        Debug.Log("Queueing: " + footstepId);
        audioQueued = true;
        yield return new WaitUntil(() => controller.isGrounded);
        yield return new WaitUntil(() => !audioSource.isPlaying);
        var footstep = footsteps.FirstOrDefault(step => step.id.Equals(footstepId));
        if (footstep != null)
        {
            AudioClip clip = footstep.sounds[currentIndex % footstep.sounds.Count];
            audioSource.clip = clip;
            audioSource.Play();
            yield return new WaitForSeconds(clip.length);
            currentIndex++;
            audioQueued = false;
        }
        else
        {
            Debug.Log("Failed to find footsteps assosiated with that material: " + footstepId);
            yield break;
        }
    }
}

[Serializable]
public class Footstep
{
    public string id = "Unnamed";
    public PhysicMaterial material;
    public List<AudioClip> sounds = new List<AudioClip>();
}
