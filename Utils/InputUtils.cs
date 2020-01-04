using CodeWalker.Properties;
using SharpDX;
using SharpDX.XInput;
using System;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace CodeWalker
{



    public class InputManager
    {
        public Controller xbcontroller = null;
        public State xbcontrollerstate;
        public State xbcontrollerstateprev;
        public Vector4 xbmainaxes = Vector4.Zero;
        public Vector4 xbmainaxesprev = Vector4.Zero;
        public Vector2 xbtrigs = Vector2.Zero;
        public Vector2 xbtrigsprev = Vector2.Zero;
        public float xbcontrolvelocity = 0.0f;

        public bool xbenable = false;
        public float xblx = 0; //left stick X axis
        public float xbly = 0; //left stick Y axis
        public float xbrx = 0; //right stick X axis
        public float xbry = 0; //right stick Y axis
        public float xblt = 0; //left trigger value
        public float xbrt = 0; //right trigger value





        public volatile bool kbmovefwd = false;
        public volatile bool kbmovebck = false;
        public volatile bool kbmovelft = false;
        public volatile bool kbmovergt = false;
        public volatile bool kbmoveup = false;
        public volatile bool kbmovedn = false;
        public volatile bool kbjump = false;
        public volatile bool kbmoving = false;

        public KeyBindings keyBindings = new KeyBindings(Settings.Default.KeyBindings);

        public bool CtrlPressed = false;
        public bool ShiftPressed = false;




        public void Init()
        {
            xbcontroller = new Controller(UserIndex.One);
            if (!xbcontroller.IsConnected)
            {
                var controllers = new[] { new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };
                foreach (var selectControler in controllers)
                {
                    if (selectControler.IsConnected)
                    {
                        xbcontroller = selectControler;
                        xbcontrollerstate = xbcontroller.GetState();
                        xbcontrollerstateprev = xbcontrollerstate;
                        break;
                    }
                }
            }
            else
            {
                xbcontrollerstate = xbcontroller.GetState();
                xbcontrollerstateprev = xbcontrollerstate;
            }

        }






        public void Update()
        {

            var s = Settings.Default;

            xbenable = (xbcontroller != null) && (xbcontroller.IsConnected);
            xblx = 0; xbly = 0; xbrx = 0; xbry = 0; xblt = 0; xbrt = 0; //input axes

            if (xbenable)
            {
                xbcontrollerstateprev = xbcontrollerstate;
                xbcontrollerstate = xbcontroller.GetState();
                xbmainaxesprev = xbmainaxes;
                xbtrigsprev = xbtrigs;
                xbmainaxes = ControllerMainAxes();
                xbtrigs = ControllerTriggers();
                xblx = xbmainaxes.X;
                xbly = xbmainaxes.Y;
                xbrx = xbmainaxes.Z;
                xbry = xbmainaxes.W;
                xblt = xbtrigs.X;
                xbrt = xbtrigs.Y;
                float lamt = s.XInputLThumbSensitivity;
                float ramt = s.XInputRThumbSensitivity;
                xbly = s.XInputLThumbInvert ? xbly : -xbly;
                xbry = s.XInputRThumbInvert ? xbry : -xbry;
                xblx *= lamt;
                xbly *= lamt;
                xbrx *= ramt;
                xbry *= ramt;
            }

        }



        public void KeyDown(KeyEventArgs e, bool enablemove)
        {
            var k = e.KeyCode;
            CtrlPressed = (e.Modifiers & Keys.Control) > 0;
            ShiftPressed = (e.Modifiers & Keys.Shift) > 0;

            //enablemove = enablemove && (!ctrl);

            //WASD move...
            if (enablemove)
            {
                if (k == keyBindings.MoveForward) kbmovefwd = true;
                if (k == keyBindings.MoveBackward) kbmovebck = true;
                if (k == keyBindings.MoveLeft) kbmovelft = true;
                if (k == keyBindings.MoveRight) kbmovergt = true;
                if (k == keyBindings.MoveUp) kbmoveup = true;
                if (k == keyBindings.MoveDown) kbmovedn = true;
                if (k == keyBindings.Jump) kbjump = true;
            }

            kbmoving = kbmovefwd || kbmovebck || kbmovelft || kbmovergt || kbmoveup || kbmovedn || kbjump;

        }

        public void KeyUp(KeyEventArgs e)
        {
            CtrlPressed = (e.Modifiers & Keys.Control) > 0;
            ShiftPressed = (e.Modifiers & Keys.Shift) > 0;

            var k = e.KeyCode;
            if (k == keyBindings.MoveForward) kbmovefwd = false;
            if (k == keyBindings.MoveBackward) kbmovebck = false;
            if (k == keyBindings.MoveLeft) kbmovelft = false;
            if (k == keyBindings.MoveRight) kbmovergt = false;
            if (k == keyBindings.MoveUp) kbmoveup = false;
            if (k == keyBindings.MoveDown) kbmovedn = false;
            if (k == keyBindings.Jump) kbjump = false;

            kbmoving = kbmovefwd || kbmovebck || kbmovelft || kbmovergt || kbmoveup || kbmovedn || kbjump;

        }

        public void KeyboardStop()
        {
            kbmovefwd = false;
            kbmovebck = false;
            kbmovelft = false;
            kbmovergt = false;
            kbmoveup = false;
            kbmovedn = false;
            kbjump = false;
        }



        public Vector3 KeyboardMoveVec(bool mapview = false)
        {
            Vector3 movevec = Vector3.Zero;
            if (mapview)
            {
                if (kbmovefwd) movevec.Y += 1.0f;
                if (kbmovebck) movevec.Y -= 1.0f;
                if (kbmovelft) movevec.X -= 1.0f;
                if (kbmovergt) movevec.X += 1.0f;
                if (kbmoveup) movevec.Y += 1.0f;
                if (kbmovedn) movevec.Y -= 1.0f;
            }
            else
            {
                if (kbmovefwd) movevec.Z -= 1.0f;
                if (kbmovebck) movevec.Z += 1.0f;
                if (kbmovelft) movevec.X -= 1.0f;
                if (kbmovergt) movevec.X += 1.0f;
                if (kbmoveup) movevec.Y += 1.0f;
                if (kbmovedn) movevec.Y -= 1.0f;
            }
            return movevec;
        }



        public Vector4 ControllerMainAxes()
        {
            var gp = xbcontrollerstate.Gamepad;
            var ldz = Gamepad.LeftThumbDeadZone;
            var rdz = Gamepad.RightThumbDeadZone;
            float ltnrng = -(short.MinValue + ldz);
            float ltprng = (short.MaxValue - ldz);
            float rtnrng = -(short.MinValue + rdz);
            float rtprng = (short.MaxValue - rdz);

            float lx = (gp.LeftThumbX < 0) ? Math.Min((gp.LeftThumbX + ldz) / ltnrng, 0) :
                       (gp.LeftThumbX > 0) ? Math.Max((gp.LeftThumbX - ldz) / ltprng, 0) : 0;
            float ly = (gp.LeftThumbY < 0) ? Math.Min((gp.LeftThumbY + ldz) / ltnrng, 0) :
                       (gp.LeftThumbY > 0) ? Math.Max((gp.LeftThumbY - ldz) / ltprng, 0) : 0;
            float rx = (gp.RightThumbX < 0) ? Math.Min((gp.RightThumbX + rdz) / rtnrng, 0) :
                       (gp.RightThumbX > 0) ? Math.Max((gp.RightThumbX - rdz) / rtprng, 0) : 0;
            float ry = (gp.RightThumbY < 0) ? Math.Min((gp.RightThumbY + rdz) / rtnrng, 0) :
                       (gp.RightThumbY > 0) ? Math.Max((gp.RightThumbY - rdz) / rtprng, 0) : 0;

            return new Vector4(lx, ly, rx, ry);
        }
        public Vector2 ControllerTriggers()
        {
            var gp = xbcontrollerstate.Gamepad;
            var tt = Gamepad.TriggerThreshold;
            float trng = byte.MaxValue - tt;
            float lt = Math.Max((gp.LeftTrigger - tt) / trng, 0);
            float rt = Math.Max((gp.RightTrigger - tt) / trng, 0);
            return new Vector2(lt, rt);
        }
        public bool ControllerButtonPressed(GamepadButtonFlags b)
        {
            return ((xbcontrollerstate.Gamepad.Buttons & b) != 0);
        }
        public bool ControllerButtonJustPressed(GamepadButtonFlags b)
        {
            return (((xbcontrollerstate.Gamepad.Buttons & b) != 0) && ((xbcontrollerstateprev.Gamepad.Buttons & b) == 0));
        }


    }




    public class KeyBindings
    {
        public Keys MoveForward = Keys.W;
        public Keys MoveBackward = Keys.S;
        public Keys MoveLeft = Keys.A;
        public Keys MoveRight = Keys.D;
        public Keys MoveUp = Keys.R;
        public Keys MoveDown = Keys.F;
        public Keys MoveSlowerZoomIn = Keys.Z;
        public Keys MoveFasterZoomOut = Keys.X;
        public Keys ToggleMouseSelect = Keys.C;
        public Keys ToggleToolbar = Keys.T;
        public Keys ExitEditMode = Keys.Q;
        public Keys EditPosition = Keys.W;
        public Keys EditRotation = Keys.E;
        public Keys EditScale = Keys.R;
        public Keys Jump = Keys.Space; //for control mode
        public Keys FirstPerson = Keys.P;

        public KeyBindings(StringCollection sc)
        {
            foreach (string s in sc)
            {
                string[] parts = s.Split(':');
                if (parts.Length == 2)
                {
                    string sval = parts[1].Trim();
                    Keys k = (Keys)Enum.Parse(typeof(Keys), sval);
                    SetBinding(parts[0], k);
                }
            }
        }

        public void SetBinding(string name, Keys k)
        {
            switch (name)
            {
                case "Move Forwards": MoveForward = k; break;
                case "Move Backwards": MoveBackward = k; break;
                case "Move Left": MoveLeft = k; break;
                case "Move Right": MoveRight = k; break;
                case "Move Up": MoveUp = k; break;
                case "Move Down": MoveDown = k; break;
                case "Move Slower / Zoom In": MoveSlowerZoomIn = k; break;
                case "Move Faster / Zoom Out": MoveFasterZoomOut = k; break;
                case "Toggle Mouse Select": ToggleMouseSelect = k; break;
                case "Toggle Toolbar": ToggleToolbar = k; break;
                case "Exit Edit Mode": ExitEditMode = k; break;
                case "Edit Position": EditPosition = k; break;
                case "Edit Rotation": EditRotation = k; break;
                case "Edit Scale": EditScale = k; break;
                case "First Person Mode": FirstPerson = k; break;
            }
        }

        public StringCollection GetSetting()
        {
            StringCollection sc = new StringCollection();
            sc.Add(GetSettingItem("Move Forwards", MoveForward));
            sc.Add(GetSettingItem("Move Backwards", MoveBackward));
            sc.Add(GetSettingItem("Move Left", MoveLeft));
            sc.Add(GetSettingItem("Move Right", MoveRight));
            sc.Add(GetSettingItem("Move Up", MoveUp));
            sc.Add(GetSettingItem("Move Down", MoveDown));
            sc.Add(GetSettingItem("Move Slower / Zoom In", MoveSlowerZoomIn));
            sc.Add(GetSettingItem("Move Faster / Zoom Out", MoveFasterZoomOut));
            sc.Add(GetSettingItem("Toggle Mouse Select", ToggleMouseSelect));
            sc.Add(GetSettingItem("Toggle Toolbar", ToggleToolbar));
            sc.Add(GetSettingItem("Exit Edit Mode", ExitEditMode));
            sc.Add(GetSettingItem("Edit Position", EditPosition));
            sc.Add(GetSettingItem("Edit Rotation", EditRotation));
            sc.Add(GetSettingItem("Edit Scale", EditScale));
            sc.Add(GetSettingItem("First Person Mode", FirstPerson));
            return sc;
        }

        private string GetSettingItem(string name, Keys val)
        {
            return name + ": " + val.ToString();
        }

        public KeyBindings Copy()
        {
            return (KeyBindings)MemberwiseClone();
        }

    }



}