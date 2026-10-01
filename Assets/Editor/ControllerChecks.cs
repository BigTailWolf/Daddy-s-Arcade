using System;
using DaddysArcade;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class ControllerChecks
{
    // Virtual-device checks run in batch mode, leaving real devices untouched.
    public static void Run()
    {
        var previousUpdate = InputSystem.settings.updateMode;
        var previousBackground = InputSystem.settings.backgroundBehavior;
        var previousEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var parent = InputSystem.AddDevice<Gamepad>();
        var child = InputSystem.AddDevice<Gamepad>();
        var third = InputSystem.AddDevice<Gamepad>();
        try {
            var p = new ArcadeInput(); var c = new ArcadeInput(); var t = new ArcadeInput();
            InputSystem.Update();
            p.Read(parent,0,.016f); c.Read(child,1,.016f); t.Read(third,2,.016f);
            InputSystem.QueueStateEvent(parent, new GamepadState().WithButton(GamepadButton.South));
            InputSystem.Update();
            Check(p.Read(parent,0,.016f).A && !c.Read(child,1,.016f).A, "A / player isolation");
            InputSystem.QueueStateEvent(parent, new GamepadState());
            InputSystem.QueueStateEvent(child, new GamepadState().WithButton(GamepadButton.East));
            InputSystem.Update();
            var b = c.Read(child,1,.016f);
            Check(b.B && !b.Pause && !p.Read(parent,0,.016f).B, "B rotates only its player");
            InputSystem.QueueStateEvent(child, new GamepadState().WithButton(GamepadButton.North).WithButton(GamepadButton.DpadDown));
            InputSystem.Update();
            var down = c.Read(child,1,.016f);
            Check(down.Drop && down.Down && down.Vertical == 1, "Y / held down");
            InputSystem.Update();
            var held = c.Read(child,1,.016f);
            Check(!held.Drop && held.Down, "Y is an edge; down remains held");
            InputSystem.QueueStateEvent(parent, new GamepadState().WithButton(GamepadButton.Start).WithButton(GamepadButton.DpadLeft));
            InputSystem.Update();
            var menu = p.Read(parent,0,.016f);
            Check(menu.Pause && menu.Horizontal == -1, "Menu / d-pad mapping");
            Check(p.Read(parent,0,.1f).Horizontal == 0 && p.Read(parent,0,.16f).Horizontal == -1,
                "Directional hold repeat");
            InputSystem.QueueStateEvent(parent, new GamepadState { leftStick = new Vector2(1,0) });
            InputSystem.Update();
            Check(p.Read(parent,0,.016f).Horizontal == 1, "Left-stick movement");
            InputSystem.QueueStateEvent(parent,new GamepadState());
            InputSystem.QueueStateEvent(child,new GamepadState());
            InputSystem.QueueStateEvent(third,new GamepadState().WithButton(GamepadButton.South).WithButton(GamepadButton.North).WithButton(GamepadButton.Start));
            InputSystem.Update();
            var thirdFrame = t.Read(third,2,.016f);
            Check(thirdFrame.A && thirdFrame.Drop && thirdFrame.Pause, "Third controller rotate/drop/pause");
            var firstFrame = p.Read(parent,0,.016f); var secondFrame = c.Read(child,1,.016f);
            Check(!firstFrame.A && !firstFrame.Drop && !firstFrame.Pause && !secondFrame.A && !secondFrame.Drop && !secondFrame.Pause,
                "Third controller does not leak into either parent");
            Debug.Log("Daddy's Arcade virtual controller checks passed.");
        } finally {
            InputSystem.RemoveDevice(parent); InputSystem.RemoveDevice(child); InputSystem.RemoveDevice(third);
            InputSystem.settings.updateMode = previousUpdate;
            InputSystem.settings.backgroundBehavior = previousBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditor;
        }
    }
    static void Check(bool condition, string label) { if (!condition) throw new Exception(label); }
}
