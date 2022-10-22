using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace KleadronCommon
{
    public class InputSystem
    {
        KeyboardState keyboardState;
        KeyboardState lastKeyboardState;

        MouseState mouseState;
        MouseState lastMouseState;

        GamePadState[] gamePadStates;
        GamePadState[] lastGamePadStates;

        public Point mousePosition;
        public Point mousePositionUnscaled;
        public Point mouseDelta;

        public bool scaleMouse = false;
        Point scaleResolution;

        Game game;

        public InputSystem(Game game)
        {
            this.game = game;

            gamePadStates = new GamePadState[4];
            lastGamePadStates = new GamePadState[4];

            Update(0, 0);
            Update(0, 0);
        }

        public void SetScaleResolution(int x, int y)
        {
            scaleResolution = new Point(x, y);
        }

        public void Update(float delta, float total)
        {
            lastKeyboardState = keyboardState;
            lastMouseState = mouseState;

            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            for (int i = 0; i < 4; i++)
            {
                lastGamePadStates[i] = gamePadStates[i];

                gamePadStates[i] = GamePad.GetState((PlayerIndex)i, GamePadDeadZone.Circular);
            }

            mousePositionUnscaled = new Point(mouseState.X, mouseState.Y);
            if (scaleMouse)
            {
                Point resolution = new Point(game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
                int scaledX = (int)(((float)mousePositionUnscaled.X / (float)resolution.X) * (float)scaleResolution.X);
                int scaledY = (int)(((float)mousePositionUnscaled.Y / (float)resolution.Y) * (float)scaleResolution.Y);
                mousePosition = new Point(scaledX, scaledY);
            }
            else
            {
                mousePosition = mousePositionUnscaled;
            }
            mouseDelta = new Point(lastMouseState.X - mouseState.X, lastMouseState.Y - mouseState.Y);

            UpdateTextInput(delta, total);
        }

        #region Keyboard

        public bool KeyPressed(Keys key)     { return keyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key); }
        public bool KeyHeld(Keys key)        { return keyboardState.IsKeyDown(key); }
        public bool KeyReleased(Keys key)    { return keyboardState.IsKeyUp(key) && lastKeyboardState.IsKeyDown(key); }

        public bool KeysPressed(params Keys[] keys) 
        {
            bool pressed = false;

            foreach (Keys key in keys)
            {
                if (keyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key))
                    pressed = true;
            }

            return pressed;
        }

        public bool KeysHeld(params Keys[] keys)
        {
            bool pressed = false;

            foreach (Keys key in keys)
            {
                if (keyboardState.IsKeyDown(key))
                    pressed = true;
            }

            return pressed;
        }

        public bool KeysReleased(params Keys[] keys)
        {
            bool pressed = false;

            foreach (Keys key in keys)
            {
                if (keyboardState.IsKeyUp(key) && lastKeyboardState.IsKeyDown(key))
                    pressed = true;
            }

            return pressed;
        }

        #endregion Keyboard

        #region Mouse

        public bool LMB_Pressed()    { return mouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton  == ButtonState.Released; }
        public bool LMB_Held()       { return mouseState.LeftButton == ButtonState.Pressed; }
        public bool LMB_Released()   { return mouseState.LeftButton == ButtonState.Released && lastMouseState.LeftButton == ButtonState.Pressed; }

        public bool MMB_Pressed()    { return mouseState.MiddleButton == ButtonState.Pressed && lastMouseState.MiddleButton == ButtonState.Released; }
        public bool MMB_Held()       { return mouseState.MiddleButton == ButtonState.Pressed; }
        public bool MMB_Released()   { return mouseState.MiddleButton == ButtonState.Released && lastMouseState.MiddleButton == ButtonState.Pressed; }

        public bool RMB_Pressed()    { return mouseState.RightButton == ButtonState.Pressed && lastMouseState.RightButton == ButtonState.Released; }
        public bool RMB_Held()       { return mouseState.RightButton == ButtonState.Pressed; }
        public bool RMB_Released()   { return mouseState.RightButton == ButtonState.Released && lastMouseState.RightButton == ButtonState.Pressed; }

        public void SetMousePosition(int x, int y)
        {
            Mouse.SetPosition(x, y);

            // this is dumb but mandatory
            mouseState = new MouseState(x, y, 
                mouseState.ScrollWheelValue, 
                mouseState.LeftButton, 
                mouseState.MiddleButton, 
                mouseState.RightButton, 
                mouseState.XButton2, 
                mouseState.XButton2);
        }

        #endregion Mouse

        #region GamePad

        public bool ButtonPressed(PlayerIndex index, Buttons button)
        {
            return false;
        }

        #endregion GamePad

        #region Text Input

        StringBuilder inputBuffer;

        //Action OnInputConfirm;  // enter
        //Action OnInputCancel;   // escape

        // int = position, bool true = exact position, bool false = position difference
        Action<int, bool> caretPositionUpdated;
        Func<int> caretQuery;

        // thinking of a better way to do this
        float backspaceHoldTime = 0f;
        float backspaceThreshold = 0.02f;
        float backspaceDelay = 0.4f;

        float leftHoldTime = 0f;
        float leftThreshold = 0.02f;
        float leftDelay = 0.4f;

        float rightHoldTime = 0f;
        float rightThreshold = 0.02f;
        float rightDelay = 0.4f;

        float timeSinceModified = 0.0f;

        public float TimeSinceInputBufferModified
        {
            get
            {
                return timeSinceModified;
            }
        }

        // preferably do not run inputs elsewhere while active
        public bool IsInputActive
        {
            get
            {
                return inputBuffer != null;
            }
        }

        public void SetInputBuffer(StringBuilder inputBuffer, Action<int, bool> caretHandler, Func<int> caretQuerier)
        {
            this.inputBuffer = inputBuffer;
            this.caretPositionUpdated = null;
            this.caretPositionUpdated += caretHandler;
            this.caretQuery = null;
            this.caretQuery += caretQuerier;
            timeSinceModified = 0f;
            backspaceHoldTime = 0f;
        }

        public StringBuilder GetInputBuffer()
        {
            return inputBuffer;
        }

        void UpdateTextInput(float delta, float total)
        {
            if (!IsInputActive)
                return;

            timeSinceModified += delta;

            // moved out of TextInputWidget so all forms of text input support pasting
            if (KeyHeld(Keys.LeftControl) && KeyPressed(Keys.V))
            {
                string clipboardText = Misc.GetClipboardText();
                if (inputBuffer.Length + clipboardText.Length > inputBuffer.Capacity)
                    clipboardText = clipboardText.Substring(0, inputBuffer.Capacity - inputBuffer.Length);

                inputBuffer.Insert(caretQuery(), clipboardText);
                caretPositionUpdated.SafeInvoke<int, bool>(clipboardText.Length, false);
                // return so that pressed keys do not interfere
                return;
            }

            if (KeyHeld(Keys.LeftControl) && KeyPressed(Keys.C))
            {
                Misc.SetClipboardText(inputBuffer.ToString());
                // return so that pressed keys do not interfere
                return;
            }

            Keys[] pressedKeys = keyboardState.GetPressedKeys();
            Keys[] lastPressedKeys = lastKeyboardState.GetPressedKeys();

            for (int i = 0; i < pressedKeys.Length; i++)
            {
                bool newKey = true;

                for (int j = 0; j < lastPressedKeys.Length; j++)
                {
                    if (pressedKeys[i] == lastPressedKeys[j])
                    {
                        // contains key
                        newKey = false;
                    }
                }

                // this key is new
                if (newKey)
                {
                    Keys key = pressedKeys[i];

                    ProcessInputKey(key);
                }
            }

            if (KeyHeld(Keys.Back))
            {
                backspaceHoldTime += delta;

                // while loop so slow framerates/update rates doesn't slow down repeat
                while (backspaceHoldTime > backspaceDelay + backspaceThreshold)
                {
                    backspaceHoldTime -= backspaceThreshold;

                    if (inputBuffer.Length > 0)
                    {
                        inputBuffer.Remove(inputBuffer.Length - 1, 1);
                        timeSinceModified = 0f;
                    }
                }
            }
            else
            {
                backspaceHoldTime = 0f;
            }

            if (KeyHeld(Keys.Right))
            {
                rightHoldTime += delta;

                // while loop so slow framerates/update rates doesn't slow down repeat
                while (rightHoldTime > rightDelay + rightThreshold)
                {
                    rightHoldTime -= rightThreshold;

                    caretPositionUpdated.SafeInvoke<int, bool>(1, false);
                }
            }
            else
            {
                rightHoldTime = 0f;
            }

            if (KeyHeld(Keys.Left))
            {
                leftHoldTime += delta;

                // while loop so slow framerates/update rates doesn't slow down repeat
                while (leftHoldTime > leftDelay + leftThreshold)
                {
                    leftHoldTime -= leftThreshold;

                    caretPositionUpdated.SafeInvoke<int, bool>(-1, false);
                }
            }
            else
            {
                leftHoldTime = 0f;
            }
        }

        void ProcessInputKey(Keys key)
        {
            string pressedSet = null;

            // oh no
            switch (key)
            {
                case Keys.D1: pressedSet = "1!"; break;
                case Keys.D2: pressedSet = "2@"; break;
                case Keys.D3: pressedSet = "3#"; break;
                case Keys.D4: pressedSet = "4$"; break;
                case Keys.D5: pressedSet = "5%"; break;
                case Keys.D6: pressedSet = "6^"; break;
                case Keys.D7: pressedSet = "7&"; break;
                case Keys.D8: pressedSet = "8*"; break;
                case Keys.D9: pressedSet = "9("; break;
                case Keys.D0: pressedSet = "0)"; break;

                case Keys.A: pressedSet = "aA"; break;
                case Keys.B: pressedSet = "bB"; break;
                case Keys.C: pressedSet = "cC"; break;
                case Keys.D: pressedSet = "dD"; break;
                case Keys.E: pressedSet = "eE"; break;
                case Keys.F: pressedSet = "fF"; break;
                case Keys.G: pressedSet = "gG"; break;
                case Keys.H: pressedSet = "hH"; break;
                case Keys.I: pressedSet = "iI"; break;
                case Keys.J: pressedSet = "jJ"; break;
                case Keys.K: pressedSet = "kK"; break;
                case Keys.L: pressedSet = "lL"; break;
                case Keys.M: pressedSet = "mM"; break;
                case Keys.N: pressedSet = "nN"; break;
                case Keys.O: pressedSet = "oO"; break;
                case Keys.P: pressedSet = "pP"; break;
                case Keys.Q: pressedSet = "qQ"; break;
                case Keys.R: pressedSet = "rR"; break;
                case Keys.S: pressedSet = "sS"; break;
                case Keys.T: pressedSet = "tT"; break;
                case Keys.U: pressedSet = "uU"; break;
                case Keys.V: pressedSet = "vV"; break;
                case Keys.W: pressedSet = "wW"; break;
                case Keys.X: pressedSet = "xX"; break;
                case Keys.Y: pressedSet = "yY"; break;
                case Keys.Z: pressedSet = "zZ"; break;

                case Keys.Space: pressedSet = "  "; break;

                case Keys.OemTilde: pressedSet = "`~"; break;
                case Keys.OemBackslash: pressedSet = @"\|"; break;
                case Keys.OemPipe: pressedSet = @"\|"; break;
                case Keys.OemCloseBrackets: pressedSet = "]}"; break;
                case Keys.OemOpenBrackets: pressedSet = "[{"; break;
                case Keys.OemComma: pressedSet = ",<"; break;
                case Keys.OemPeriod: pressedSet = ".>"; break;
                case Keys.OemQuestion: pressedSet = "/?"; break;
                case Keys.OemSemicolon: pressedSet = ";:"; break;
                case Keys.OemPlus: pressedSet = "=+"; break;
                case Keys.OemQuotes: pressedSet = "\'\""; break;
            }

            if (pressedSet != null)
            {
                int offset = 0;
                if (KeyHeld(Keys.LeftShift) || KeyHeld(Keys.RightShift))
                    offset = 1;

                if (inputBuffer.Length < inputBuffer.Capacity)
                {
                    inputBuffer.Insert(caretQuery(), pressedSet.Substring(offset, 1));
                    caretPositionUpdated.SafeInvoke<int, bool>(1, false);
                    timeSinceModified = 0f;
                }
            }

            if (KeyPressed(Keys.Back))
            {
                if (inputBuffer.Length > 0)
                {
                    inputBuffer.Remove(caretQuery() - 1, 1);
                    caretPositionUpdated.SafeInvoke<int, bool>(-1, false);
                    timeSinceModified = 0f;
                }
            }

            if (KeyPressed(Keys.Delete))
            {
                if (caretQuery() < inputBuffer.Length)
                {
                    inputBuffer.Remove(caretQuery(), 1);
                    timeSinceModified = 0f;
                }
            }
        }

        public void CloseInputBuffer()
        {
            this.inputBuffer = null;
        }

        #endregion Text Input
    }
}
