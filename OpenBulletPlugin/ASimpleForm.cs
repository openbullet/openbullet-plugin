using PluginFramework;
using PluginFramework.Attributes;
using RuriLib;
using RuriLib.Interfaces;
using RuriLib.Models;
using RuriLib.ViewModels;
using System.IO;

namespace OpenBulletPlugin
{
    /*
     * ============================================================================================================
     * 
     * -- INTRODUCTION --
     * This is a sample plugin that will appear inside the Plugins section of OpenBullet.
     * Make sure to read all comments to understand what everything is doing, and feel free to experiment with it.
     * 
     * -- BUILDING --
     * This project must reference RuriLib and PluginFramework, which are available at https://github.com/openbullet/openbullet
     * and are also included in this sample project (but they will not be kept up to date).
     * When you compile this project, it will output a DLL file that you need to place in OpenBullet's Plugins folder,
     * and then restart OpenBullet in order for it to work.
     * 
     * -- DEPENDENCIES --
     * If you want to load any dependencies you will be able to do so by moving all the DLLs into a subfolder with the
     * same name as the master DLL, for example something like this:
     * Plugins/
     * Plugins/Example.dll
     * Plugins/Example/Dependency.dll
     * 
     * Note: If you want to add a dependency that OpenBullet already depends on (e.g. Newtonsoft.Json) you MUST make sure
     * to use the EXACT SAME VERSION that OpenBullet uses when you compile the plugin (it's easy to just copy the DLL file from
     * OpenBullet's folder and reference that instead of the nuget package which might be more updated), and finally you MUST NOT INCLUDE
     * the DLL of that dependency in the subfolder since OpenBullet already loads it by default.
     * 
     * ============================================================================================================
     */

    /*
     * Each class will make a separate entry in the plugins dropdown menu, so a single DLL can contain multiple plugins inside.
     * For a normal plugin, you must implement the IPlugin interface.
     */
    public class ASimpleForm : IPlugin
    {
        // The name of the plugin shown in the dropdown box
        public string Name => "A Simple Form";

        /*
         * This property will be linked to a single-line textbox in the user interface.
         * The property MUST be of type string.
         * The Text attribute is here applied to the property FirstName to specify that it needs to be drawn as a textbox.
         * The attribute accepts 2 parameters:
         * - a label (which will be shown right next to the textbox)
         * - a tooltip (which will be shown on mouse hover) (optional)
         * You can also set a default value for the field, for example in this case it's "John".
         * 
         * ONLY properties with a valid attribute will be drawn on the interface, other properties can still be used in the class for
         * coding purposes but they will be disregarded by the interface builder.
         */
        [Text("First Name", "Your first name... duh")]
        public string FirstName { get; set; } = "John";

        [Text("Last Name", "Your last name... duh")]
        public string LastName { get; set; } = "Doe";

        /*
         * This property will be linked to a numerical up-down control in the user interface.
         * The property MUST be of type int.
         * The Numeric attribute accepts 4 parameters:
         * - a label
         * - a tooltip (optional)
         * - the minimum acceptable value
         * - the maximum acceptable value
         */
        [Numeric("Age", minimum = 0, maximum = 100)]
        public int Age { get; set; } = 18;

        /*
         * This property will be linked to a checkbox in the user interface.
         * The property MUST be of type bool.
         * The Checkbox attribute only accepts a label and optional tooltip.
         */
        [Checkbox("Are you tall?")]
        public bool Tall { get; set; } = true;

        /*
         * This property will be linked to a multi-line textbox in the user interface.
         * The property MUST be of type string[].
         * When the user inputs values in the textbox, they will be returned in an array where each element is a line of the textbox.
         * The TextMulti attribute only accepts a label and an optional tooltip.
         */
        [TextMulti("Interests", "Type one interest per line")]
        public string[] Interests { get; set; } = new string[] { "Soccer", "Jogging", "Fishing" };

        /*
         * This property will be linked to an Open File Dialogue (allows to select a file from disk) in the user interface.
         * The property MUST be of type string and will contain the full path to the file chosen by the user.
         * The FilePicker attribute only accepts a label and an optional tooltip.
         */
        [FilePicker("File to open", "The file you want to read")]
        public string FileToRead { get; set; }

        /*
         * This property will be linked to a Dropdown Box in the user interface.
         * The property MUST be of type string and it will contain the option chosen by the user.
         * The Dropdown attribute accepts a label and optional tooltip, and a list of options for the dropdown.
         * You can specify the option to select at the beginning via the default property value.
         */
        [Dropdown("Order", "What would you like to drink?", options = new string[] { "Coke", "Pepsi", "Sprite", "Fanta" })]
        public string Order { get; set; } = "Coke";

        /*
         * This property will be linked to a Wordlist Picker Dialogue in the user interface.
         * The property MUST be of type Wordlist and it will contain the Wordlist model chosen by the user.
         * The WordlistPicker attribute only accepts a label and an optional tooltip.
         */
        [WordlistPicker("Wordlist", "The wordlist you want to choose")]
        public Wordlist Wordlist { get; set; }

        /*
         * This property will be linked to a Config Picker Dialogue in the user interface.
         * The property MUST be of type ConfigViewModel and it will contain the viewmodel of the config chosen by the user.
         * The ConfigPicker attribute only accepts a label and an optional tooltip.
         */
        [ConfigPicker("Config", "The config you want to choose")]
        public ConfigViewModel Config { get; set; }

        /*
         * This method will be linked to a Button in the user interface and will be executed whenever the user pressed the button.
         * The method MUST be of type void and accept one parameter of type IApplication.
         * 
         * IApplication is a group of interfaces that allow the plugin to talk to most of OpenBullet's components, such as Runners,
         * the Logger or the Config Manager. This is where the power of the plugin system actually resides, and it allows you to provide
         * additional functionalities to OpenBullet without the need to modify the original source code.
         * 
         * ONLY methods with a valid attribute will be linked to buttons on the interface, other methods can still be used in the class for
         * coding purposes but they will be disregarded by the interface builder.
         */
        [Button("Show Message Box")]
        public void Execute(IApplication app)
        {
            var isTall = Tall ? "" : " not";
            var interests = string.Join(", ", Interests);
            var fileContent = string.IsNullOrEmpty(FileToRead) ? "INVALID FILE" : File.ReadAllText(FileToRead);
            var wordlistLines = Wordlist == null ? 0 : Wordlist.Total;
            var configBlocks = Config == null ? 0 : Config.Config.BlocksAmount;

            /*
             * The Log method of the Logger will log a message to the system log with the specified level, and in this case
             * it will also be prompted to the user through a MessageBox.
             */
            app.Logger.Log($@"Hello {FirstName} {LastName} you're {Age} years old and you're{isTall} tall. You like {interests}.
The content of the file is {fileContent}.
You would like to order some {Order}.
Your wordlist has {wordlistLines} lines and your config has {configBlocks} blocks.", LogLevel.Info, true);
        }
    }
}
