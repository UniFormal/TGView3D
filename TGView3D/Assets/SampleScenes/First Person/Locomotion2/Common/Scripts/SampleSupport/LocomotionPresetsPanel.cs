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

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// This code will prepare all the options that are available in the Locomotion Presets panel within this sample.
/// While it is possible to set up the panel options and bindings within the editor itself, in this case we opted
/// to duplicate a template button and configure the various handlers in code.
/// 
/// It might make sense for a shipping title to have something similar to allow for the user to choose between
/// some set of preconfigured modes that are suitable for the game. It is not uncommon for games to support 
/// teleport and linear movement as two different ways to move through the world. One of the goals of this sample
/// is to make it easier to support additional configurations for for developers to experiment with in order to
/// discover which works best for their title.
/// </summary>
public class LocomotionPresetsPanel : LocomotionSampleSupport
{
    [Tooltip("This game object should have a button component in addition to a Text component in one of the children.")]
    public GameObject ButtonTemplate;

    private LocomotionTeleport TeleportController
    {
        get
        {
            return LocomotionController.GetComponent<LocomotionTeleport>(); 
        }
    }

    void Start ()
    {
        AddButton("Node-only teleport button A Laser", SetupNodeTeleport);
        AddButton("Navmesh-only teleport right stick", SetupNavTeleport);
        AddButton("Teleport button A orient left stick", SetupTeleportOnly);
        AddButton("Teleport button A CapTouch Warp", SetupTeleportOnlyWarpCapTouch);
        AddButton("Teleport right thumbstick", SetupRightStickTeleport);
        AddButton("Teleport right thumbstick 360", SetupRightStickTeleport360);
        AddButton("1-speed locomotion, teleport button A", SetupFixed1Teleport);
        AddButton("2-speed locomotion, teleport button A", SetupFixed2Teleport);
        AddButton("Linear locomotion, snap, laser, teleport button A Warp", SetupLinearSnapTeleport);
        AddButton("Fixed speed snap turns No teleport", SetupFixedSpeedNoTeleport);
        AddButton("Linear locomotion and turns, teleport button A", SetupLinearTeleport);
        AddButton("Fully Linear No Teleport", SetupLinearNoTeleport);
        ButtonTemplate.SetActive(false);
    }

    /// <summary>
    /// Button handlers will call this to enable/disable teleports as needed.
    /// When teleports are disabled, linear motion needs to be re-enabled.
    /// Since not all teleport presets support linear motion, this will not modify linear motion when teleporting is being enabled.
    /// </summary>
    /// <param name="mode"></param>
    void EnableTeleport(bool mode)
    {
        TeleportController.enabled = mode;
        if (!mode)
        {
            LocomotionController.PlayerController.EnableLinearMovement = true;
            LocomotionController.PlayerController.EnableRotation = true;
        }
    }

    void SetupNodeTeleport()
    {
        // No teleport
        // Linear turns
        // Linear movement
        SetupTeleportDefaults();
        SetupNonCap();
        LocomotionController.PlayerController.SnapRotation = true;
        LocomotionController.PlayerController.FixedSpeedSteps = 1;
        TeleportController.EnableRotation(true, false, false, true);
        ActivateHandlers<TeleportInputHandlerAvatarTouch, TeleportAimHandlerLaser, TeleportTargetHandlerNode, TeleportOrientationHandlerThumbstick, TeleportTransitionBlink>();
    }

    void SetupNavTeleport()
    {
        // Navmesh teleport
        // Snap turns
        // Linear movement
        SetupTeleportDefaults();
        SetupNonCap();
        LocomotionController.PlayerController.SnapRotation = true;
        LocomotionController.PlayerController.FixedSpeedSteps = 1;
        TeleportController.EnableRotation(true, false, false, true);
        ActivateHandlers<TeleportInputHandlerAvatarTouch, TeleportAimHandlerParabolic,
            TeleportTargetHandlerNavMesh, TeleportOrientationHandlerThumbstick, TeleportTransitionBlink>();
        ConfigureRightStickTeleport();
    }

    void ConfigureRightStickTeleport()
    { 
        var input = TeleportController.GetComponent<TeleportInputHandlerAvatarTouch>();
        input.InputMode = TeleportInputHandlerAvatarTouch.InputModes.ThumbstickTeleport;
        var orient = TeleportController.GetComponent<TeleportOrientationHandlerThumbstick>();
        orient.Thumbstick = TeleportOrientationHandlerThumbstick.Thumbsticks.RightThumbstick;
    }

    void SetupFixedTeleport()
    {
        SetupTeleportDefaults();
        SetupNonCap();
        LocomotionController.PlayerController.SnapRotation = true;
        TeleportController.EnableRotation(true, false, false, true);
        TeleportController.EnableMovement(true, false, false, true);
        ActivateHandlers<TeleportInputHandlerAvatarTouch, TeleportAimHandlerParabolic, TeleportTargetHandlerPhysical, TeleportOrientationHandlerThumbstick, TeleportTransitionBlink>();
    }
    void SetupFixed1Teleport()
    {
        SetupFixedTeleport();
        LocomotionController.PlayerController.FixedSpeedSteps = 1;
    }

    void SetupFixed2Teleport()
    {
        SetupFixedTeleport();
        LocomotionController.PlayerController.FixedSpeedSteps = 2;
    }

    void SetupLinearSnapTeleport()
    {
        SetupTeleportDefaults();
        SetupNonCap();
        LocomotionController.PlayerController.SnapRotation = true;
        LocomotionController.PlayerController.FixedSpeedSteps = 0;
        TeleportController.EnableRotation(true, false, false, true);
        TeleportController.EnableMovement(true, false, false, true);
        ActivateHandlers<TeleportInputHandlerAvatarTouch, TeleportAimHandlerLaser, TeleportTargetHandlerPhysical, TeleportOrientationHandler360, TeleportTransitionWarp>();
    }


