using System;
using System.Collections.Generic;
using System.Windows;

namespace TeheManX4
{
    class Layout
    {
        public WindowLayout mainWindowLayout;
        public List<WindowLayout> windowLayouts = new List<WindowLayout>();
        //Screen Viewer
        public double screenLeft;
        public double screenTop;
        public double screenWidth;
        public double screenHeight;
        public int screenState;
        //Screen Flags Window
        public double extraLeft;
        public double extraTop;
        //Files Viewer
        public double fileLeft;
        public double fileTop;
        public double fileWidth;
        public double fileHeight;
        public int fileState;
        //Clut Tools & Picker (and manual clut tpage)
        public double clutLeft;
        public double clutTop;
        public double pickerLeft;
        public double pickerTop;
        public double manualClutLeft;
        public double manualClutTop;
        //TileSet Tools
        public double tileLeft;
        public double tileTop;
        //Auto Edit Text
        public double autoTextLeft;
        public double autoTextTop;
        //Tools Window
        public bool textureToolsOpen;
        public bool soundToolsOpen;
        public bool isoToolsOpen;
        public bool otherToolsOpen;
        //Sprite Window
        public bool is1X;
    }
    class WindowLayout
    {
        public double left;
        public double top;
        public double width;
        public double height;
        public bool max;
        public int windowState;
        public Type type;
        public object child;
    }
    class BranchLayout
    {
        public Type firstItemType;
        public object firstItem;
        public GridLength firstItemLength;

        public Type secondItemType;
        public object secondItem;
        public GridLength secondItemLength;

        public int Orientation;
    }
}
