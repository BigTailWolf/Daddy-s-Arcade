using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace DaddysArcade
{
    public struct ArcadeInputFrame
    {
        public int Horizontal, Vertical;
        public bool A, B, Drop, Pause, Down;
    }
    public sealed class ArcadeInput
    {
        int lastHorizontal, lastVertical;
        float horizontalWait, verticalWait;
        public static bool Pressed(ButtonControl button) => button != null && button.wasPressedThisFrame;
        static bool Held(ButtonControl button) => button != null && button.isPressed;
        static int Repeat(int value, ref int last, ref float wait, float dt)
        {
            if (value == 0) { last = 0; wait = 0; return 0; }
            if (value != last) { last = value; wait = .25f; return value; }
            wait -= dt;
            if (wait > 0) return 0;
            wait = .085f;
            return value;
        }
        public ArcadeInputFrame Read(Gamepad pad, int keyboardPlayer, float dt)
        {
            var k = Keyboard.current;
            bool second = keyboardPlayer == 1;
            bool third = keyboardPlayer == 2;
            bool left = Held(third ? k?.jKey : second ? k?.aKey : k?.leftArrowKey);
            bool right = Held(third ? k?.lKey : second ? k?.dKey : k?.rightArrowKey);
            bool up = Held(third ? k?.iKey : second ? k?.wKey : k?.upArrowKey);
            bool down = Held(third ? k?.kKey : second ? k?.sKey : k?.downArrowKey);
            if (pad != null && pad.added) {
                left |= pad.dpad.left.isPressed || pad.leftStick.x.ReadValue() < -.5f;
                right |= pad.dpad.right.isPressed || pad.leftStick.x.ReadValue() > .5f;
                up |= pad.dpad.up.isPressed || pad.leftStick.y.ReadValue() > .5f;
                down |= pad.dpad.down.isPressed || pad.leftStick.y.ReadValue() < -.5f;
            } else pad = null;
            return new ArcadeInputFrame {
                Horizontal = Repeat((right ? 1 : 0) - (left ? 1 : 0), ref lastHorizontal, ref horizontalWait, dt),
                Vertical = Repeat((down ? 1 : 0) - (up ? 1 : 0), ref lastVertical, ref verticalWait, dt),
                Down = down,
                A = Pressed(pad?.buttonSouth) || Pressed(third ? k?.oKey : second ? k?.eKey : k?.xKey) || (keyboardPlayer == 0 && Pressed(k?.enterKey)),
                B = Pressed(pad?.buttonEast) || Pressed(third ? k?.uKey : second ? k?.qKey : k?.zKey),
                Drop = Pressed(pad?.buttonNorth) || Pressed(third ? k?.hKey : second ? k?.fKey : k?.spaceKey),
                Pause = Pressed(pad?.startButton) || (keyboardPlayer == 0 && (Pressed(k?.escapeKey) || Pressed(k?.pKey)))
            };
        }
    }
}
