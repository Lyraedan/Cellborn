using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ControllerCursor : MonoBehaviour
{

    public float cursorSpeed = 3f;
    public RawImage cursor;

    private bool wasClicked = false;

    void Update()
    {
        cursor.gameObject.SetActive(ControlManager.ControllerConnected);
        if(ControlManager.ControllerConnected && cursor.gameObject.activeSelf)
        {
            // Move
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");

            var up = transform.up;
            var right = transform.right;

            Vector3 rightMovement = right * cursorSpeed * Time.deltaTime * horizontal;
            Vector3 upMovement = up * cursorSpeed * Time.deltaTime * vertical;

            Vector3 heading = Vector3.Normalize(rightMovement + upMovement);
            Vector3 movingDirection = heading * cursorSpeed * Time.deltaTime;
            cursor.gameObject.transform.Translate(movingDirection);

            try
            {
                // Detect click
                var buttons = FindObjectsOfType<Button>();
                Dictionary<int, float> distances = new Dictionary<int, float>();
                for (int i = 0; i < buttons.Length; i++)
                {
                    float distance = Vector3.Distance(cursor.transform.position, buttons[i].transform.position);
                    distances.Add(i, distance);
                }

                var sorted = distances.ToList();
                sorted.Sort((a, b) => a.Value.CompareTo(b.Value));

                // Press closest button
                var button = sorted[0];
                Button selected = buttons[button.Key];
                selected.Select();
                Debug.Log("Distance: " + button.Value + " -> " + button.Key);

                if (Input.GetAxisRaw(ControlManager.INPUT_FIRE) > 0 && !wasClicked)
                {
                    selected.onClick?.Invoke();
                    wasClicked = true;
                }
                else if (Input.GetAxisRaw(ControlManager.INPUT_FIRE) <= 0)
                {
                    wasClicked = false;
                }
            } catch(Exception e)
            {
                // Do nothing - Also I hate doing it like this but no time to make a cleaner implementation
            }
        }
    }
}
