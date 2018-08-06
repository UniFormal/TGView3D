/************************************************************************************

Copyright   :   Copyright 2017 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.4.1 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1


Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

// simple on-screen keyboard using Unity UI system

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class VRKeyboard : MonoBehaviour {

    public GameObject canvas;
    public GameObject buttonPrototype;
    public Text displayText;
    public Vector2 origin;
    public Vector2 spacing;
    public Vector3 keyTranslation;

    public bool capsShift = false;
    public bool capsLock = false;

    private bool prevCapsLock = false;

    class Key
    {
        public string name;             // name displayed on button
        public string character;
        public string shiftedCharacter;
        public KeyCode code;
        public GameObject button;
        public bool pressed;
        public bool special;
    };

    List<Key> keys;
    Dictionary<KeyCode, Key> keyCodeToKey = new Dictionary<KeyCode, Key>(); // map from key codes to keys

    // create a single key
    void CreateKey(string name, string _char, string shiftedChar, KeyCode code, Vector2 pos, Vector2 scale, bool special=false)
    {
        Key key = new Key();
        key.name = name;
        key.character = _char;
        key.shiftedCharacter = shiftedChar;
        key.code = code;
        key.pressed = false;
        key.special = special;

        // create button
        GameObject obj = (GameObject)Object.Instantiate(buttonPrototype);
        obj.name = name;
        obj.transform.SetParent(canvas.transform, false);
        obj.GetComponent<RectTransform>().anchoredPosition = pos;
        obj.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, spacing.x * scale.x);
        obj.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, spacing.y * scale.y);

        obj.GetComponent<Button>().onClick.AddListener(() => ButtonPressed(key));
        obj.GetComponentInChildren<Text>().text = name;

        key.button = obj;

        keys.Add(key);
        keyCodeToKey.Add(code, key);
    }

    // create a row of keys
    void CreateKeyRow(string chars, string shiftedChars, KeyCode[] codes, Vector2 pos, float offset)
    {
        pos.x += offset*spacing.x;
        for (int i = 0; i < chars.Length; i++)
        {
            CreateKey(chars[i].ToString(), chars[i].ToString(), shiftedChars[i].ToString(), codes[i], pos, Vector2.one);
            pos.x += spacing.x;
        }
    }

    // create a whole keyboard
    void CreateKeyboard()
    {
        keys = new List<Key>();

        Vector2 pos = origin;

        CreateKeyRow(
            "1234567890-=",
            "!@#$%^&*()_+",
            new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.Minus, KeyCode.Equals },
            pos,
            0.0f
        );
        CreateKey("Bksp", "\b", "\b", KeyCode.Backspace, pos + new Vector2(spacing.x * 12.5f, 0.0f), new Vector2(2.0f, 1.0f), true);

        pos.y -= spacing.y;

        CreateKeyRow(
            "qwertyuiop[]",
            "QWERTYUIOP{}",
            new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, KeyCode.LeftBracket, KeyCode.RightBracket },
            pos,
            0.5f
        );
        CreateKey("Enter", "\r", "\r", KeyCode.Return, pos + new Vector2(spacing.x * 12.85f, -spacing.y * 0.5f), new Vector2(1.3f, 2.0f), true);

        pos.y -= spacing.y;

        CreateKeyRow(
            "asdfghjkl;'\\",
            "ASDFGHJKL:\"||",
            new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon, KeyCode.Quote, KeyCode.Backslash },
            pos,
            0.7f
            );
        pos.y -= spacing.y;

        CreateKeyRow(
            "zxcvbnm,./",
            "ZXCVBNM<>?",
            new KeyCode[] { KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M, KeyCode.Comma, KeyCode.Period, KeyCode.Slash, KeyCode.RightShift },
            pos,
            1.0f
            );
        pos.y -= spacing.y;

        CreateKey("Space", " ", " ", KeyCode.Space, pos + new Vector2(spacing.x * 5.5f, 0.0f), new Vector2(6.0f, 1.0f), true);
    }

    // Use this for initialization
    void Start()
    {
        CreateKeyboard();

        if (displayText)
        {
            //displayText.GetComponent<InputField>().ActivateInputField();
        }
    }

    // insert text into input field when button is pressed
    void ButtonPressed(Key key)
    {
        //Debug.Log("button pressed: " + obj.name);
        if (!displayText) return;

        /*
        if (key.code == KeyCode.Return)
        {
            // send submit event
            var submitEvent = new BaseEventData(EventSystem.current);
            ExecuteEvents.Execute(inputField, submitEvent, ExecuteEvents.submitHandler);
        }
         */

        else if (key.code == KeyCode.Backspace && displayText.text.Length > 0)
        {
            // remove last char
            displayText.text = displayText.text.Substring(0, displayText.text.Length - 1);
        }
        else
        {
            displayText.text += capsShift ? key.shiftedCharacter : key.character;
        }
    }

    // change key text to show shifted character
    void SetCapsShift(bool down)
    {
        foreach(var k in keys)
        {
            if (!k.special)
            {
                k.button.GetComponentInChildren<Text>().text = down ? k.shiftedCharacter : k.character;
            }
        }
    }

	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        // activate correct button when key is pressed
        Event e = Event.current;

        if (e.isKey && e.keyCode != KeyCode.None)
        {
            //Debug.Log("Key code: " + e.keyCode);
            if (keyCodeToKey.ContainsKey(e.keyCode))
            {
                Key key = keyCodeToKey[e.keyCode];
                GameObject b = key.button;
                //Debug.Log(b.name);
                //b.GetComponent<Button>().Select();

                // send pointer down/up events to button
                var pointerEvent = new PointerEventData(EventSystem.current);
#if UNITY_2017_2_OR_NEWER
                if (e.type == EventType.KeyDown)
#else
                if (e.type == EventType.keyDown)
#endif
                {
                    //Debug.Log("key down: " + e.keyCode);
                    ExecuteEvents.Execute(b, pointerEvent, ExecuteEvents.pointerEnterHandler);
                    ExecuteEvents.Execute(b, pointerEvent, ExecuteEvents.pointerDownHandler);

                    ButtonPressed(key);
                    if (!key.pressed)
                    {
                        // move key down
                        b.transform.position += keyTranslation;
                        key.pressed = true;
                    }
                }

#if UNITY_2017_2_OR_NEWER
                if (e.type == EventType.KeyUp)
#else
                if (e.type == EventType.keyUp)
#endif
                {
                    //Debug.Log("key up: " + e.keyCode);
                    ExecuteEvents.Execute(b, pointerEvent, ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.Execute(b, pointerEvent, ExecuteEvents.pointerExitHandler);

                    b.transform.position -= keyTranslation;
                    key.pressed = false;
                }
            }
        }

        // caps lock is true while caps lock key is pressed
        if (e.capsLock && !prevCapsLock)
        {
            capsLock = !capsLock;
            //Debug.Log("capsLock = " + capsLock);
            SetCapsShift(capsLock);
        }
        prevCapsLock = e.capsLock;

        // handle shift key
        if (e.shift)
        {
            if (!capsShift)
            {
                capsShift = true;
                //Debug.Log("shift down");
                SetCapsShift(capsLock ? false : true);
            }
        }
        else
        {
            if (capsShift)
            {
                capsShift = false;
                //Debug.Log("shift up");
                SetCapsShift(capsLock ? true : false);
            }
        }
    }
}