    void SetupLinearTeleport()
    {
        SetupTeleportDefaults();
        SetupNonCap();
        LocomotionController.PlayerController.SnapRotation = false;
        LocomotionController.PlayerController.FixedSpeedSteps = 0;
        TeleportController.EnableRotation(true, false, false, true);
        TeleportController.EnableMovement(true, false, false, true);
        ActivateHandlers<TeleportInputHandlerAvatarTouch, TeleportAimHandlerParabolic, TeleportTargetHandlerPhysical, TeleportOrientationHandlerThumbstick, TeleportTransitionBlink>();
    }

    void SetupLinearNoTeleport()
    {
        EnableTeleport(false);
        LocomotionController.PlayerController.SnapRotation = false;
        LocomotionController.PlayerController.FixedSpeedSteps = 0;
    }

    void SetupFixedSpeedNoTeleport()
    {
        EnableTeleport(false);
        LocomotionController.PlayerController.SnapRotation = true;
        LocomotionController.PlayerController.FixedSpeedSteps = 1;
    }

    void SetupNonCap()
    {
        var input = TeleportController.GetComponent<TeleportInputHandlerAvatarTouch>();
        input.InputMode = TeleportInputHandlerAvatarTouch.InputModes.SeparateButtonsForAimAndTeleport;
        input.AimButton = OVRInput.RawButton.A;
        input.TeleportButton = OVRInput.RawButton.A;
    }

    void SetupTeleportDefaults()
    {
        EnableTeleport(true);
        LocomotionController.PlayerController.SnapRotation = true;
        LocomotionController.PlayerController.FixedSpeedSteps = 0;
        TeleportController.EnableMovement(false, false, false, false);
        TeleportController.EnableRotation(false, false, false, false);

        var input = TeleportController.GetComponent<TeleportInputHandlerAvatarTouch>();
        input.InputMode = TeleportInputHandlerAvatarTouch.InputModes.CapacitiveButtonForAimAndTeleport;
        input.AimButton = OVRInput.RawButton.A;
        input.TeleportButton = OVRInput.RawButton.A;
        input.CapacitiveAimAndTeleportButton = TeleportInputHandlerAvatarTouch.AimCapTouchButtons.A;
        input.FastTeleport = false;

        var hmd = TeleportController.GetComponent<TeleportInputHandlerHMD>();
        hmd.AimButton = OVRInput.RawButton.A;
        hmd.TeleportButton = OVRInput.RawButton.A;

        var orient = TeleportController.GetComponent<TeleportOrientationHandlerThumbstick>();
        orient.Thumbstick = TeleportOrientationHandlerThumbstick.Thumbsticks.LeftThumbstick;
    }

    void SetupTeleportOnly()
    {
        // Simple press A to teleport
        // 
        // Teleport physical, no motion, snap turns, A button, right thumb orient
        SetupTeleportDefaults();
        TeleportController.EnableRotation(true, false, false, true);
        TeleportController.EnableMovement(false, false, false, false);

        var input = TeleportController.GetComponent<TeleportInputHandlerAvatarTouch>();
        input.InputMode = TeleportInputHandlerAvatarTouch.InputModes.SeparateButtonsForAimAndTeleport;
        ActivateHandlers<TeleportInputHandlerAvatarTouch, TeleportAimHandlerParabolic, TeleportTargetHandlerPhysical, TeleportOrientationHandlerThumbstick, TeleportTransitionBlink>();
        var orient = TeleportController.GetComponent<TeleportOrientationHandlerThumbstick>();
        orient.Thumbstick = TeleportOrientationHandlerThumbstick.Thumbsticks.LeftThumbstick;
    }

    void SetupTeleportOnlyWarpCapTouch()
    {
        // Simple press A to teleport
        // 
        // Teleport physical, no motion, snap turns, A button, right thumb orient
        SetupTeleportDefaults();
        TeleportController.EnableRotation(true, false, false, true);
        TeleportController.EnableMovement(false, false, false, false);
        ActivateHandlers<TeleportInputHandlerAvatarTouch, TeleportAimHandlerParabolic, TeleportTargetHandlerPhysical, TeleportOrientationHandler360, TeleportTransitionWarp>();
        var input = TeleportController.GetComponent<TeleportInputHandlerAvatarTouch>();
        input.FastTeleport = true;
    }

    void SetupRightStickTeleport()
    {
        // Thumbstick to activate teleport.
        // No snap turns
        // No locomotion
        SetupTeleportDefaults();
        ActivateHandlers<TeleportInputHandlerAvatarTouch, TeleportAimHandlerParabolic, TeleportTargetHandlerPhysical, TeleportOrientationHandlerThumbstick, TeleportTransitionBlink>();
        ConfigureRightStickTeleport();
    }

    void SetupRightStickTeleport360()
    {
        // Thumbstick to activate teleport.
        // No snap turns
        // No locomotion
        SetupTeleportDefaults();
        ActivateHandlers<TeleportInputHandlerAvatarTouch, TeleportAimHandlerParabolic, TeleportTargetHandlerPhysical, TeleportOrientationHandler360, TeleportTransitionBlink>();
        var input = TeleportController.GetComponent<TeleportInputHandlerAvatarTouch>();
        input.InputMode = TeleportInputHandlerAvatarTouch.InputModes.ThumbstickTeleport;
    }

    void AddButton(string label, Action action)
    {
        var go = (GameObject) GameObject.Instantiate(ButtonTemplate, transform, false);
        go.name = label;
        var text = go.GetComponentInChildren<Text>();
        text.text = label;
        var button = go.GetComponent<Button>();
        button.onClick.AddListener(() => { action(); });
    }
}
