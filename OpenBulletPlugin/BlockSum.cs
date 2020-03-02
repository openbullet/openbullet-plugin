using PluginFramework;
using PluginFramework.Attributes;
using RuriLib;
using RuriLib.LS;
using System;
using System.Collections.Generic;

namespace OpenBulletPlugin
{
    /*
     * ============================================================================================================
     * 
     * -- INTRODUCTION --
     * This is a sample block plugin that will appear inside the Add Block section of OpenBullet's Stacker.
     * Make sure to read all comments to understand what everything is doing, and feel free to experiment with it.
     * 
     * For more information about building and dependencies, please refer to the comments in the ASimpleForm.cs class.
     * 
     * ============================================================================================================
     */

    /*
     * Each class will make a separate block plugin, so a single DLL can contain multiple block plugins inside.
     * For a block plugin, you must inherit from BlockBase and implement the IBlockPlugin interface.
     */
    public class BlockSum : BlockBase, IBlockPlugin
    {
        // The default label of the block when it's added
        public string Name => "SUM";

        /* The color of the block. This must contain a string with one of the following color names
         * http://www.flounder.com/csharp_color_table.htm
         */
        public string Color => "Cyan";

        /*
         * If this is true, the label of the block will be displayed with a light font instead of a dark one.
         * Use it when your background color is very dark, in order to increase contrast.
         */
        public bool LightForeground => false;

        /*
         * Please refer to the comments in the ASimpleForm.cs class for an overview of attributes and properties.
         */
        [Text("First Number", "The first operand")]
        public string First { get; set; } = "1";

        [Text("Second Number", "The second operand")]
        public string Second { get; set; } = "2";

        [Text("Variable Name", "The output variable name")]
        public string VariableName { get; set; } = "";

        [Checkbox("Is Capture", "Should the output variable be marked as capture?")]
        public bool IsCapture { get; set; } = false;

        /*
         * This is the constructor of the block, it MUST accept ZERO arguments.
         */
        public BlockSum()
        {
            Label = Name;
        }

        /*
         * This method will read and build the block from a LoliScript statement.
         * You can take a look at similar methods from other existing blocks in the original OpenBullet source to see how to
         * do it, but I will try to explain everything that is done here so that even new coders can understand easily.
         * 
         * The FromLS method will override the default FromLS method of the abstract BlockBase class and will accept as argument
         * the single LoliScript directive that specifies the block and its options (as you know in LoliScript each line is a separate statement).
         */
        public override BlockBase FromLS(string line)
        {
            // We trim the input in case the user added any spaces at the start or at the end.
            var input = line.Trim();

            /* We parse the label of the block, for example if someone types
             * #MYLABEL SUM ...
             * it means that the SUM block has a label named 'MYLABEL'.
             * So we need to parse this value in order to correctly show it in the stacker.
             */
            if (input.StartsWith("#")) // If the input actually has a label
                Label = LineParser.ParseLabel(ref input); // Parse the label and remove it from the original string

            /*
             * The syntax of this very simple block is the following (square brackets mean 'optional'):
             * SUM "FIRST" "SECOND" [-> VAR/CAP "NAME"]
             * */

            /*
             * A literal is an argument between double quotes, which usually represents a string.
             * When we parse a literal, the LineParser will make sure that the parameter is double-quoted.
             * 
             * The first parameter is the reference to the input string, while the second is needed to display
             * to the user that a certain parameter is missing from the statement in case it's required.
             */
            First = LineParser.ParseLiteral(ref input, "First Number");
            Second = LineParser.ParseLiteral(ref input, "Second Number");

            /* Try to parse the arrow. Remember that the arrow is optional, a user does not necessarily need to
             * provide an output variable (even though it makes little sense in this example), so we try to parse
             * an arrow token. If the returned value is empty, then it means that it didn't find an arrow and 
             * we need to return the block as-is, without modifying any other property because the input is empty.
             */
            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == "")
                return this;

            /*
             * If we are here, it means that the user specified a certain VAR or CAP to output to, so
             * we need to parse those and set their values inside the block.
             */
            try
            {
                // We parse VAR or CAP, and depending on its value we set IsCapture to true or false. If not specified, throw an exception.
                var varType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (varType.ToUpper() == "VAR" || varType.ToUpper() == "CAP")
                    IsCapture = varType.ToUpper() == "CAP";
            }
            catch { throw new ArgumentException("Invalid or missing variable type"); }

            // We try to parse the VAR or CAP name. If it was not specified, throw an exception.
            try { VariableName = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            // Now we can finally return the block as there is nothing else to parse.
            return this;
        }

        /*
         * This method basically does the opposite of the method above, so it converts a Block from a class to an actual
         * LoliScript statement. It's much easier to write since the BlockWriter was implemented with a fluent pattern.
         */
        public override string ToLS(bool indent = true)
        {
            /*
             * We initialize a new BlockWriter with the given Block type (in this case, GetType() will return BlockSum)
             * and indentation enabled by default when needed (you can type .Indent() anywhere to indent to the next line).
             * The Disabled parameter will tell the writer if the block has been disabled. In that case, it will put a '!'
             * in front of the statement to indicate that it shouldn't be executed by a debugger or runner.
             */
            var writer = new BlockWriter(GetType(), indent, Disabled)
                .Label(Label) // Write the label. If the label is the default one, nothing is written.
                .Token(Name) // Write the block name. This cannot be changed.
                .Literal(First) // Write the 'First' parameter as a literal (it will be double quoted).
                .Literal(Second); // Write the 'Second' parameter as a literal.

            // Check if the VariableName is the default one. If it is, we don't need to write it to the string.
            if (!writer.CheckDefault(VariableName, nameof(VariableName)))
            {
                // Here we reutilize the writer to write more stuff.
                writer
                     .Arrow() // Write the -> arrow.
                     .Token(IsCapture ? "CAP" : "VAR") // Write CAP or VAR depending on IsCapture.
                     .Literal(VariableName); // Write the Variable Name as a literal.
            }

            // Finally we call the ToString() method of the writer to return the statement that will be written to the LoliScript code.
            return writer.ToString();
        }

        /*
         * This method will run whenever a block is processed by a Runner or Debugger.
         * It overrides the same method of the BlockBase abstract class.
         */
        public override void Process(BotData data)
        {
            // Call the Process method of the BlockBase abstract class for any pre-processing needed.
            base.Process(data);

            /* Replace variables in First and Second, for example if a user wrote
             * SUM "<VAR1>" "<VAR2>"
             * we want to replace <VAR1> and <VAR2> with their actual stored values, and then change their type from string to int.
             */
            var first = int.Parse(ReplaceValues(First, data));
            var second = int.Parse(ReplaceValues(Second, data));
            
            // We compute the result as a string (remember that OpenBullet only accepts strings, or list of strings, or dictionaries of strings).
            var result = (first + second).ToString();

            // We create a list of outputs with a single element (this is an old system that needs a rework, sorry).
            var list = new List<string>() { result };

            /* This method will add the variables from the list into OpenBullet. Hover on the name of the method
             * to see what all the parameters mean.
             */
            InsertVariables(data, IsCapture, false, list, VariableName, "", "", false, false);

            /*
             * We use the Log method of data to output information to the debugger log / bot log. If you want to use a
             * different color, you can reference the System.Drawing assembly and pass a LogEntry with the desired color.
             * 
             * Note: do not use data.LogBuffer.Add() since it will not check if the user has disabled bot logging.
             */
            data.Log($"Added {first} and {second} with result {result}");
        }
    }
}
