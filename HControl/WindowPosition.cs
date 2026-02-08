using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;

namespace HControl {
    public class WindowPosition {

        public static WindowPosition getPosition(Window wnd) {
            return new WindowPosition() { Height = wnd.Height, Width = wnd.Width, PositionX = wnd.Position.X, PositionY = wnd.Position.Y };
        }

        public static void setPosition(Window wnd, WindowPosition position) {
            wnd.Height = position.Height;
            wnd.Width = position.Width;
            wnd.Position = new Avalonia.PixelPoint(position.PositionX, position.PositionY);
        }

        public int PositionX { get; set; } = 0;
        public int PositionY { get; set; } = 0;
        public double Width { get; set; } = 0;
        public double Height { get; set; } = 0;

        public WindowPosition() { }

        public WindowPosition(JObject jsonData) {
            PositionX = jsonData.Value<int>(JKEY_POSX);
            PositionY = jsonData.Value<int>(JKEY_POSY);
            Width = jsonData.Value<double>(JKEY_WIDTH);
            Height = jsonData.Value<double>(JKEY_HEIGHT);
        }

        public JObject toJSON() {
            JObject _result = new JObject();

            _result.Add(JKEY_POSX, PositionX);
            _result.Add(JKEY_POSY, PositionY);
            _result.Add(JKEY_WIDTH, Width);
            _result.Add(JKEY_HEIGHT, Height);

            return _result;
        }

        private static readonly string JKEY_POSX = "PositionX";
        private static readonly string JKEY_POSY = "PositionY";
        private static readonly string JKEY_WIDTH = "Width";
        private static readonly string JKEY_HEIGHT = "Height";
    }
}
