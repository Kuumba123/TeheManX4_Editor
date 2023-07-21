using System.Windows;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        readonly string[] messages = new string[]
        {
            //0
            "Before you can start level editing you need to extract the game files. " +
            "You can do this by clicking on tools then clicking on the extract ISO 9960 button. " +
            "Make sure you all tracks are in a single file before you use that extractor! " +
            "You can also switch between Stage Files via F1 & F2 " +
            "along with that switch between tabs via CTRL + Left/Right and " +
            "Full-Screen via F11. " +
            "This Editor also supports real time level editing via PCSX Redux or NOPS. " +
            "For Redux make sure you  have the web server enabled. " +
            "If you want to use a real PS1 for the level editing then make sure you have foldering location of NOPS setup in your " +
            "envirment variables. " +
            "After that just click Reload (or CTRL + R) and watch the magic happen! Also make sure you have the right player selected",
            //1
            "This is where you can edit what screens go where (20hex width 20hex height). " +
            "You can move around by using W A S D . " +
            "Right Click = Copy , Left Click = Paste and " +
            "Hold Shift + Right Click = Selecting the Clicked screen in the Screen Tab. " +
            "Hold Shift + Left Click = Manually setting the screen Id. " +
            "Lastly if you press the Delete Key (Num Pad) it will delete the selected screen!",
            //2
            "This is where you can edit what tiles make up a screen , Left Click = Paste , Right Click = Copy . Clicking the flag icon will " +
            "bring up the tile flags window. " +
            "Right Click = Remove Flag , Left Click = Enable Flag." +
            "Clicking the toggle button will switch between editing Trasnperancy (BLUE) or Priority (RED). " +
            "for the 16x16 tab is mostly the same however you can set the texture location by just clicking on it. " +
            "Use your mouse wheel to cycle through the Clut (mouse must be above textures)",
            //3
            "Wont go too much in detail about this tab , this is where you can place enemies and stuff. " +
            "Keep in mind that there's also start enemies (Tools) so if want to lets just say disable the clut anime " +
            "the start enemies is likley what your gonna be mess",
            //4
            "Its kinda hard to really explain how/what this tab is for unless you just watch the video but " +
            "in each level to save CPU RAM + Speed-Up loads the game ony keeps the required SPRT Frames (the info that tells the game how to peace the textures into something lookable) " +
            "and just keeps copying them across the all the other stages. This tool can extract and re-add in that SPRT data so that you don't get a glitchy " +
            "mess when you change the enemies in a stage (just watch the video)",
            //5
            "This is where you can edit the width and height of the level layout , if the width/height is less than  what it orignally was " +
            $"the right and bottom of layout will be trimmed off. The max size of the layout data is {Const.MaxLayoutSize} " +
            $"however that is for all the levels combined so if you need extra layout space just trim off some other level. " +
            $"(Some of the sub-levels dont use there entire layout so it would be a good choice to trim those) " +
            $"The total size label tells you the amount of space the level uses (in CPU's RAM). " +
            $"It does not include the CLUT but it does include the backup screen data. " +
            $"The total enemies count does not include the start enemies and " +
            $"lastly you can also edit the amount of 16x16 tiles and the amount of screens per level. " +
            $"there is no real limet you have to worry about while editing these to parameters , just make sure " +
            $"you rebuild the disc before messing with the real-time level editing (Reload button)",
            //6
            "This is where you can edit the Clut Animes , everything for the most part is pretty simple just keep in mind that " +
            "the if the timer value is less than 0 (its being shown as hex but treat it as signed) it means to be go back " +
            "that about of frames."
        };
        public HelpWindow(int msgId)
        {
            InitializeComponent();
            box.Text = messages[msgId];
        }
    }
}
