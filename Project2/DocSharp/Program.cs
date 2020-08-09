/*
 * Kyle Peppe
 * CSE 681 Project 2
 * Used parts of programs I found online
 */
using System;
using System.Windows.Forms;

namespace DocSharp {
    static class Program {
        /// The main entry point for the application
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DocSharp());
        }
    }
}
